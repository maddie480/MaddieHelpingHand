using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

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
    public class FlagSwitchGate : Solid {
        private ParticleType P_RecoloredFire;
        private ParticleType P_RecoloredFireBack;

        private MTexture texture;

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
        private readonly bool moveEased;

        private readonly string moveSound;
        private readonly string finishedSound;

        private readonly bool smoke;

        private readonly bool allowReturn;

        private readonly bool isShatter;
        private readonly string blockSpriteName;
        private readonly string debrisPath;

        public FlagSwitchGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false) {

            isShatter = data.Bool("isShatter", defaultValue: false);
            if (data.Nodes.Length > 0) {
                node = data.Nodes[0] + offset;
            }

            ID = data.ID;
            Flag = data.Attr("flag");

            inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
            activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
            finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));

            shakeTime = data.Float("shakeTime", 0.5f);
            moveTime = data.Float("moveTime", 1.8f);
            moveEased = data.Bool("moveEased", true);

            moveSound = data.Attr("moveSound", "event:/game/general/touchswitch_gate_open");
            finishedSound = data.Attr("finishedSound", "event:/game/general/touchswitch_gate_finish");

            smoke = data.Bool("smoke", true);

            allowReturn = data.Bool("allowReturn", false);

            P_RecoloredFire = new ParticleType(TouchSwitch.P_Fire) {
                Color = finishColor
            };
            P_RecoloredFireBack = new ParticleType(TouchSwitch.P_Fire) {
                Color = inactiveColor
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

            blockSpriteName = data.Attr("sprite", "block");
            texture = GFX.Game["objects/switchgate/" + blockSpriteName];

            debrisPath = data.Attr("debrisPath");
            if (string.IsNullOrEmpty(debrisPath)) {
                switch (blockSpriteName) {
                    default:
                        debrisPath = "debris/VortexHelper/disintegate/1";
                        break;
                    case "mirror":
                        debrisPath = "debris/VortexHelper/disintegate/2";
                        break;
                    case "temple":
                        debrisPath = "debris/VortexHelper/disintegate/3";
                        break;
                    case "stars":
                        debrisPath = "debris/VortexHelper/disintegate/4";
                        break;
                }
            }

            Add(openSfx = new SoundSource());
            Add(new LightOcclude(0.5f));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            if ((SceneAs<Level>().Session.GetFlag(Flag + "_gate" + ID) && !allowReturn) || SceneAs<Level>().Session.GetFlag(Flag)) {
                if (isShatter) {
                    RemoveSelf();
                }

                if (allowReturn) {
                    // watch the flag to return to the start if necessary.
                    Add(new Coroutine(moveBackAndForthSequence(Position, node, startAtNode: true)));
                }

                MoveTo(node);
                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
                icon.Color = finishColor;
            } else {
                if (isShatter) {
                    Add(new Coroutine(shatterSequence()));
                } else if (allowReturn) {
                    // go back and forth as needed.
                    Add(new Coroutine(moveBackAndForthSequence(Position, node, startAtNode: false)));
                } else {
                    // we are only going to the node, then stopping.
                    Add(new Coroutine(moveSequence(node, goingBack: false)));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool InView() {
            Camera camera = (Scene as Level).Camera;
            return Position.X + Width > camera.X - 16f && Position.Y + Height > camera.Y - 16f && Position.X < camera.X + 320f && Position.Y < camera.Y + 180f;
        }

        public override void Render() {
            if (!InView()) return;

            int widthInTiles = (int) Collider.Width / 8 - 1;
            int heightInTiles = (int) Collider.Height / 8 - 1;

            Vector2 renderPos = new Vector2(Position.X + Shake.X, Position.Y + Shake.Y);
            Texture2D baseTexture = texture.Texture.Texture;
            int clipBaseX = texture.ClipRect.X;
            int clipBaseY = texture.ClipRect.Y;

            Rectangle clipRect = new Rectangle(clipBaseX, clipBaseY, 8, 8);

            for (int i = 0; i <= widthInTiles; i++) {
                clipRect.X = clipBaseX + ((i < widthInTiles) ? i == 0 ? 0 : 8 : 16);
                for (int j = 0; j <= heightInTiles; j++) {
                    int tilePartY = (j < heightInTiles) ? j == 0 ? 0 : 8 : 16;
                    clipRect.Y = tilePartY + clipBaseY;
                    Draw.SpriteBatch.Draw(baseTexture, renderPos, clipRect, Color.White);
                    renderPos.Y += 8f;
                }
                renderPos.X += 8f;
                renderPos.Y = Position.Y + Shake.Y;
            }

            icon.Position = iconOffset + Shake;
            icon.DrawOutline();

            base.Render();
        }

        public void Trigger() {
            Triggered = true;
        }

        private IEnumerator moveBackAndForthSequence(Vector2 position, Vector2 node, bool startAtNode) {
            while (true) {
                if (!startAtNode) {
                    // go forth
                    IEnumerator forthSeq = moveSequence(node, goingBack: false);
                    while (forthSeq.MoveNext()) {
                        yield return forthSeq.Current;
                    }
                }

                // go back
                IEnumerator backSeq = moveSequence(position, goingBack: true);
                while (backSeq.MoveNext()) {
                    yield return backSeq.Current;
                }

                startAtNode = false;
            }
        }

        private IEnumerator moveSequence(Vector2 node, bool goingBack) {
            Vector2 start = Position;

            Color fromColor, toColor;

            if (!goingBack) {
                fromColor = inactiveColor;
                toColor = finishColor;
                while ((!Triggered || allowReturn) && !SceneAs<Level>().Session.GetFlag(Flag)) {
                    yield return null;
                }
            } else {
                fromColor = finishColor;
                toColor = inactiveColor;
                while (SceneAs<Level>().Session.GetFlag(Flag)) {
                    yield return null;
                }
            }

            yield return 0.1f;
            if (shouldCancelMove(goingBack)) yield break;

            // animate the icon
            openSfx.Play(moveSound);
            if (shakeTime > 0f) {
                StartShaking(shakeTime);
                while (icon.Rate < 1f) {
                    icon.Color = Color.Lerp(fromColor, activeColor, icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                    if (shouldCancelMove(goingBack)) yield break;
                }
            } else {
                icon.Rate = 1f;
            }

            yield return 0.1f;
            if (shouldCancelMove(goingBack)) yield break;

            // move the switch gate, emitting particles along the way
            int particleAt = 0;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, moveEased ? Ease.CubeOut : null, moveTime + (moveEased ? 0.2f : 0f), start: true);
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

            float moveTimeLeft = moveTime;
            while (moveTimeLeft > 0f) {
                yield return null;
                moveTimeLeft -= Engine.DeltaTime;
                if (shouldCancelMove(goingBack, tween)) yield break;
            }

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
            Audio.Play(finishedSound, Position);
            StartShaking(0.2f);
            while (icon.Rate > 0f) {
                icon.Color = Color.Lerp(activeColor, toColor, 1f - icon.Rate);
                icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
                if (shouldCancelMove(goingBack)) yield break;
            }
            icon.Rate = 0f;
            icon.SetAnimationFrame(0);
            wiggler.Start();

            // emit fire particles if the block is not behind a solid.
            collidableBackup = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center) && smoke) {
                for (int i = 0; i < 32; i++) {
                    float angle = Calc.Random.NextFloat((float) Math.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(goingBack ? P_RecoloredFireBack : P_RecoloredFire, Position + iconOffset + Calc.AngleToVector(angle, 4f), angle);
                }
            }
            Collidable = collidableBackup;
        }

        private bool shouldCancelMove(bool goingBack, Tween tween = null) {
            if (allowReturn && SceneAs<Level>().Session.GetFlag(Flag) == goingBack) {
                // whoops, the flag changed too fast! we need to backtrack.
                if (tween != null) {
                    Remove(tween);
                }

                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
                return true;
            }
            return false;
        }

        // this is heavily inspired by Vortex Helper by catapillie, and also requires Vortex Helper to fully work.
        private IEnumerator shatterSequence() {
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "VortexHelper", Version = new Version(1, 1, 0) })) {
                // error postcards are nicer than crashes!
                Audio.SetMusic(null);
                LevelEnter.ErrorMessage = "{big}Oops!{/big}{n}To use {# F94A4A}Shatter Flag Switch Gates{#}, you need to have {# d678db}Vortex Helper{#} installed!";
                LevelEnter.Go(new Session(SceneAs<Level>().Session.Area), fromSaveData: false);
                yield break;
            }

            Level level = SceneAs<Level>();
            while ((!Triggered || allowReturn) && !SceneAs<Level>().Session.GetFlag(Flag)) {
                yield return null;
            }

            openSfx.Play("event:/game/general/fallblock_shake");
            yield return 0.1f;

            if (shakeTime > 0f) {
                StartShaking(shakeTime);
                while (icon.Rate < 1f) {
                    icon.Color = Color.Lerp(inactiveColor, finishColor, icon.Rate);
                    icon.Rate += Engine.DeltaTime / shakeTime;
                    yield return null;
                }
            } else {
                icon.Rate = 1f;
            }

            yield return 0.1f;
            for (int k = 0; k < 32; k++) {
                float num = Calc.Random.NextFloat((float) Math.PI * 2f);
                SceneAs<Level>().ParticlesFG.Emit(P_RecoloredFire, Position + iconOffset + Calc.AngleToVector(num, 4f), num);
            }
            openSfx.Stop();
            Audio.Play("event:/game/general/wall_break_stone", Center);
            Audio.Play(finishedSound, Center);
            level.Shake();

            for (int i = 0; i < Width / 8f; i++) {
                for (int j = 0; j < Height / 8f; j++) {
                    Debris debris = new Debris().orig_Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '1').BlastFrom(Center);
                    DynData<Debris> debrisData = new DynData<Debris>(debris);
                    debrisData.Get<Image>("image").Texture = GFX.Game[debrisPath];
                    Scene.Add(debris);
                }
            }
            DestroyStaticMovers();
            RemoveSelf();
        }
    }
}
