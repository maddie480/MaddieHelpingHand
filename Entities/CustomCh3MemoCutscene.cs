using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {

    public class CustomCh3MemoCutscene : CutsceneEntity {
        private class MemoPage : Entity {
            private Atlas atlas;
            private MTexture paper;
            private MTexture title;

            private VirtualRenderTarget target;

            private FancyText.Text text;

            private float textDownscale = 1f;
            private float alpha = 1f;
            private float scale = 1f;
            private float rotation;
            private float timer;

            private bool easingOut;

            public MemoPage(string folderName, string dialogId) {
                Tag = Tags.HUD;

                if (string.IsNullOrEmpty(folderName)) {
                    atlas = Atlas.FromAtlas("Graphics/Atlases/Memo", Atlas.AtlasDataFormat.Packer);
                } else {
                    atlas = Atlas.FromAtlas("Graphics/Atlases/MaxHelpingHandCustomMemos/" + folderName, Atlas.AtlasDataFormat.Packer);
                }

                paper = atlas["memo"];
                if (atlas.Has("title_" + Settings.Instance.Language)) {
                    title = atlas["title_" + Settings.Instance.Language];
                } else {
                    title = atlas["title_english"];
                }

                float paperSize = paper.Width * 1.5f - 120f;
                text = FancyText.Parse(Dialog.Get(dialogId), (int) (paperSize / 0.75f), -1, 1f, Color.Black * 0.6f);
                float textSize = text.WidestLine() * 0.75f;
                if (textSize > paperSize) {
                    textDownscale = paperSize / textSize;
                }

                Add(new BeforeRenderHook(BeforeRender));
            }

            public IEnumerator EaseIn() {
                Audio.Play("event:/game/03_resort/memo_in");

                Vector2 from = new Vector2(Engine.Width / 2, Engine.Height + 100);
                Vector2 to = new Vector2(Engine.Width / 2, Engine.Height / 2 - 150);
                float rFrom = -0.1f;
                float rTo = 0.05f;

                for (float p = 0f; p < 1f; p += Engine.DeltaTime) {
                    Position = from + (to - from) * Ease.CubeOut(p);
                    alpha = Ease.CubeOut(p);
                    rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                    yield return null;
                }
            }

            public IEnumerator Wait() {
                float start = Position.Y;

                int index = 0;
                while (!Input.MenuCancel.Pressed) {
                    float targetPosition = start - (index * 400);

                    Position.Y += (targetPosition - Position.Y) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));

                    if (Input.MenuUp.Pressed && index > 0) {
                        index--;
                    } else if (index * 400 < (paper.Height * 1.5 - 700)) {
                        if ((Input.MenuDown.Pressed && !Input.MenuDown.Repeating) || Input.MenuConfirm.Pressed) {
                            index++;
                        }
                    } else if (Input.MenuConfirm.Pressed) {
                        break;
                    }

                    yield return null;
                }

                Audio.Play("event:/ui/main/button_lowkey");
            }

            public IEnumerator EaseOut() {
                Audio.Play("event:/game/03_resort/memo_out");

                easingOut = true;

                Vector2 from = Position;
                Vector2 to = new Vector2(Engine.Width / 2, -target.Height);
                float rFrom = rotation;
                float rTo = rotation + 0.1f;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f) {
                    Position = from + (to - from) * Ease.CubeIn(p);
                    alpha = 1f - Ease.CubeIn(p);
                    rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                    yield return null;
                }

                RemoveSelf();
            }

            public void BeforeRender() {
                if (target == null) {
                    target = VirtualContent.CreateRenderTarget("oshiro-memo", (int) (paper.Width * 1.5f), (int) (paper.Height * 1.5f));
                }

                Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                paper.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
                title.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
                text.Draw(new Vector2(paper.Width * 1.5f / 2f, 210f), new Vector2(0.5f, 0f), Vector2.One * 0.75f * textDownscale, 1f);
                Draw.SpriteBatch.End();
            }

            public override void Removed(Scene scene) {
                if (target != null) {
                    target.Dispose();
                }

                target = null;
                atlas.Dispose();
                base.Removed(scene);
            }

            public override void SceneEnd(Scene scene) {
                if (target != null) {
                    target.Dispose();
                }

                target = null;
                atlas.Dispose();
                base.SceneEnd(scene);
            }

            public override void Update() {
                timer += Engine.DeltaTime;
                base.Update();
            }

            public override void Render() {
                Level level = Scene as Level;
                if ((level == null || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)) && target != null) {
                    Draw.SpriteBatch.Draw(target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, 0f) / 2f, scale, SpriteEffects.None, 0f);
                    if (!easingOut) {
                        GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height + ((timer % 1f < 0.25f) ? 6 : 0)));
                    }
                }
            }
        }

        private Player player;
        private MemoPage memo;

        private readonly string folderName;
        private readonly string dialogId;
        private readonly string dialogBeforeId;
        private readonly string dialogAfterId;
        private readonly string flagOnCompletion;

        public CustomCh3MemoCutscene(Player player, string folderName, string dialogId, string dialogBeforeId, string dialogAfterId, string flagOnCompletion) {
            this.player = player;
            this.folderName = folderName;
            this.dialogId = dialogId;
            this.dialogBeforeId = dialogBeforeId;
            this.dialogAfterId = dialogAfterId;
            this.flagOnCompletion = flagOnCompletion;
        }

        public override void OnBegin(Level level) {
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine() {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;

            // slow dialog before if present
            if (!string.IsNullOrEmpty(dialogBeforeId)) {
                yield return Textbox.Say(dialogBeforeId);
            }

            // show the memo
            memo = new MemoPage(folderName, dialogId);
            Scene.Add(memo);

            yield return memo.EaseIn();
            yield return memo.Wait();
            yield return memo.EaseOut();

            memo = null;

            // show dialog after if present
            if (!string.IsNullOrEmpty(dialogAfterId)) {
                yield return Textbox.Say(dialogAfterId);
            }

            EndCutscene(Level);
        }

        public override void OnEnd(Level level) {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            if (memo != null) {
                memo.RemoveSelf();
            }

            // set the flag on completion if present
            if (!string.IsNullOrEmpty(flagOnCompletion)) {
                level.Session.SetFlag(flagOnCompletion);
            }
        }
    }
}
