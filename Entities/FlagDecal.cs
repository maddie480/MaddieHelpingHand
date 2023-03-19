using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagDecal", "MaxHelpingHand/FlagDecalXML")]
    public class FlagDecal : Entity {
        private Sprite sprite;
        private string flag;
        private bool inverted;
        private bool activated;

        public FlagDecal(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");

            Depth = data.Int("depth");

            if (data.Has("sprite")) {
                sprite = GFX.SpriteBank.Create(data.Attr("sprite"));
            } else {
                float animationDelay = 1f / data.Float("fps");

                // set up the sprite like a Sprites.xml one, based on the entity settings
                // "enabled" = the decal
                sprite = new Sprite(GFX.Game, "decals/");
                sprite.AddLoop("enabled", data.Attr("decalPath"), animationDelay);

                // "appear" = the appear animation, that goes to the enabled one
                if (!string.IsNullOrEmpty(data.Attr("appearAnimationPath"))) {
                    sprite.Add("appear", data.Attr("appearAnimationPath"), animationDelay, "enabled");
                }

                // "disappear" = the disappear animation, that makes the decal invisible once done
                if (!string.IsNullOrEmpty(data.Attr("disappearAnimationPath"))) {
                    sprite.Add("disappear", data.Attr("disappearAnimationPath"), animationDelay);
                }

                sprite.CenterOrigin();
            }


            sprite.OnFinish += anim => {
                if (anim == "disappear") {
                    hide();
                }
            };

            Add(sprite);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            activated = (SceneAs<Level>().Session.GetFlag(flag) != inverted);

            if (activated) {
                // flag is activated => play the enabled animation
                sprite.Play("enabled");
            } else {
                // flag is not activated => hide the decal
                hide();
            }
        }

        public override void Update() {
            base.Update();

            bool currentlyActive = (SceneAs<Level>().Session.GetFlag(flag) != inverted);

            if (currentlyActive != activated) {
                if (currentlyActive) {
                    // the flag became active => make the sprite visible, and play the appear animation if present.
                    Visible = true;
                    sprite.Play(sprite.Has("appear") ? "appear" : "enabled");
                } else {
                    // the flag became inactive => play the disappear animation, or hide the decal immediately if absent.
                    if (sprite.Has("disappear")) {
                        sprite.Play("disappear");
                    } else {
                        hide();
                    }
                }

                activated = currentlyActive;
            }
        }

        private void hide() {
            // if no "disabled" animation exists, we just hide the sprite instead
            if (sprite.Has("disabled")) {
                sprite.Play("disabled");
            } else {
                Visible = false;
            }
        }
    }
}
