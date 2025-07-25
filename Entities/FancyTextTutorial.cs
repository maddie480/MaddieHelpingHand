using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // A tutorial made of fancy text. Because that's how formatted text is called in Celeste.
    [CustomEntity("MaxHelpingHand/FancyTextTutorial")]
    [Tracked]
    public class FancyTextTutorial : Entity {
        private static Hook customBirdTutorialTriggerOnEnter = null;

        public static void Load() {
            customBirdTutorialTriggerOnEnter = new Hook(
                typeof(CustomBirdTutorialTrigger).GetMethod("OnEnter"),
                typeof(FancyTextTutorial).GetMethod("onEnterCustomBirdTutorialTrigger", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            customBirdTutorialTriggerOnEnter?.Dispose();
            customBirdTutorialTriggerOnEnter = null;
        }

        private static void onEnterCustomBirdTutorialTrigger(Action<CustomBirdTutorialTrigger, Player> orig, CustomBirdTutorialTrigger self, Player player) {
            orig(self, player);

            // do the same for fancy text tutorial!
            FancyTextTutorial matchingTutorial = FindById(self.Scene as Level, self.BirdId);
            if (matchingTutorial != null) {
                if (self.ShowTutorial) {
                    matchingTutorial.TriggerShowTutorial();
                } else {
                    matchingTutorial.TriggerHideTutorial();
                }
            }
        }


        private enum Direction { Up, Down, Left, Right, None }

        private static readonly Color bgColor = Calc.HexToColor("061526");
        private static readonly Color lineColor = new Color(1f, 1f, 1f);

        private float scale = 0f;
        private bool triggered = false;
        private bool open = false;

        private readonly FancyText.Text fancyText;
        private readonly float fancyTextScale;
        private readonly Vector2 fancyTextDimensions;

        private readonly EntityID entityID;

        private readonly Direction direction;
        private readonly string birdId;
        private readonly bool onlyOnce;

        public FancyTextTutorial(EntityData data, Vector2 offset, EntityID entityID) : base(data.Position + offset) {
            fancyText = FancyText.Parse(Dialog.Get(data.Attr("dialogId")), int.MaxValue, int.MaxValue);
            fancyTextScale = data.Float("textScale");
            direction = data.Enum("direction", Direction.Down);
            birdId = data.Attr("birdId");
            onlyOnce = data.Bool("onlyOnce");

            this.entityID = entityID;

            int lineCount = 1;
            foreach (FancyText.Node node in fancyText.Nodes) {
                if (node is FancyText.NewLine) lineCount++;
            }
            fancyTextDimensions = new Vector2(fancyText.WidestLine(), lineCount * (float) fancyText.Font.Get(fancyText.BaseSize).LineHeight) * fancyTextScale;

            AddTag(Tags.HUD);
        }

        public override void Update() {
            scale = Calc.Approach(scale, open ? 1 : 0, Engine.RawDeltaTime * 8f);
            base.Update();
        }

        public override void Render() {
            Level level = Scene as Level;
            if (level.FrozenOrPaused || level.RetryPlayerCorpse != null || scale <= 0f) {
                return;
            }

            Camera camera = SceneAs<Level>().Camera;
            Vector2 drawPosition = Position - camera.Position.Floor();
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                drawPosition.X = MaxHelpingHandModule.CameraWidth - drawPosition.X;
            }
            // usually 6f, use CameraWidth for Zoomout compatibility
            drawPosition *= 1920f / MaxHelpingHandModule.CameraWidth;

            float width = (fancyTextDimensions.X + 32f) * scale;
            float height = fancyTextDimensions.Y + 32f;
            float x = drawPosition.X - width / 2f;
            float y = drawPosition.Y - height - 32f;

            Draw.Rect(x - 6f, y - 6f, width + 12f, height + 12f, lineColor);
            Draw.Rect(x, y, width, height, bgColor);

            for (int i = 0; i <= 36; i++) {
                float bubbleWidth = (73 - i * 2) * scale;

                switch (direction) {
                    case Direction.Down:
                        Draw.Rect(drawPosition.X - bubbleWidth / 2f, y + height + i, bubbleWidth, 1f, lineColor);
                        if (bubbleWidth > 12f) {
                            Draw.Rect(drawPosition.X - bubbleWidth / 2f + 6f, y + height + i, bubbleWidth - 12f, 1f, bgColor);
                        }
                        break;

                    case Direction.Up:
                        Draw.Rect(drawPosition.X - bubbleWidth / 2f, y - i - 1, bubbleWidth, 1f, lineColor);
                        if (bubbleWidth > 12f) {
                            Draw.Rect(drawPosition.X - bubbleWidth / 2f + 6f, y - i, bubbleWidth - 12f, 1f, bgColor);
                        }
                        break;

                    case Direction.Left:
                        Draw.Rect(x - i - 1, y + height / 2f - bubbleWidth / 2f, 1f, bubbleWidth, lineColor);
                        if (bubbleWidth > 12f) {
                            Draw.Rect(x - i, y + height / 2f - bubbleWidth / 2f + 6f, 1f, bubbleWidth - 12f, bgColor);
                        }
                        break;

                    case Direction.Right:
                        Draw.Rect(x + width + i, y + height / 2f - bubbleWidth / 2f, 1f, bubbleWidth, lineColor);
                        if (bubbleWidth > 12f) {
                            Draw.Rect(x + width + i, y + height / 2f - bubbleWidth / 2f + 6f, 1f, bubbleWidth - 12f, bgColor);
                        }
                        break;
                }
            }
            if (width <= 3f) {
                return;
            }

            fancyText.DrawJustifyPerLine(new Vector2(drawPosition.X, y + 16f), new Vector2(0.5f, 0f), new Vector2(scale, 1f) * fancyTextScale, 1f);
        }

        // here is code ripped off from Everest's custom tutorial bird to handle bird tutorial triggers.

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (scene.Tracker.GetEntities<CustomBirdTutorialTrigger>().OfType<CustomBirdTutorialTrigger>()
                .All(trigger => !trigger.ShowTutorial || trigger.BirdId != birdId)) {

                // none of the custom bird tutorial triggers are here to make the tutorial bubble show up.
                // so, make the bubble show up right away.
                TriggerShowTutorial();
            }
        }

        /// <summary>
        /// Helper method to find a fancy text tutorial in a scene by bird ID.
        /// Note that if only 1 fancy text tutorial is on screen, you can use level.Tracker.GetEntity&lt;CustomBirdTutorial>() instead.
        /// </summary>
        /// <param name="level">The level to search in</param>
        /// <param name="birdId">The ID of the bird to look for</param>
        /// <returns>The custom bird tutorial matching this ID, or null if none was found.</returns>
        public static FancyTextTutorial FindById(Level level, string birdId) {
            return level.Tracker.GetEntities<FancyTextTutorial>()
                .OfType<FancyTextTutorial>()
                .Where(bird => bird.birdId == birdId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Shows the tutorial to the player. Does nothing if it was already shown and hidden before.
        /// </summary>
        public void TriggerShowTutorial() {
            if (!triggered) {
                triggered = true;
                open = true;
            }
        }

        /// <summary>
        /// Hides the tutorial if it is currently shown.
        /// </summary>
        public void TriggerHideTutorial() {
            if (open) {
                open = false;

                if (onlyOnce) {
                    SceneAs<Level>().Session.DoNotLoad.Add(entityID);
                }
            }
        }
    }
}
