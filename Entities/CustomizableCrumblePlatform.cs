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
        private static MethodInfo crumblePlatformSequence = typeof(CrumblePlatform).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance);

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
            Coroutine outlineFader = self.Get<Coroutine>("outlineFader");
            List<Coroutine> falls = self.Get<List<Coroutine>>("falls");

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
            IEnumerator vanillaSequence = (IEnumerator) crumblePlatformSequence.Invoke(this, new object[0]);

            while (vanillaSequence.MoveNext()) {
                if (vanillaSequence.Current is float time && time == 2f) {
                    // the platform crumbled and is waiting to respawn.

                    if (oneUse) {
                        // wait for the platform to animate for a bit (1 second) then delete it.
                        yield return 1f;
                        RemoveSelf();
                        yield break;
                    } else {
                        // go on, but use the custom delay instead of 2 seconds.
                        yield return respawnDelay;
                    }
                } else {
                    // the platform isn't crumbled yet, go on.
                    yield return vanillaSequence.Current;
                }
            }
        }
    }
}
