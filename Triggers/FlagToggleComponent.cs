using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    /// <summary>
    /// A component that can be attached to an entity to make it (dis)appear depending on a flag.
    /// This is the co;ponent that gets attached to camera triggers to make them flag-toggled.
    /// </summary>
    public class FlagToggleComponent : Component {
        public bool Enabled = true;
        private string flag;
        private Action onDisable;
        private Action onEnable;
        private bool inverted;

        public FlagToggleComponent(string flag, bool inverted, Action onDisable = null, Action onEnable = null) : base(true, false) {
            this.flag = flag;
            this.inverted = inverted;
            this.onDisable = onDisable;
            this.onEnable = onEnable;
        }

        public override void Update() {
            base.Update();
            UpdateFlag();
        }

        public void UpdateFlag() {
            if ((!inverted && SceneAs<Level>().Session.GetFlag(flag) != Enabled)
                || (inverted && SceneAs<Level>().Session.GetFlag(flag) == Enabled)) {

                if (Enabled) {
                    // disable the entity.
                    Entity.Visible = Entity.Collidable = false;
                    onDisable?.Invoke();
                    Enabled = false;
                } else {
                    // enable the entity.
                    Entity.Visible = Entity.Collidable = true;
                    onEnable?.Invoke();
                    Enabled = true;
                }
            }
        }
    }
}