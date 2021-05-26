using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // a near vanilla FloatySpaceBlock copy-paste, except it also supports attached sideways jumpthrus.
    // for example, a player dashing against an attached sideways jumpthru will push the block, and a player hanging to
    // an attached sideways jumpthru will make the block sink.
    // because extending or patching the vanilla one would be a bit too painful...
    [CustomEntity("MaxHelpingHand/FloatySpaceBlockWithAttachedSidewaysJumpthruSupport")]
    [Tracked]
    public class FloatySpaceBlockWithAttachedSidewaysJumpthruSupport : Solid {
        private TileGrid tiles;
        private char tileType;

        private float sineWave;
        private float sinkTimer;
        private float yLerp;
        private float dashEase;
        private Vector2 dashDirection;

        private bool awake;

        private FloatySpaceBlockWithAttachedSidewaysJumpthruSupport master;
        private List<FloatySpaceBlockWithAttachedSidewaysJumpthruSupport> group;
        private List<JumpThru> jumpthrus;
        private List<AttachedSidewaysJumpThru> attachedSidewaysJumpthrus;
        private Dictionary<Platform, Vector2> moves;
        private Point groupBoundsMin;
        private Point groupBoundsMax;

        public bool HasGroup {
            get;
            private set;
        }

        public bool MasterOfGroup {
            get;
            private set;
        }

        public FloatySpaceBlockWithAttachedSidewaysJumpthruSupport(Vector2 position, float width, float height, char tileType, bool disableSpawnOffset)
            : base(position, width, height, safe: true) {
            this.tileType = tileType;
            Depth = -9000;
            Add(new LightOcclude());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            if (!disableSpawnOffset) {
                sineWave = Calc.Random.NextFloat((float) Math.PI * 2f);
            } else {
                sineWave = 0f;
            }
        }

        public FloatySpaceBlockWithAttachedSidewaysJumpthruSupport(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("disableSpawnOffset")) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            awake = true;
            if (!HasGroup) {
                MasterOfGroup = true;
                moves = new Dictionary<Platform, Vector2>();
                group = new List<FloatySpaceBlockWithAttachedSidewaysJumpthruSupport>();
                jumpthrus = new List<JumpThru>();
                attachedSidewaysJumpthrus = new List<AttachedSidewaysJumpThru>();
                groupBoundsMin = new Point((int) base.X, (int) base.Y);
                groupBoundsMax = new Point((int) base.Right, (int) base.Bottom);
                addToGroupAndFindChildren(this);
                Rectangle rectangle = new Rectangle(groupBoundsMin.X / 8, groupBoundsMin.Y / 8, (groupBoundsMax.X - groupBoundsMin.X) / 8 + 1, (groupBoundsMax.Y - groupBoundsMin.Y) / 8 + 1);
                VirtualMap<char> tilemap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
                foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport block in group) {
                    int startX = (int) (block.X / 8f) - rectangle.X;
                    int startY = (int) (block.Y / 8f) - rectangle.Y;
                    int widthTiles = (int) (block.Width / 8f);
                    int heightTiles = (int) (block.Height / 8f);
                    for (int i = startX; i < startX + widthTiles; i++) {
                        for (int j = startY; j < startY + heightTiles; j++) {
                            tilemap[i, j] = tileType;
                        }
                    }
                }
                tiles = GFX.FGAutotiler.GenerateMap(tilemap, new Autotiler.Behaviour {
                    EdgesExtend = false,
                    EdgesIgnoreOutOfLevel = false,
                    PaddingIgnoreOutOfLevel = false
                }).TileGrid;
                tiles.Position = new Vector2(groupBoundsMin.X - X, groupBoundsMin.Y - Y);
                Add(tiles);
            }
            tryToInitPosition();
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            if (sm.Entity is Spring) {
                switch ((sm.Entity as Spring).Orientation) {
                    case Spring.Orientations.Floor:
                        sinkTimer = 0.5f;
                        break;
                    case Spring.Orientations.WallLeft:
                        dashEase = 1f;
                        dashDirection = -Vector2.UnitX;
                        break;
                    case Spring.Orientations.WallRight:
                        dashEase = 1f;
                        dashDirection = Vector2.UnitX;
                        break;
                }
            }
        }

        private void tryToInitPosition() {
            if (MasterOfGroup) {
                foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport item in group) {
                    if (!item.awake) {
                        return;
                    }
                }
                moveToTarget();
            } else {
                master.tryToInitPosition();
            }
        }

        private void addToGroupAndFindChildren(FloatySpaceBlockWithAttachedSidewaysJumpthruSupport from) {
            if (from.X < groupBoundsMin.X) {
                groupBoundsMin.X = (int) from.X;
            }
            if (from.Y < groupBoundsMin.Y) {
                groupBoundsMin.Y = (int) from.Y;
            }
            if (from.Right > groupBoundsMax.X) {
                groupBoundsMax.X = (int) from.Right;
            }
            if (from.Bottom > groupBoundsMax.Y) {
                groupBoundsMax.Y = (int) from.Bottom;
            }
            from.HasGroup = true;
            from.OnDashCollide = onDash;
            group.Add(from);
            moves.Add(from, from.Position);
            if (from != this) {
                from.master = this;
            }
            foreach (JumpThru jumpthru in Scene.CollideAll<JumpThru>(new Rectangle((int) from.X - 1, (int) from.Y, (int) from.Width + 2, (int) from.Height))) {
                if (!jumpthrus.Contains(jumpthru)) {
                    addJumpThru(jumpthru);
                }
            }
            foreach (JumpThru jumpthru in Scene.CollideAll<JumpThru>(new Rectangle((int) from.X, (int) from.Y - 1, (int) from.Width, (int) from.Height + 2))) {
                if (!jumpthrus.Contains(jumpthru)) {
                    addJumpThru(jumpthru);
                }
            }
            foreach (AttachedSidewaysJumpThru jumpthru in Scene.CollideAll<AttachedSidewaysJumpThru>(new Rectangle((int) from.X - 1, (int) from.Y, (int) from.Width + 2, (int) from.Height))) {
                if (!attachedSidewaysJumpthrus.Contains(jumpthru)) {
                    addAttachedSidewaysJumpThru(jumpthru);
                }
            }
            foreach (AttachedSidewaysJumpThru jumpthru in Scene.CollideAll<AttachedSidewaysJumpThru>(new Rectangle((int) from.X, (int) from.Y - 1, (int) from.Width, (int) from.Height + 2))) {
                if (!attachedSidewaysJumpthrus.Contains(jumpthru)) {
                    addAttachedSidewaysJumpThru(jumpthru);
                }
            }
            foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport block in Scene.Tracker.GetEntities<FloatySpaceBlockWithAttachedSidewaysJumpthruSupport>()) {
                if (!block.HasGroup && block.tileType == tileType && (Scene.CollideCheck(new Rectangle((int) from.X - 1, (int) from.Y, (int) from.Width + 2, (int) from.Height), block)
                    || Scene.CollideCheck(new Rectangle((int) from.X, (int) from.Y - 1, (int) from.Width, (int) from.Height + 2), block))) {

                    addToGroupAndFindChildren(block);
                }
            }
        }

        private void addJumpThru(JumpThru jp) {
            jp.OnDashCollide = onDash;
            jumpthrus.Add(jp);
            moves.Add(jp, jp.Position);
            foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport block in Scene.Tracker.GetEntities<FloatySpaceBlockWithAttachedSidewaysJumpthruSupport>()) {
                if (!block.HasGroup && block.tileType == tileType && Scene.CollideCheck(new Rectangle((int) jp.X - 1, (int) jp.Y, (int) jp.Width + 2, (int) jp.Height), block)) {
                    addToGroupAndFindChildren(block);
                }
            }
        }

        private void addAttachedSidewaysJumpThru(AttachedSidewaysJumpThru jp) {
            jp.OnDashCollide = onDash;
            attachedSidewaysJumpthrus.Add(jp);
            foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport block in Scene.Tracker.GetEntities<FloatySpaceBlockWithAttachedSidewaysJumpthruSupport>()) {
                if (!block.HasGroup && block.tileType == tileType && Scene.CollideCheck(new Rectangle((int) jp.X, (int) jp.Y - 1, (int) jp.Width, (int) jp.Height + 2), block)) {
                    addToGroupAndFindChildren(block);
                }
            }
        }

        private DashCollisionResults onDash(Player player, Vector2 direction) {
            if (MasterOfGroup && dashEase <= 0.2f) {
                dashEase = 1f;
                dashDirection = direction;
            }
            return DashCollisionResults.NormalOverride;
        }

        public override void Update() {
            base.Update();
            if (MasterOfGroup) {
                bool blockHasPlayerOnIt = false;
                foreach (FloatySpaceBlockWithAttachedSidewaysJumpthruSupport block in group) {
                    if (block.HasPlayerRider()) {
                        blockHasPlayerOnIt = true;
                        break;
                    }
                }
                if (!blockHasPlayerOnIt) {
                    foreach (JumpThru jumpthru in jumpthrus) {
                        if (jumpthru.HasPlayerRider()) {
                            blockHasPlayerOnIt = true;
                            break;
                        }
                    }
                }
                if (!blockHasPlayerOnIt) {
                    foreach (AttachedSidewaysJumpThru jumpthru in attachedSidewaysJumpthrus) {
                        if (SidewaysMovingPlatform.GetPlayerClimbing(jumpthru, jumpthru.Left) != null) {
                            blockHasPlayerOnIt = true;
                            break;
                        }
                    }
                }
                if (blockHasPlayerOnIt) {
                    sinkTimer = 0.3f;
                } else if (sinkTimer > 0f) {
                    sinkTimer -= Engine.DeltaTime;
                }
                if (sinkTimer > 0f) {
                    yLerp = Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
                } else {
                    yLerp = Calc.Approach(yLerp, 0f, 1f * Engine.DeltaTime);
                }
                sineWave += Engine.DeltaTime;
                dashEase = Calc.Approach(dashEase, 0f, Engine.DeltaTime * 1.5f);
                moveToTarget();
            }
            LiftSpeed = Vector2.Zero;
        }

        private void moveToTarget() {
            float sine = (float) Math.Sin(sineWave) * 4f;
            Vector2 displacement = Calc.YoYo(Ease.QuadIn(dashEase)) * dashDirection * 8f;
            for (int i = 0; i < 2; i++) {
                foreach (KeyValuePair<Platform, Vector2> move in moves) {
                    Platform platform = move.Key;
                    bool hasPlayer = false;
                    JumpThru jumpThru = platform as JumpThru;
                    Solid solid = platform as Solid;
                    if ((jumpThru != null && jumpThru.HasRider()) || (solid?.HasRider() ?? false)) {
                        hasPlayer = true;
                    }
                    if ((hasPlayer || i != 0) && (!hasPlayer || i != 1)) {
                        Vector2 value = move.Value;
                        float yMove = MathHelper.Lerp(value.Y, value.Y + 12f, Ease.SineInOut(yLerp)) + sine;
                        platform.MoveToY(yMove + displacement.Y);
                        platform.MoveToX(value.X + displacement.X);
                    }
                }
            }
        }

        public override void OnShake(Vector2 amount) {
            if (!MasterOfGroup) {
                return;
            }
            base.OnShake(amount);
            tiles.Position += amount;
            foreach (JumpThru jumpthru in jumpthrus) {
                foreach (Component component in jumpthru.Components) {
                    Image image = component as Image;
                    if (image != null) {
                        image.Position += amount;
                    }
                }
            }
            foreach (AttachedSidewaysJumpThru jumpthru in attachedSidewaysJumpthrus) {
                foreach (Component component in jumpthru.Components) {
                    Image image = component as Image;
                    if (image != null) {
                        image.Position += amount;
                    }
                }
            }
        }
    }

}
