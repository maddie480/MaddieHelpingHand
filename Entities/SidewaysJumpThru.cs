using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity(
        "MaxHelpingHand/SidewaysJumpThru = CreateSidewaysJumpThru",
        "MaxHelpingHand/OneWayInvisibleBarrierHorizontal = CreateOneWayInvisibleBarrierHorizontal"
    )]
    [Tracked]
    public class SidewaysJumpThru : Entity {

        private static ILHook hookOnUpdateSprite;

        private static FieldInfo actorMovementCounter = typeof(Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;
        private static bool hooksActiveNoJungleHelper = false;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;

            deactivateHooks();
            deactivateHooksNoJungleHelper();
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity =>
                entity.Name == "MaxHelpingHand/SidewaysJumpThru" || entity.Name == "MaxHelpingHand/OneWayInvisibleBarrierHorizontal") ?? false) ?? false) {

                activateHooks();

                if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "JungleHelper", Version = new Version(1, 0, 0) })
                    && (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "JungleHelper/ClimbableOneWayPlatform") ?? false) ?? false)) {

                    deactivateHooksNoJungleHelper();
                } else {
                    activateHooksNoJungleHelper();
                }
            } else {
                deactivateHooks();
                deactivateHooksNoJungleHelper();
            }
        }

        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow) {
            orig(self, startMode, snow);

            deactivateHooks();
            deactivateHooksNoJungleHelper();
        }

        public static void activateHooks() {
            if (hooksActive) {
                return;
            }
            hooksActive = true;

            Logger.Log("MaxHelpingHand/SidewaysJumpThru", "=== Activating sideways jumpthru hooks");

            // implement the basic collision between actors/platforms and sideways jumpthrus.
            using (new DetourContext { Before = { "*" } }) { // these don't always call the orig methods, better apply them first.
                On.Celeste.Platform.MoveHExactCollideSolids += onPlatformMoveHExactCollideSolids;
            }

            // block "climb hopping" on top of sideways jumpthrus, because this just looks weird.
            On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

            using (new DetourContext { Before = { "*" } }) { // let's take over Spring Collab 2020, we can break it, this is not a collab map!
                // mod collide checks to include sideways jumpthrus, so that the player behaves with them like with walls.
                IL.Celeste.Player.WallJumpCheck += modCollideChecks; // allow player to walljump off them
                IL.Celeste.Player.NormalUpdate += modCollideChecks; // get the wall slide effect
            }

            // one extra hook that kills the player momentum when hitting a jumpthru so that they don't get "stuck" on them.
            On.Celeste.Player.NormalUpdate += onPlayerNormalUpdate;
        }

        public static void activateHooksNoJungleHelper() {
            if (hooksActiveNoJungleHelper) {
                return;
            }
            hooksActiveNoJungleHelper = true;

            Logger.Log("MaxHelpingHand/SidewaysJumpThru", "=== Activating non Jungle Helper sideways jumpthru hooks");

            // implement the basic collision between actors/platforms and sideways jumpthrus.
            using (new DetourContext { Before = { "*" } }) { // these don't always call the orig methods, better apply them first.
                On.Celeste.Actor.MoveHExact += onActorMoveHExact;
            }

            using (new DetourContext { Before = { "*" } }) { // let's take over Spring Collab 2020, we can break it, this is not a collab map!
                // mod collide checks to include sideways jumpthrus, so that the player behaves with them like with walls.
                IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
                IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
                IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
                IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
                IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down

                // have the push animation when Madeline runs against a jumpthru for example
                hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);
            }
        }

        public static void deactivateHooks() {
            if (!hooksActive) {
                return;
            }
            hooksActive = false;

            Logger.Log("MaxHelpingHand/SidewaysJumpThru", "=== Deactivating sideways jumpthru hooks");

            On.Celeste.Platform.MoveHExactCollideSolids -= onPlatformMoveHExactCollideSolids;

            On.Celeste.Player.ClimbHopBlockedCheck -= onPlayerClimbHopBlockedCheck;

            IL.Celeste.Player.WallJumpCheck -= modCollideChecks;
            IL.Celeste.Player.NormalUpdate -= modCollideChecks;

            On.Celeste.Player.NormalUpdate -= onPlayerNormalUpdate;
        }

        public static void deactivateHooksNoJungleHelper() {
            if (!hooksActiveNoJungleHelper) {
                return;
            }
            hooksActiveNoJungleHelper = false;

            Logger.Log("MaxHelpingHand/SidewaysJumpThru", "=== Deactivating non Jungle Helper sideways jumpthru hooks");

            On.Celeste.Actor.MoveHExact -= onActorMoveHExact;

            IL.Celeste.Player.ClimbCheck -= modCollideChecks;
            IL.Celeste.Player.ClimbBegin -= modCollideChecks;
            IL.Celeste.Player.ClimbUpdate -= modCollideChecks;
            IL.Celeste.Player.SlipCheck -= modCollideChecks;
            IL.Celeste.Player.OnCollideH -= modCollideChecks;

            hookOnUpdateSprite?.Dispose();
        }

        private static bool onActorMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            // fall back to vanilla if no sideways jumpthru is in the room.
            if (self.Scene == null || !RoomContainsSidewaysJumpThrus(self))
                return orig(self, moveH, onCollide, pusher);

            Vector2 targetPosition = self.Position + Vector2.UnitX * moveH;
            int moveDirection = Math.Sign(moveH);
            int moveAmount = 0;
            bool movingLeftToRight = moveH > 0;
            while (moveH != 0) {
                bool didCollide = false;

                // check if colliding with a solid
                Solid solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * moveDirection);
                if (solid != null) {
                    didCollide = true;
                } else {
                    didCollide = CheckCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight);
                }

                if (didCollide) {
                    Vector2 movementCounter = (Vector2) actorMovementCounter.GetValue(self);
                    movementCounter.X = 0f;
                    actorMovementCounter.SetValue(self, movementCounter);
                    onCollide?.Invoke(new CollisionData {
                        Direction = Vector2.UnitX * moveDirection,
                        Moved = Vector2.UnitX * moveAmount,
                        TargetPosition = targetPosition,
                        Hit = solid,
                        Pusher = pusher
                    });
                    return true;
                }

                // continue moving
                moveAmount += moveDirection;
                moveH -= moveDirection;
                self.X += moveDirection;
            }
            return false;
        }

        public static bool RoomContainsSidewaysJumpThrus(Actor self) {
            return self.Scene.Tracker.CountEntities<SidewaysJumpThru>() != 0;
        }

        public static bool CheckCollisionWithSidewaysJumpthruWhileMoving(Actor self, int moveDirection, bool movingLeftToRight) {
            // check if colliding with a sideways jumpthru
            SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * moveDirection);
            if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight && (!(self is Seeker) || !jumpThru.letSeekersThrough)) {
                // there is a sideways jump-thru and we are moving in the opposite direction => collision
                return true;
            }

            return false;
        }

        private static bool onPlatformMoveHExactCollideSolids(On.Celeste.Platform.orig_MoveHExactCollideSolids orig, Platform self,
            int moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide) {
            // fall back to vanilla if no sideways jumpthru is in the room.
            if (self.Scene == null || self.Scene.Tracker.CountEntities<SidewaysJumpThru>() == 0)
                return orig(self, moveH, thruDashBlocks, onCollide);

            float x = self.X;
            int moveDirection = Math.Sign(moveH);
            int moveAmount = 0;
            Solid solid = null;
            bool movingLeftToRight = moveH > 0;
            bool collidedWithJumpthru = false;
            while (moveH != 0) {
                if (thruDashBlocks) {
                    // check if we have dash blocks to break on our way.
                    foreach (DashBlock entity in self.Scene.Tracker.GetEntities<DashBlock>()) {
                        if (self.CollideCheck(entity, self.Position + Vector2.UnitX * moveDirection)) {
                            entity.Break(self.Center, Vector2.UnitX * moveDirection, true, true);
                            self.SceneAs<Level>().Shake(0.2f);
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }
                    }
                }

                // check for collision with a solid
                solid = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * moveDirection);

                // check for collision with a sideways jumpthru
                SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * moveDirection);
                if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight) {
                    // there is a sideways jump-thru and we are moving in the opposite direction => collision
                    collidedWithJumpthru = true;
                }

                if (solid != null || collidedWithJumpthru) {
                    break;
                }

                // continue moving
                moveAmount += moveDirection;
                moveH -= moveDirection;
                self.X += moveDirection;
            }

            // actually move and call the collision callback if any
            self.X = x;
            self.MoveHExact(moveAmount);
            if (solid != null && onCollide != null) {
                onCollide(Vector2.UnitX * moveDirection, Vector2.UnitX * moveAmount, solid);
            }
            return solid != null || collidedWithJumpthru;
        }

        private static bool onPlayerClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
            bool vanillaCheck = orig(self);
            if (vanillaCheck)
                return vanillaCheck;

            // block climb hops on jumpthrus because those look weird
            return self.CollideCheckOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * (int) self.Facing);
        }

        private static void modCollideChecks(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            bool isClimb = il.Method.Name.Contains("Climb");
            bool isWallJump = il.Method.Name.Contains("WallJump") || il.Method.Name.Contains("NormalUpdate");

            while (cursor.Next != null) {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("MaxHelpingHand/SidewaysJumpThru", $"Patching Entity.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    cursor.Remove();
                    cursor.EmitDelegate<Func<Entity, Vector2, bool>>((self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (self.CollideCheck<Solid>(checkAtPosition))
                            return true;

                        // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                        if (self.Position.X == checkAtPosition.X)
                            return false;

                        return EntityCollideCheckWithSidewaysJumpthrus(self, checkAtPosition, isClimb, isWallJump);
                    });
                }

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("MaxHelpingHand/SidewaysJumpThru", $"Patching Scene.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    cursor.Remove();
                    cursor.EmitDelegate<Func<Scene, Vector2, bool>>((self, vector) => {
                        if (self.CollideCheck<Solid>(vector)) {
                            return true;
                        }
                        return SceneCollideCheckWithSidewaysJumpthrus(self, vector, isClimb, isWallJump);
                    });
                }

                cursor.Index++;
            }
        }

        public static bool EntityCollideCheckWithSidewaysJumpthrus(Entity self, Vector2 checkAtPosition, bool isClimb, bool isWallJump) {
            // our entity collides if this is with a jumpthru and we are colliding with the solid side of it.
            // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one) 
            // and we are checking the collision on the left side of the player for example.
            bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
            SidewaysJumpThru jumpthru = self.CollideFirstOutside<SidewaysJumpThru>(checkAtPosition);
            return jumpthru != null && self is Player && (jumpthru.AllowLeftToRight == collideOnLeftSideOfPlayer
                && (!isWallJump || jumpthru.allowWallJumping) && (!isClimb || jumpthru.allowClimbing))
                && jumpthru.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3;
        }

        public static bool SceneCollideCheckWithSidewaysJumpthrus(Scene self, Vector2 vector, bool isClimb, bool isWallJump) {
            SidewaysJumpThru jumpthru;
            if ((jumpthru = self.CollideFirst<SidewaysJumpThru>(vector)) != null) {
                return (!isWallJump || jumpthru.allowWallJumping) && (!isClimb || jumpthru.allowClimbing);
            }
            return false;
        }

        private static int onPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
            int result = orig(self);

            // kill speed if player is going towards a jumpthru.
            if (self.Speed.X != 0) {
                bool movingLeftToRight = self.Speed.X > 0;
                SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * Math.Sign(self.Speed.X));
                if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight) {
                    self.Speed.X = 0;
                }
            }

            return result;
        }

        // ======== Begin of entity code ========

        public static Entity CreateSidewaysJumpThru(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
            => new SidewaysJumpThru(entityData, offset);

        public static Entity CreateOneWayInvisibleBarrierHorizontal(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // horizontal one-way barriers are really just invisible sideways jumpthrus you can't climb or walljump on...
            entityData.Values["allowClimbing"] = false;
            entityData.Values["allowWallJumping"] = false;
            entityData.Values["texture"] = "MaxHelpingHand/invisible";
            return new SidewaysJumpThru(entityData, offset);
        }

        private int lines;
        private string overrideTexture;
        private float animationDelay;

        public bool AllowLeftToRight;

        private bool allowClimbing;
        private bool allowWallJumping;

        private bool letSeekersThrough;

        public SidewaysJumpThru(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay)
            : base(position) {

            lines = height / 8;
            AllowLeftToRight = allowLeftToRight;
            Depth = -60;
            this.overrideTexture = overrideTexture;
            this.animationDelay = animationDelay;

            float hitboxOffset = 0f;
            if (AllowLeftToRight)
                hitboxOffset = 3f;

            Collider = new Hitbox(5f, height, hitboxOffset, 0);
        }

        public SidewaysJumpThru(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f)) {

            allowClimbing = data.Bool("allowClimbing", true);
            allowWallJumping = data.Bool("allowWallJumping", true);
            letSeekersThrough = data.Bool("letSeekersThrough", false);
        }

        public override void Awake(Scene scene) {
            if (animationDelay > 0f) {
                for (int i = 0; i < lines; i++) {
                    Sprite jumpthruSprite = new Sprite(GFX.Game, "objects/jumpthru/" + overrideTexture);
                    jumpthruSprite.AddLoop("idle", "", animationDelay);

                    jumpthruSprite.Y = i * 8;
                    jumpthruSprite.Rotation = (float) (Math.PI / 2);
                    if (AllowLeftToRight)
                        jumpthruSprite.X = 8;
                    else
                        jumpthruSprite.Scale.Y = -1;

                    jumpthruSprite.Play("idle");
                    Add(jumpthruSprite);
                }
            } else {
                AreaData areaData = AreaData.Get(scene);
                string jumpthru = areaData.Jumpthru;
                if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default")) {
                    jumpthru = overrideTexture;
                }

                MTexture mTexture = GFX.Game["objects/jumpthru/" + jumpthru];
                int num = mTexture.Width / 8;
                for (int i = 0; i < lines; i++) {
                    int xTilePosition;
                    int yTilePosition;
                    if (i == 0) {
                        xTilePosition = 0;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, -1f))) ? 1 : 0);
                    } else if (i == lines - 1) {
                        xTilePosition = num - 1;
                        yTilePosition = ((!CollideCheck<Solid>(Position + new Vector2(0f, 1f))) ? 1 : 0);
                    } else {
                        xTilePosition = 1 + Calc.Random.Next(num - 2);
                        yTilePosition = Calc.Random.Choose(0, 1);
                    }
                    Image image = new Image(mTexture.GetSubtexture(xTilePosition * 8, yTilePosition * 8, 8, 8));
                    image.Y = i * 8;
                    image.Rotation = (float) (Math.PI / 2);

                    if (AllowLeftToRight)
                        image.X = 8;
                    else
                        image.Scale.Y = -1;

                    Add(image);
                }
            }
        }
    }
}