using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
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

        private bool oneUse;
        private float respawnDelay;
        private float minCrumbleDurationOnTop;
        private float maxCrumbleDurationOnTop;
        private float crumbleDurationOnSide;
        private bool grouped;

        private HashSet<CustomizableCrumblePlatform> groupedCrumblePlatforms = new HashSet<CustomizableCrumblePlatform>();

        public CustomizableCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) {
            OverrideTexture = data.Attr("texture", null);
            oneUse = data.Bool("oneUse", false);
            respawnDelay = data.Float("respawnDelay", 2f);
            minCrumbleDurationOnTop = data.Float("minCrumbleDurationOnTop", 0.2f);
            maxCrumbleDurationOnTop = data.Float("maxCrumbleDurationOnTop", 0.6f);
            crumbleDurationOnSide = data.Float("crumbleDurationOnSide", 1f);
            grouped = data.Bool("grouped", false);
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
                Player player = getOnePlayerOnTop();
                bool onTop;
                if (player != null) {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                } else {
                    player = getOnePlayerClimbing();
                    if (player == null) {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                // make pieces shake and emit particles
                Audio.Play("event:/game/general/platform_disintegrate", Center);
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
                    while (timer > 0f && getOnePlayerOnTop() != null) {
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
                            falls[i].Replace((IEnumerator) crumblePlatformTileIn.Invoke(this, new object[] { i, images[fallOrder[i]], 0.05f * (float) m }));
                        }
                    }
                }
            }
        }

        private Player getOnePlayerOnTop() {
            foreach (CustomizableCrumblePlatform platform in groupedCrumblePlatforms) {
                Player p = platform.GetPlayerOnTop();
                if (p != null) {
                    return p;
                }
            }
            return null;
        }

        private Player getOnePlayerClimbing() {
            foreach (CustomizableCrumblePlatform platform in groupedCrumblePlatforms) {
                Player p = platform.GetPlayerClimbing();
                if (p != null) {
                    return p;
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
