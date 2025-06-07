using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableCrumblePlatform")]
    [Tracked]
    public class CustomizableCrumblePlatform : CrumblePlatform {
        private enum FlagMode { None, UntilPlatformRespawn, UntilDeathOrRoomChange, Permanent }

        private static ILHook crumblePlatformOrigAddedHook = null;
        private static ILHook crumblePlatformTileOutHook = null;

        public static void Load() {
            crumblePlatformOrigAddedHook = new ILHook(typeof(CrumblePlatform).GetMethod("orig_Added"), onCrumblePlatformAdded);
            crumblePlatformTileOutHook = new ILHook(crumblePlatformTileOut.GetStateMachineTarget(), onCrumblePlatformTileOut);
        }

        public static void Unload() {
            crumblePlatformOrigAddedHook?.Dispose();
            crumblePlatformOrigAddedHook = null;

            crumblePlatformTileOutHook?.Dispose();
            crumblePlatformTileOutHook = null;
        }

        private static MethodInfo crumblePlatformOutlineFade = typeof(CrumblePlatform).GetMethod("OutlineFade", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo crumblePlatformTileOut = typeof(CrumblePlatform).GetMethod("TileOut", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo crumblePlatformTileIn = typeof(CrumblePlatform).GetMethod("TileIn", BindingFlags.NonPublic | BindingFlags.Instance);

        // private variables made accessible through DynData
        private Coroutine outlineFader;
        private List<Coroutine> falls;
        private List<int> fallOrder;
        private ShakerList shaker;
        private LightOcclude occluder;
        private List<Image> images;

        private string outlineTexture;
        private bool oneUse;
        private float respawnDelay;
        private float minCrumbleDurationOnTop;
        private float maxCrumbleDurationOnTop;
        private float crumbleDurationOnSide;
        private bool grouped;
        private bool onlyEmitSoundForPlayer;
        private Color fadeOutTint;
        private bool attachStaticMovers;
        private string flag;
        private bool setFlagOnPlayerContact;
        private FlagMode flagMode;
        private bool flagInverted;
        private float lightBlockingOpacity;

        // the former one contains the direct neighbors, the latter contains the entire group
        private HashSet<CustomizableCrumblePlatform> linkedCrumblePlatforms = new HashSet<CustomizableCrumblePlatform>();
        private List<CustomizableCrumblePlatform> groupMembers;

        // values cached and shared with group members to avoid doing the same collide checks multiple times on the same frame
        private class BuddyCache {
            public class Result<T> {
                public T result;
                public bool computed;

                public void Reset() {
                    result = default;
                    computed = false;
                }
            }

            public readonly Result<CustomizableCrumblePlatform> getOnePlatformWithPlayerOnTop = new Result<CustomizableCrumblePlatform>();
            public readonly Result<CustomizableCrumblePlatform> getOnePlatformWithPlayerClimbing = new Result<CustomizableCrumblePlatform>();
            public readonly Result<bool> isGroupCollidingWithSomething = new Result<bool>();
        }

        private readonly BuddyCache buddyCache = new BuddyCache();

        public CustomizableCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) {
            OverrideTexture = data.Attr("texture", null);
            outlineTexture = data.Attr("outlineTexture", "objects/crumbleBlock/outline");
            oneUse = data.Bool("oneUse", false);
            respawnDelay = data.Float("respawnDelay", 2f);
            minCrumbleDurationOnTop = data.Float("minCrumbleDurationOnTop", 0.2f);
            maxCrumbleDurationOnTop = data.Float("maxCrumbleDurationOnTop", 0.6f);
            crumbleDurationOnSide = data.Float("crumbleDurationOnSide", 1f);
            grouped = data.Bool("grouped", false);
            onlyEmitSoundForPlayer = data.Bool("onlyEmitSoundForPlayer", false);
            fadeOutTint = data.HexColor("fadeOutTint", Color.Gray);
            attachStaticMovers = data.Bool("attachStaticMovers", false);
            flag = data.Attr("flag");
            setFlagOnPlayerContact = data.Bool("setFlagOnPlayerContact", false);
            flagMode = data.Enum("flagMode", defaultValue: FlagMode.None);
            flagInverted = data.Bool("flagInverted");
            lightBlockingOpacity = data.Float("lightBlockingOpacity", 0.2f);
        }

        private static void onCrumblePlatformAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("objects/crumbleBlock/outline"))) {
                Logger.Log("MaxHelpingHand/CustomizableCrumblePlatform", $"Modding crumble platform outline texture at {cursor.Index} in IL for CrumblePlatform.orig_Added");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, CrumblePlatform, string>>((orig, self) => {
                    if (self is CustomizableCrumblePlatform customPlatform) {
                        return customPlatform.outlineTexture;
                    }
                    return orig;
                });
            }
        }

        private static void onCrumblePlatformTileOut(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_Gray"))) {
                Logger.Log("MaxHelpingHand/CustomizableCrumblePlatform", $"Modding crumble platform tile out tinting at {cursor.Index} in IL for CrumblePlatform.TileOut");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, crumblePlatformTileOut.GetStateMachineTarget().DeclaringType.GetField("img"));
                cursor.EmitDelegate<Func<Color, Image, Color>>((orig, self) => {
                    if (self.Entity is CustomizableCrumblePlatform customPlatform) {
                        return customPlatform.fadeOutTint;
                    }
                    return orig;
                });
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            DynData<CrumblePlatform> self = new DynData<CrumblePlatform>(this);
            outlineFader = self.Get<Coroutine>("outlineFader");
            falls = self.Get<List<Coroutine>>("falls");
            fallOrder = self.Get<List<int>>("fallOrder");
            shaker = self.Get<ShakerList>("shaker");
            occluder = self.Get<LightOcclude>("occluder");
            images = self.Get<List<Image>>("images");

            foreach (Component component in this) {
                if (component is Coroutine coroutine && coroutine != outlineFader && !falls.Contains(coroutine)) {
                    // this coroutine is the sequence! hijack it
                    coroutine.RemoveSelf();
                    Add(new Coroutine(customSequence()));
                    break;
                }
            }

            if (oneUse) {
                // prevent the outline from appearing.
                outlineFader.RemoveSelf();
            }

            occluder.Alpha = lightBlockingOpacity;
        }

        private void addRange(HashSet<CustomizableCrumblePlatform> set, IEnumerable<CustomizableCrumblePlatform> elements) {
            foreach (CustomizableCrumblePlatform element in elements) {
                set.Add(element);
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (grouped) {
                // get surrounding grouped crumble blocks: above, below, on left and on right.
                addRange(linkedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position + Vector2.UnitX).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(linkedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position - Vector2.UnitX).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(linkedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position + Vector2.UnitY).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(linkedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position - Vector2.UnitY).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
            }
        }

        public override void Update() {
            if (groupMembers == null) {
                groupMembers = new List<CustomizableCrumblePlatform>();
                IEnumerator<CustomizableCrumblePlatform> crumblePlatformTreeWalker = walkThroughCrumblePlatformTree();
                while (crumblePlatformTreeWalker.MoveNext()) groupMembers.Add(crumblePlatformTreeWalker.Current);
                foreach (CustomizableCrumblePlatform groupMember in groupMembers) groupMember.groupMembers = groupMembers;
                Logger.Log(LogLevel.Verbose, "MaxHelpingHand/CustomizableCrumblePlatform", $"Computed crumble platform group w/ {groupMembers.Count} members");
            }

            base.Update();

            // the buddy cache will be obsolete by the next frame
            buddyCache.getOnePlatformWithPlayerOnTop.Reset();
            buddyCache.getOnePlatformWithPlayerClimbing.Reset();
            buddyCache.isGroupCollidingWithSomething.Reset();
        }

        private IEnumerator customSequence() {
            while (true) {
                // wait until player is on top
                CustomizableCrumblePlatform triggeredPlatform = getOnePlatformWithPlayerOnTop();
                bool onTop;
                if (triggeredPlatform != null) {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                } else {
                    triggeredPlatform = getOnePlatformWithPlayerClimbing();
                    if (triggeredPlatform == null) {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                // only emit sound if we should emit sound no matter what, or if the current platform is the one that got triggered.
                if (!onlyEmitSoundForPlayer || triggeredPlatform == this) {
                    Audio.Play("event:/game/general/platform_disintegrate", Center);
                }

                // the player touched the platform!
                if (flagMode != FlagMode.None && setFlagOnPlayerContact) {
                    SceneAs<Level>().Session.SetFlag(flag, !flagInverted);
                }

                // make pieces shake and emit particles
                shaker.ShakeFor(onTop ? maxCrumbleDurationOnTop : crumbleDurationOnSide, removeOnFinish: false);
                foreach (Image image in images) {
                    SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                float targetTime = (onTop ? minCrumbleDurationOnTop : maxCrumbleDurationOnTop);
                for (float time = 0f; time < targetTime; time += 0.2f) {
                    yield return Math.Min(0.2f, targetTime - time);
                    foreach (Image image in images) {
                        SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }

                // wait for a bit more
                float timer = (onTop ? maxCrumbleDurationOnTop - minCrumbleDurationOnTop : crumbleDurationOnSide - maxCrumbleDurationOnTop);
                if (onTop) {
                    while (timer > 0f && getOnePlatformWithPlayerOnTop() != null) {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                } else {
                    while (timer > 0f) {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }

                // make the platform disappear
                outlineFader.Replace((IEnumerator) crumblePlatformOutlineFade.Invoke(this, new object[] { 1f }));
                occluder.Visible = false;
                Collidable = false;
                if (attachStaticMovers) {
                    DisableStaticMovers();
                }
                if (flagMode != FlagMode.None && !setFlagOnPlayerContact) {
                    SceneAs<Level>().Session.SetFlag(flag, !flagInverted);
                }
                float delay = 0.05f;
                for (int m = 0; m < 4; m++) {
                    for (int i = 0; i < images.Count; i++) {
                        if (i % 4 - m == 0) {
                            falls[i].Replace((IEnumerator) crumblePlatformTileOut.Invoke(this, new object[] { images[fallOrder[i]], delay * (float) m }));
                        }
                    }
                }

                if (oneUse) {
                    // wait for the platform to animate for a bit (1 second) then delete it.
                    yield return 1f;
                    RemoveSelf();
                    Scene.OnEndOfFrame += () => {
                        if (groupMembers == null) return; // seems like another member already took care of it.
                        List<CustomizableCrumblePlatform> platforms = groupMembers;
                        Logger.Log(LogLevel.Debug, "MaxHelpingHand/CustomizableCrumblePlatform", $"A block was removed: invalidating group for {platforms.Count} crumble blocks!");
                        foreach (CustomizableCrumblePlatform platform in platforms) platform.groupMembers = null;
                    };
                    yield break;
                } else {
                    // wait for the custom delay instead of 2 seconds.
                    yield return respawnDelay;
                }

                // wait if something is where the platform is supposed to respawn
                while (CollideCheck<Actor>() || CollideCheck<Solid>() || isGroupCollidingWithSomething()) {
                    yield return null;
                }

                // make the platform reappear
                outlineFader.Replace((IEnumerator) crumblePlatformOutlineFade.Invoke(this, new object[] { 0f }));
                occluder.Visible = true;
                Collidable = true;
                if (attachStaticMovers) {
                    EnableStaticMovers();
                }
                if (flagMode == FlagMode.UntilPlatformRespawn) {
                    SceneAs<Level>().Session.SetFlag(flag, flagInverted);
                }
                for (int m = 0; m < 4; m++) {
                    for (int i = 0; i < images.Count; i++) {
                        if (i % 4 - m == 0) {
                            // only emit sound if we should emit sound no matter what, or if the current platform is the one that got triggered.
                            if (!onlyEmitSoundForPlayer || triggeredPlatform == this) {
                                falls[i].Replace((IEnumerator) crumblePlatformTileIn.Invoke(this, new object[] { i, images[fallOrder[i]], 0.05f * (float) m }));
                            } else {
                                falls[i].Replace(tileInNoSound(i, images[fallOrder[i]], 0.05f * (float) m));
                            }
                        }
                    }
                }
            }
        }

        public override void Removed(Scene scene) {
            if (flagMode == FlagMode.UntilPlatformRespawn || flagMode == FlagMode.UntilDeathOrRoomChange) {
                SceneAs<Level>().Session.SetFlag(flag, flagInverted);
            }

            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene) {
            if (flagMode == FlagMode.UntilPlatformRespawn || flagMode == FlagMode.UntilDeathOrRoomChange) {
                SceneAs<Level>().Session.SetFlag(flag, flagInverted);
            }

            base.SceneEnd(scene);
        }

        // copy-paste of the vanilla TileIn method, except without the sound.
        private IEnumerator tileInNoSound(int index, Image img, float delay) {
            yield return delay;
            img.Visible = true;
            img.Color = Color.White;
            img.Position = new Vector2(index * 8 + 4, 4f);
            for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f) {
                yield return null;
                img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
            }
            img.Scale = Vector2.One;
        }

        private CustomizableCrumblePlatform getOnePlatformWithPlayerOnTop() {
            return getBuddyCached(cache => cache.getOnePlatformWithPlayerOnTop, () => {
                foreach (CustomizableCrumblePlatform platform in groupMembers) {
                    if (platform.GetPlayerOnTop() != null) {
                        return platform;
                    }
                }
                return null;
            });
        }

        private CustomizableCrumblePlatform getOnePlatformWithPlayerClimbing() {
            return getBuddyCached(cache => cache.getOnePlatformWithPlayerClimbing, () => {
                foreach (CustomizableCrumblePlatform platform in groupMembers) {
                    if (platform.GetPlayerClimbing() != null) {
                        return platform;
                    }
                }
                return null;
            });
        }

        private bool isGroupCollidingWithSomething() {
            return getBuddyCached(cache => cache.isGroupCollidingWithSomething, () => {
                foreach (CustomizableCrumblePlatform platform in groupMembers) {
                    if (platform.CollideCheck<Actor>() || platform.CollideCheck<Solid>()) {
                        return true;
                    }
                }
                return false;
            });
        }

        private T getBuddyCached<T>(Func<BuddyCache, BuddyCache.Result<T>> buddyCacheGetter, Func<T> compute) {
            BuddyCache.Result<T> myResult = buddyCacheGetter(buddyCache);
            if (myResult.computed) return myResult.result;

            // compute and tell our buddies about it
            T result = compute();
            foreach (CustomizableCrumblePlatform platform in groupMembers) {
                BuddyCache.Result<T> buddyResult = buddyCacheGetter(platform.buddyCache);
                buddyResult.computed = true;
                buddyResult.result = result;
            }
            return result;
        }

        private IEnumerator<CustomizableCrumblePlatform> walkThroughCrumblePlatformTree() {
            HashSet<CustomizableCrumblePlatform> alreadyHandled = new HashSet<CustomizableCrumblePlatform>();
            Queue<CustomizableCrumblePlatform> toHandle = new Queue<CustomizableCrumblePlatform>();
            toHandle.Enqueue(this);

            while (toHandle.Count > 0) {
                CustomizableCrumblePlatform processing = toHandle.Dequeue();
                yield return processing;
                alreadyHandled.Add(processing);

                foreach (CustomizableCrumblePlatform platform in processing.linkedCrumblePlatforms) {
                    if (platform.Scene != null && !alreadyHandled.Contains(platform)) {
                        toHandle.Enqueue(platform);
                    }
                }
            }
        }
    }
}
