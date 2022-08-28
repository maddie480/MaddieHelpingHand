using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagDecal")]
    public class FlagDecal : Entity {
        private Sprite sprite;
        private string flag;
        private bool inverted;
        private bool activated;

        public FlagDecal(EntityData data, Vector2 offset) : base(data.Position + offset) {
            float animationDelay = 1f / data.Float("fps");

            flag = data.Attr("flag");
            inverted = data.Bool("inverted");

            Depth = data.Bool("foreground") ? Depths.FGDecals : Depths.BGDecals;

            // set up the sprite like a Sprites.xml one, based on the entity settings
            // "idle" = the decal
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("idle", data.Attr("decalPath"), animationDelay);

            // "appear" = the appear animation, that goes to the idle one
            if (!string.IsNullOrEmpty(data.Attr("appearAnimationPath"))) {
                sprite.Add("appear", data.Attr("appearAnimationPath"), animationDelay, "idle");
            }

            // "disappear" = the disappear animation, that makes the decal invisible once done
            if (!string.IsNullOrEmpty(data.Attr("disappearAnimationPath"))) {
                sprite.Add("disappear", data.Attr("disappearAnimationPath"), animationDelay);

                sprite.OnFinish += anim => {
                    if (anim == "disappear") {
                        Visible = false;
                    }
                };
            }

            sprite.CenterOrigin();

            Add(sprite);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            activated = (SceneAs<Level>().Session.GetFlag(flag) != inverted);

            if (activated) {
                // flag is activated => play the idle animation
                sprite.Play("idle");
            } else {
                // flag is not activated => hide the decal entirely
                Visible = false;
            }
        }

        public override void Update() {
            base.Update();

            bool currentlyActive = (SceneAs<Level>().Session.GetFlag(flag) != inverted);

            if (currentlyActive != activated) {
                if (currentlyActive) {
                    // the flag became active => make the sprite visible, and play the appear animation if present.
                    Visible = true;
                    sprite.Play(sprite.Has("appear") ? "appear" : "idle");
                } else {
                    // the flag became inactive => play the disappear animation, or hide the decal immediately if absent.
                    if (sprite.Has("disappear")) {
                        sprite.Play("disappear");
                    } else {
                        Visible = false;
                    }
                }

                activated = currentlyActive;
            }
        }
    }
}
