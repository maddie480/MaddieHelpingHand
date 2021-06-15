using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    // same as Music Fade Trigger, but also modifies the given parameter in session to make it persist through deaths / save & quit.
    [CustomEntity("MaxHelpingHand/PersistentMusicFadeTrigger")]
    class PersistentMusicFadeTrigger : MusicFadeTrigger {
        public PersistentMusicFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnStay(Player player) {
            float paramValue = LeftToRight ? Calc.ClampedMap(player.Center.X, Left, Right, FadeA, FadeB) : Calc.ClampedMap(player.Center.Y, Top, Bottom, FadeA, FadeB);

            AudioState audioState = SceneAs<Level>().Session.Audio;
            audioState.Music.Param(Parameter, paramValue);
            audioState.Apply();
        }
    }
}
