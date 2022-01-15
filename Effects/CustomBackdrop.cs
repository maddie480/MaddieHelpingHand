using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public abstract class CustomBackdrop : Backdrop {
        public float GetFadeAlpha(Scene scene) {
            return GetFadeAlphaFor(this, scene);
        }

        public static float GetFadeAlphaFor(Backdrop backdrop, Scene scene) {
            float fadeAlpha = 1f;

            Vector2 value = ((scene as Level).Camera.Position).Floor();
            if (backdrop.FadeX != null) {
                fadeAlpha *= backdrop.FadeX.Value(value.X + 160f);
            }
            if (backdrop.FadeY != null) {
                fadeAlpha *= backdrop.FadeY.Value(value.Y + 90f);
            }

            return fadeAlpha;
        }
    }
}
