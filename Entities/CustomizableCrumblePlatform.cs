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
    class CustomizableCrumblePlatform : CrumblePlatform {
        private static ILHook crumblePlatformOrigAddedHook = null;

        public static void Load() {
            crumblePlatformOrigAddedHook = new ILHook(typeof(CrumblePlatform).GetMethod("orig_Added"), onCrumblePlatformAdded);
        }

        public static void Unload() {
            crumblePlatformOrigAddedHook?.Dispose();
            crumblePlatformOrigAddedHook = null;
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

        private HashSet<CustomizableCrumblePlatform> groupedCrumblePlatforms = new HashSet<CustomizableCrumblePlatform>();

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
        }

        private void addRange(HashSet<CustomizableCrumblePlatform> set, IEnumerable<CustomizableCrumblePlatform> elements) {
            foreach (CustomizableCrumblePlatform element in elements) {
                set.Add(element);
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // add ourselves to our group.
            groupedCrumblePlatforms.Add(this);

            if (grouped) {
                // get surrounding grouped crumble blocks: above, below, on left and on right.
                addRange(groupedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position + Vector2.UnitX).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(groupedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position - Vector2.UnitX).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(groupedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position + Vector2.UnitY).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));
                addRange(groupedCrumblePlatforms, CollideAll<CustomizableCrumblePlatform>(Position - Vector2.UnitY).OfType<CustomizableCrumblePlatform>().Where(p => p.grouped));

                // share the set of platforms in the group with the group.
                foreach (CustomizableCrumblePlatform platform in new HashSet<CustomizableCrumblePlatform>(groupedCrumblePlatforms)) {
                    addRange(groupedCrumblePlatforms, platform.groupedCrumblePlatforms);
                    platform.groupedCrumblePlatforms = groupedCrumblePlatforms;
                }
            }
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
                    triggeredPlatform = getOnePlatformWithPlayerOnTop();
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
            foreach (CustomizableCrumblePlatform platform in groupedCrumblePlatforms) {
                if (platform.GetPlayerOnTop() != null) {
                    return platform;
                }
            }
            return null;
        }

        private CustomizableCrumblePlatform getOnePlatformWithPlayerClimbing() {
            foreach (CustomizableCrumblePlatform platform in groupedCrumblePlatforms) {
                if (platform.GetPlayerClimbing() != null) {
                    return platform;
                }
            }
            return null;
        }

        private bool isGroupCollidingWithSomething() {
            foreach (CustomizableCrumblePlatform platform in groupedCrumblePlatforms) {
                if (platform.CollideCheck<Actor>() || platform.CollideCheck<Solid>()) {
                    return true;
                }
            }
            return false;
        }
    }
}
