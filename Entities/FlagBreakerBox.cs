using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A breaker box that sets a flag when broken.
    ///
    /// Attributes:
    /// - flag: the session flag this breaker box sets.
    /// - health: the number of dashes needed to break the box.
    /// - floaty: whether the box moves by itself.
    /// - bouncy: whether the box moves when dashed into.
    /// - sprite: the sprite for the breaker box, taken from the map's Sprites.xml.
    /// - flipX: whether the sprite should be horizontally flipped.
    /// - music: change the currently playing music to this when box is broken. leave empty to not change song.
    /// - music_progress: the new music progress once box is broken. -1 keeps the same progress.
    /// - music_session: whether the music should be stored in the session when box is broken.
    /// - surfaceIndex: determines which sound the box will make when the player climbs on it.
    /// - color, color2: particle colors.
    /// - refill: whether the box should refill the player's dash when broken.
    /// </summary>
    [CustomEntity("MaxHelpingHand/FlagBreakerBox")]
    [Tracked]
    public class FlagBreakerBox : Solid {
        private ParticleType P_RecolouredSmash;
        private ParticleType P_RecolouredSparks;

        private Sprite sprite;
        private string flag;

        private int health;

        private string music;
        private int musicProgress = -1;
        private bool musicStoreInSession;

        private Vector2 start;
        private bool floaty;
        private bool bouncy;
        private SineWave sine;
        private float sink;
        private Vector2 bounceDir;
        private Wiggler bounce;
        private float shakeCounter;
        private Shaker shaker;

        private bool makeSparks;
        private bool smashParticles;
        private bool opened;

        private bool refillOnBreak;

        private SoundSource firstHitSfx;

        private bool spikesLeft, spikesRight, spikesUp, spikesDown;

        public FlagBreakerBox(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, safe: true) {
            Depth = -10550;
            start = Position;

            flag = data.Attr("flag");
            health = data.Int("health", 2);
            music = data.Attr("music", null);
            musicProgress = data.Int("music_progress", -1);
            musicStoreInSession = data.Bool("music_session");
            SurfaceSoundIndex = data.Int("surfaceIndex", 9);
            Color color = Calc.HexToColor(data.Attr("color", "fffc75"));
            Color color2 = Calc.HexToColor(data.Attr("color2", "6bffff"));

            sprite = GFX.SpriteBank.Create(data.Attr("sprite", "breakerBox"));
            sprite.OnLastFrame = anim => {
                if (anim == "break") {
                    Visible = false;
                } else if (anim == "open") {
                    makeSparks = true;
                }
            };

            sprite.Position = new Vector2(Width, Height) / 2f;
            sprite.FlipX = data.Bool("flipX", false);
            Add(sprite);

            floaty = data.Bool("floaty", true);
            bouncy = data.Bool("bouncy", true);

            Add(sine = new SineWave(0.5f));

            bounce = Wiggler.Create(1f, 0.5f);
            bounce.StartZero = false;
            Add(bounce);

            Add(shaker = new Shaker(on: false));

            P_RecolouredSmash = new ParticleType(LightningBreakerBox.P_Smash) {
                Color = color,
                Color2 = color2
            };
            P_RecolouredSparks = new ParticleType(LightningBreakerBox.P_Sparks) {
                Color = color,
                Color2 = color2
            };

            refillOnBreak = data.Bool("refill", true);
            OnDashCollide = Dashed;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if ((Scene as Level).Session.GetFlag(flag)) {
                RemoveSelf();
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            spikesUp = CollideCheck<Spikes>(Position - Vector2.UnitY);
            spikesDown = CollideCheck<Spikes>(Position + Vector2.UnitY);
            spikesLeft = CollideCheck<Spikes>(Position - Vector2.UnitX);
            spikesRight = CollideCheck<Spikes>(Position + Vector2.UnitX);
        }


        public override void Update() {
            base.Update();
            if (makeSparks && Scene.OnInterval(0.03f)) {
                SceneAs<Level>().ParticlesFG.Emit(P_RecolouredSparks, 1, Center, Vector2.One * 12f);
            }
            if (shakeCounter > 0f) {
                shakeCounter -= Engine.DeltaTime;
                if (shakeCounter <= 0f) {
                    shaker.On = false;
                    sprite.Scale = Vector2.One * 1.2f;
                    if (!opened) {
                        sprite.Play("open");
                        opened = true;
                    }
                }
            }
            if (Collidable) {
                sink = Calc.Approach(sink, HasPlayerRider() ? 1 : 0, 2f * Engine.DeltaTime);
                sine.Rate = MathHelper.Lerp(1f, 0.5f, sink);
                Vector2 pos = start;
                if (floaty) {
                    pos.Y += sink * 6f + sine.Value * MathHelper.Lerp(4f, 2f, sink);
                }
                if (bouncy) {
                    pos += bounce.Value * bounceDir * 12f;
                }
                MoveToX(pos.X);
                MoveToY(pos.Y);
                if (smashParticles) {
                    smashParticles = false;
                    SmashParticles(bounceDir.Perpendicular());
                    SmashParticles(-bounceDir.Perpendicular());
                }
            }
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, Engine.DeltaTime * 4f);
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, Engine.DeltaTime * 4f);
            LiftSpeed = Vector2.Zero;
        }

        public override void Render() {
            Vector2 original_pos = sprite.Position;
            sprite.Position += shaker.Value;
            base.Render();
            sprite.Position = original_pos;
        }

        public DashCollisionResults Dashed(Player player, Vector2 dir) {
            if (!SaveData.Instance.Assists.Invincible) {
                if (dir == Vector2.UnitX && spikesLeft) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitX && spikesRight) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == Vector2.UnitY && spikesUp) {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitY && spikesDown) {
                    return DashCollisionResults.NormalCollision;
                }
            }

            (Scene as Level).DirectionalShake(dir);
            sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
            health--;
            if (health > 0) {
                if (firstHitSfx != null) {
                    firstHitSfx.Stop();
                    Remove(firstHitSfx);
                }
                Add(firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                Celeste.Freeze(0.1f);
                shakeCounter = 0.2f;
                shaker.On = true;
                bounceDir = dir;
                bounce.Start();
                smashParticles = true;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            } else {
                if (firstHitSfx != null) {
                    firstHitSfx.Stop();
                }
                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                Celeste.Freeze(0.2f);
                if (refillOnBreak) {
                    player.RefillDash();
                }
                Break();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                SmashParticles(dir.Perpendicular());
                SmashParticles(-dir.Perpendicular());
            }
            return DashCollisionResults.Rebound;
        }

        private void Break() {
            Session session = (Scene as Level).Session;
            session.SetFlag(flag);

            sprite.Play("break");
            Collidable = false;
            DestroyStaticMovers();
            makeSparks = false;

            shakeCounter = 0f;
            shaker.On = false;

            if (musicStoreInSession) {
                if (!string.IsNullOrEmpty(music)) {
                    session.Audio.Music.Event = SFX.EventnameByHandle(music);
                }
                if (musicProgress >= 0) {
                    session.Audio.Music.SetProgress(musicProgress);
                }
                session.Audio.Apply();
            } else {
                if (!string.IsNullOrEmpty(music)) {
                    Audio.SetMusic(SFX.EventnameByHandle(music), startPlaying: false);
                }
                if (musicProgress >= 0) {
                    Audio.SetMusicParam("progress", musicProgress);
                }
                if (!string.IsNullOrEmpty(music) && Audio.CurrentMusicEventInstance != null) {
                    Audio.CurrentMusicEventInstance.start();
                }
            }
        }

        private void SmashParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int amount;
            if (dir == Vector2.UnitX) {
                direction = 0f;
                position = CenterRight - Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
                amount = (int) (Height / 8f) * 4;
            } else if (dir == -Vector2.UnitX) {
                direction = (float) Math.PI;
                position = CenterLeft + Vector2.UnitX * 12f;
                positionRange = Vector2.UnitY * (Height - 6f) * 0.5f;
                amount = (int) (Height / 8f) * 4;
            } else if (dir == Vector2.UnitY) {
                direction = (float) Math.PI / 2f;
                position = BottomCenter - Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
                amount = (int) (Width / 8f) * 4;
            } else {
                direction = -(float) Math.PI / 2f;
                position = TopCenter + Vector2.UnitY * 12f;
                positionRange = Vector2.UnitX * (Width - 6f) * 0.5f;
                amount = (int) (Width / 8f) * 4;
            }
            amount += 2;
            SceneAs<Level>().Particles.Emit(P_RecolouredSmash, amount, position, positionRange, direction);
        }
    }
}