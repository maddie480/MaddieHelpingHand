using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A switch gate triggered by a flag touch switch.
    /// 
    /// Attributes:
    /// - flag: the session flag this switch gate reacts to. Must be the same across the touch switch group.
    /// - icon: the name of the icon for the switch gate (relative to objects/MaxHelpingHand/flagSwitchGate) or "vanilla" for the default one.
    /// - persistent: enable to have the gate stay open even when you die / change rooms.
    /// - inactiveColor / activeColor / finishColor: custom colors for the touch switch.
    /// - sprite: the texture for the gate block.
    /// </summary>
    [CustomEntity("MaxHelpingHand/FlagSwitchGate")]
    [Tracked]
    class FlagSwitchGate : Solid {
        private ParticleType P_RecoloredFire;

        private MTexture[,] nineSlice;

        private Sprite icon;
        private Vector2 iconOffset;

        private Wiggler wiggler;

        private Vector2 node;

        private SoundSource openSfx;

        public int ID { get; private set; }
        public string Flag { get; private set; }

        public bool Triggered { get; private set; }

        private readonly Color inactiveColor;
        private readonly Color activeColor;
        private readonly Color finishColor;

        private readonly float shakeTime;
        private readonly float moveTime;

        public FlagSwitchGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false) {

            node = data.Nodes[0] + offset;
            ID = data.ID;
            Flag = data.Attr("flag");

            inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
            activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
            finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));

            shakeTime = data.Float("shakeTime", 0.5f);
            moveTime = data.Float("moveTime", 1.8f);

            P_RecoloredFire = new ParticleType(TouchSwitch.P_Fire) {
                Color = finishColor
            };

            string iconAttribute = data.Attr("icon", "vanilla");
            icon = new Sprite(GFX.Game, iconAttribute == "vanilla" ? "objects/switchgate/icon" : $"objects/MaxHelpingHand/flagSwitchGate/{iconAttribute}/icon");
            Add(icon);
            icon.Add("spin", "", 0.1f, "spin");
            icon.Play("spin");
            icon.Rate = 0f;
            icon.Color = inactiveColor;
            icon.Position = (iconOffset = new Vector2(data.Width / 2f, data.Height / 2f));
            icon.CenterOrigin();
            Add(wiggler = Wiggler.Create(0.5f, 4f, f => {
                icon.Scale = Vector2.One * (1f + f);
            }));

            MTexture nineSliceTexture = GFX.Game["objects/switchgate/" + data.Attr("sprite", "block")];
            nineSlice = new MTexture[3, 3];
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    nineSlice[i, j] = nineSliceTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            Add(openSfx = new SoundSource());
            Add(new LightOcclude(0.5f));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if (SceneAs<Level>().Session.GetFlag(Flag + "_gate" + ID) || SceneAs<Level>().Session.GetFlag(Flag)) {
                MoveTo(node);
                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
                icon.Color = finishColor;
            } else {
                Add(new Coroutine(Sequence(node)));
            }
        }

        public override void Render() {
            float widthInTiles = Collider.Width / 8f - 1f;
            float heightInTiles = Collider.Height / 8f - 1f;
            for (int i = 0; i <= widthInTiles; i++) {
                for (int j = 0; j <= heightInTiles; j++) {
                    int tilePartX = (i < widthInTiles) ? Math.Min(i, 1) : 2;
                    int tilePartY = (j < heightInTiles) ? Math.Min(j, 1) : 2;
                    nineSlice[tilePartX, tilePartY].Draw(Position + Shake + new Vector2(i * 8, j * 8));
                }
            }

            icon.Position = iconOffset + Shake;
            icon.DrawOutline();

            base.Render();
        }

        public void Trigger() {
            Triggered = true;
        }

        private IEnumerator Sequence(Vector2 node) {
            Vector2 start = Position;

            while (!Triggered && !SceneAs<Level>().Session.GetFlag(Flag)) {
                yield return null;
            }

            yield return 0.1f;

            // animate the icon
            openSfx.Play("event:/game/general/touchswitch_gate_open");
            if (shakeTime > 0f) {
                StartShaking(shakeTime);
                while (icon.Rate < 1f) {
                    icon.Color = Color.Lerp(inactiveColor, activeColor, icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                }
            } else {
                icon.Rate = 1f;
            }

            yield return 0.1f;

            // move the switch gate, emitting particles along the way
            int particleAt = 0;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, moveTime + 0.2f, start: true);
            tween.OnUpdate = tweenArg => {
                MoveTo(Vector2.Lerp(start, node, tweenArg.Eased));
                if (Scene.OnInterval(0.1f)) {
                    particleAt++;
                    particleAt %= 2;
                    for (int tileX = 0; tileX < Width / 8f; tileX++) {
                        for (int tileY = 0; tileY < Height / 8f; tileY++) {
                            if ((tileX + tileY) % 2 == particleAt) {
                                SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind,
                                    Position + new Vector2(tileX * 8, tileY * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                            }
                        }
                    }
                }
            };
            Add(tween);

            yield return moveTime;

            bool collidableBackup = Collidable;
            Collidable = false;

            // collide dust particles on the left
            if (node.X <= start.X) {
                Vector2 add = new Vector2(0f, 2f);
                for (int tileY = 0; tileY < Height / 8f; tileY++) {
                    Vector2 collideAt = new Vector2(Left - 1f, Top + 4f + (tileY * 8));
                    Vector2 noCollideAt = collideAt + Vector2.UnitX;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float) Math.PI);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float) Math.PI);
                    }
                }
            }

            // collide dust particles on the rigth
            if (node.X >= start.X) {
                Vector2 add = new Vector2(0f, 2f);
                for (int tileY = 0; tileY < Height / 8f; tileY++) {
                    Vector2 collideAt = new Vector2(Right + 1f, Top + 4f + (tileY * 8));
                    Vector2 noCollideAt = collideAt - Vector2.UnitX * 2f;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, 0f);
                    }
                }
            }

            // collide dust particles on the top
            if (node.Y <= start.Y) {
                Vector2 add = new Vector2(2f, 0f);
                for (int tileX = 0; tileX < Width / 8f; tileX++) {
                    Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Top - 1f);
                    Vector2 noCollideAt = collideAt + Vector2.UnitY;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, -(float) Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, -(float) Math.PI / 2f);
                    }
                }
            }

            // collide dust particles on the bottom
            if (node.Y >= start.Y) {
                Vector2 add = new Vector2(2f, 0f);
                for (int tileX = 0; tileX < Width / 8f; tileX++) {
                    Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Bottom + 1f);
                    Vector2 noCollideAt = collideAt - Vector2.UnitY * 2f;
                    if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float) Math.PI / 2f);
                        SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float) Math.PI / 2f);
                    }
                }
            }
            Collidable = collidableBackup;

            // moving is over
            Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
            StartShaking(0.2f);
            while (icon.Rate > 0f) {
                icon.Color = Color.Lerp(activeColor, finishColor, 1f - icon.Rate);
                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
            }
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            wiggler.Start();

            // emit fire particles if the block is not behind a solid.
            collidableBackup = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center)) {
                for (int i = 0; i < 32; i++) {
                    float angle = Calc.Random.NextFloat((float) Math.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_RecoloredFire, Position + iconOffset + Calc.AngleToVector(angle, 4f), angle);
                }
            }
            Collidable = collidableBackup;
        }
    }
}