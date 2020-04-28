using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableCrumblePlatform")]
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
        private List<Image> outline;

        private bool oneUse;
        private float respawnDelay;

        public CustomizableCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) {
            OverrideTexture = data.Attr("texture", null);
            oneUse = data.Bool("oneUse", false);
            respawnDelay = data.Float("respawnDelay", 2f);
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
            outline = self.Get<List<Image>>("outline");

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

        private IEnumerator customSequence() {
            while (true) {
                // wait until player is on top
                Player player = GetPlayerOnTop();
                bool onTop;
                if (player != null) {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                } else {
                    player = GetPlayerClimbing();
                    if (player == null) {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                // make pieces shake and emit particles
                Audio.Play("event:/game/general/platform_disintegrate", Center);
                shaker.ShakeFor(onTop ? 0.6f : 1f, removeOnFinish: false);
                foreach (Image image in images) {
                    SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                for (int l = 0; l < (onTop ? 1 : 3); l++) {
                    yield return 0.2f;
                    foreach (Image image in images) {
                        SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }

                // wait for a bit more
                float timer = 0.4f;
                if (onTop) {
                    while (timer > 0f && GetPlayerOnTop() != null) {
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
                while (CollideCheck<Actor>() || CollideCheck<Solid>()) {
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
    }
}
