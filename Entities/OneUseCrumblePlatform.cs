using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/OneUseCrumblePlatform")]
    class OneUseCrumblePlatform : CrumblePlatform {
        private static MethodInfo crumblePlatformSequence = typeof(CrumblePlatform).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance);

        public OneUseCrumblePlatform(EntityData data, Vector2 offset) : base(data, offset) {
            OverrideTexture = data.Attr("texture", null);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            DynData<CrumblePlatform> self = new DynData<CrumblePlatform>(this);
            Coroutine outlineFader = self.Get<Coroutine>("outlineFader");
            List<Coroutine> falls = self.Get<List<Coroutine>>("falls");
            outlineFader.RemoveSelf();

            foreach (Component component in this) {
                if (component is Coroutine coroutine && !falls.Contains(coroutine)) {
                    // this coroutine is the sequence! hijack it
                    coroutine.RemoveSelf();
                    Add(new Coroutine(oneUseSequence()));
                    break;
                }
            }
        }

        private IEnumerator oneUseSequence() {
            IEnumerator vanillaSequence = (IEnumerator) crumblePlatformSequence.Invoke(this, new object[0]);

            while (vanillaSequence.MoveNext()) {
                yield return vanillaSequence.Current;
                if (vanillaSequence.Current is float time && time == 2f) {
                    // done crumbling! now, go away.
                    RemoveSelf();
                    yield break;
                }
            }
        }
    }
}
