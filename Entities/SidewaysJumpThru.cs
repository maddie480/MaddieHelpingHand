using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
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
        private static ILHook hookOnOrigUpdate;

        private static MethodInfo playerJumpthruBoostBlockedCheck = typeof(Player).GetMethod("JumpThruBoostBlockedCheck", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool hooksActive = false;

        public static void Load() {
            On.Celeste.LevelLoader.ctor += onLevelLoad;
            On.Celeste.OverworldLoader.ctor += onOverworldLoad;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.ctor -= onLevelLoad;
            On.Celeste.OverworldLoader.ctor -= onOverworldLoad;

            deactivateHooks();
        }

        private static void onLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            if (session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => EntityNameRegistry.SidewaysJumpThrus.Contains(entity.Name)) ?? false) ?? false) {

                activateHooks();

            } else {
                deactivateHooks();
            }
        }

        private static void onOverworldLoad(On.Celeste.OverworldLoader.orig_ctor orig, OverworldLoader self, Overworld.StartMode startMode, HiresSnow snow) {
            orig(self, startMode, snow);

            if (startMode != (Overworld.StartMode) (-1)) { // -1 = in-game overworld from the collab utils
                deactivateHooks();
            }
        }

        private static void activateHooks() {
            if (hooksActive) {
                return;
            }
            hooksActive = true;

            Logger.Log(LogLevel.Debug, "MaxHelpingHand/SidewaysJumpThru", "=== Activating sideways jumpthru hooks");

            // implement the basic collision between actors/platforms and sideways jumpthrus.
            IL.Celeste.Actor.MoveHExact += addSidewaysJumpthrusInHorizontalMoveMethods;
            IL.Celeste.Platform.MoveHExactCollideSolids += addSidewaysJumpthrusInHorizontalMoveMethods;

            // block "climb hopping" on top of sideways jumpthrus, because this just looks weird.
            On.Celeste.Player.ClimbHopBlockedCheck += onPlayerClimbHopBlockedCheck;

            // mod collide checks to include sideways jumpthrus, so that the player behaves with them like with walls.
            IL.Celeste.Player.WallJumpCheck += modCollideChecks; // allow player to walljump off them
            IL.Celeste.Player.NormalUpdate += modCollideChecks; // get the wall slide effect
            IL.Celeste.Player.ClimbCheck += modCollideChecks; // allow player to climb on them
            IL.Celeste.Player.ClimbBegin += modCollideChecks; // if not applied, the player will clip through jumpthrus if trying to climb on them
            IL.Celeste.Player.ClimbUpdate += modCollideChecks; // when climbing, jumpthrus are handled like walls
            IL.Celeste.Player.SlipCheck += modCollideChecks; // make climbing on jumpthrus not slippery
            IL.Celeste.Player.OnCollideH += modCollideChecks; // handle dashes against jumpthrus properly, without "shifting" down
            IL.Celeste.Seeker.OnCollideH += modCollideChecks; // make seekers bump against jumpthrus, instead of vibrating at maximum velocity
            hookOnOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), modCollideChecks); // patch wall retention to include sideways jumpthru detection as well

            // don't make Madeline duck when dashing against a sideways jumpthru
            On.Celeste.Player.DuckFreeAt += preventDuckWhenDashingAgainstJumpthru;

            // one extra hook that kills the player momentum when hitting a jumpthru so that they don't get "stuck" on them.
            On.Celeste.Player.NormalUpdate += onPlayerNormalUpdate;

            On.Celeste.SurfaceIndex.GetPlatformByPriority += modSurfaceIndexGetPlatformByPriority;

            // have the push animation when Madeline runs against a jumpthru for example
            hookOnUpdateSprite = new ILHook(typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modCollideChecks);
        }

        private static void deactivateHooks() {
            if (!hooksActive) {
                return;
            }
            hooksActive = false;

            Logger.Log(LogLevel.Debug, "MaxHelpingHand/SidewaysJumpThru", "=== Deactivating sideways jumpthru hooks");

            IL.Celeste.Actor.MoveHExact -= addSidewaysJumpthrusInHorizontalMoveMethods;
            IL.Celeste.Platform.MoveHExactCollideSolids -= addSidewaysJumpthrusInHorizontalMoveMethods;

            On.Celeste.Player.ClimbHopBlockedCheck -= onPlayerClimbHopBlockedCheck;

            IL.Celeste.Player.WallJumpCheck -= modCollideChecks;
            IL.Celeste.Player.NormalUpdate -= modCollideChecks;
            IL.Celeste.Player.ClimbCheck -= modCollideChecks;
            IL.Celeste.Player.ClimbBegin -= modCollideChecks;
            IL.Celeste.Player.ClimbUpdate -= modCollideChecks;
            IL.Celeste.Player.SlipCheck -= modCollideChecks;
            IL.Celeste.Player.OnCollideH -= modCollideChecks;
            IL.Celeste.Seeker.OnCollideH -= modCollideChecks;
            hookOnOrigUpdate?.Dispose();

            On.Celeste.Player.DuckFreeAt -= preventDuckWhenDashingAgainstJumpthru;

            On.Celeste.Player.NormalUpdate -= onPlayerNormalUpdate;

            On.Celeste.SurfaceIndex.GetPlatformByPriority -= modSurfaceIndexGetPlatformByPriority;

            hookOnUpdateSprite?.Dispose();
        }

        private class FakeCollidingSolid : Solid {
            public FakeCollidingSolid() : base(Vector2.Zero, 0, 0, false) { }
        }

        private static void addSidewaysJumpthrusInHorizontalMoveMethods(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("CollideFirst"))) {
                Logger.Log("MaxHelpingHand/SidewaysJumpThru", $"Injecting sideways jumpthru check at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Solid, Entity, int, Solid>>((orig, self, moveH) => {
                    if (orig != null) return orig;

                    int moveDirection = Math.Sign(moveH);
                    bool movingLeftToRight = moveH > 0;
                    if (getCollisionWithSidewaysJumpthruWhileMoving(self, moveDirection, movingLeftToRight) is SidewaysJumpThru jumpThru) {
                        FakeCollidingSolid fakeSolid = new FakeCollidingSolid();
                        if (self is Player player && player.DashAttacking && jumpThru is AttachedSidewaysJumpThru attachedSidewaysJumpThru) {
                            fakeSolid.OnDashCollide = attachedSidewaysJumpThru.OnDashCollide;
                        }
                        return fakeSolid; // so Celeste will call that callback and not discard return value.
                    }

                    return null;
                });
            }
        }

        private static SidewaysJumpThru getCollisionWithSidewaysJumpthruWhileMoving(Entity self, int moveDirection, bool movingLeftToRight) {
            // check if colliding with a sideways jumpthru
            SidewaysJumpThru jumpThru = self.CollideFirstOutside<SidewaysJumpThru>(self.Position + Vector2.UnitX * moveDirection);
            if (jumpThru != null && jumpThru.AllowLeftToRight != movingLeftToRight && (!(self is Seeker) || !jumpThru.letSeekersThrough)) {
                // there is a sideways jump-thru and we are moving in the opposite direction => collision
                return jumpThru;
            }

            return null;
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

            // create a Vector2 temporary variable
            VariableDefinition checkAtPositionStore = new VariableDefinition(il.Import(typeof(Vector2)));
            il.Body.Variables.Add(checkAtPositionStore);

            bool isClimb = il.Method.Name.Contains("Climb");
            bool isWallJump = il.Method.Name.Contains("WallJump") || il.Method.Name.Contains("NormalUpdate");

            while (cursor.Next != null) {
                Instruction next = cursor.Next;

                // we want to replace all CollideChecks with solids here.
                if (next.OpCode == OpCodes.Call && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Entity::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("MaxHelpingHand/SidewaysJumpThru", $"Patching Entity.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: false);

                    // mod the result
                    cursor.EmitDelegate<Func<bool, Entity, Vector2, bool>>((orig, self, checkAtPosition) => {
                        // we still want to check for solids...
                        if (orig) {
                            return true;
                        }

                        // if we are not checking a side, this certainly has nothing to do with jumpthrus.
                        if (self.Position.X == checkAtPosition.X)
                            return false;

                        return entityCollideCheckWithSidewaysJumpthrus(self, checkAtPosition, isClimb, isWallJump);
                    });
                }

                if (next.OpCode == OpCodes.Callvirt && (next.Operand as MethodReference)?.FullName == "System.Boolean Monocle.Scene::CollideCheck<Celeste.Solid>(Microsoft.Xna.Framework.Vector2)") {
                    Logger.Log("MaxHelpingHand/SidewaysJumpThru", $"Patching Scene.CollideCheck to include sideways jumpthrus at {cursor.Index} in IL for {il.Method.Name}");

                    callOrigMethodKeepingEverythingOnStack(cursor, checkAtPositionStore, isSceneCollideCheck: true);

                    cursor.EmitDelegate<Func<bool, Scene, Vector2, bool>>((orig, self, vector) => {
                        if (orig) {
                            return true;
                        }
                        return sceneCollideCheckWithSidewaysJumpthrus(self, vector, isClimb, isWallJump);
                    });
                }

                cursor.Index++;
            }
        }

        private static bool preventDuckWhenDashingAgainstJumpthru(On.Celeste.Player.orig_DuckFreeAt orig, Player self, Vector2 at) {
            if (orig(self, at)) {
                // check for collisions against sideways jumpthrus while ducked.
                Collider origHitbox = self.Collider;
                self.Collider = new Hitbox(8f, 6f, -4f, -6f); // duck hitbox
                bool result = !entityCollideCheckWithSidewaysJumpthrus(self, at, false, false);
                self.Collider = origHitbox;

                return result;
            }
            return false;
        }

        private static void callOrigMethodKeepingEverythingOnStack(ILCursor cursor, VariableDefinition checkAtPositionStore, bool isSceneCollideCheck) {
            // store the position in the local variable
            cursor.Emit(OpCodes.Stloc, checkAtPositionStore);
            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);

            // let vanilla call CollideCheck
            cursor.Index++;

            // reload the parameters
            cursor.Emit(OpCodes.Ldarg_0);
            if (isSceneCollideCheck) {
                cursor.Emit(OpCodes.Call, typeof(Entity).GetProperty("Scene").GetGetMethod());
            }

            cursor.Emit(OpCodes.Ldloc, checkAtPositionStore);
        }

        private static bool entityCollideCheckWithSidewaysJumpthrus(Entity self, Vector2 checkAtPosition, bool isClimb, bool isWallJump) {
            // our entity collides if this is with a jumpthru and we are colliding with the solid side of it.
            // we are in this case if the jumpthru is left to right (the "solid" side of it is the right one)
            // and we are checking the collision on the left side of the player for example.
            bool collideOnLeftSideOfPlayer = (self.Position.X > checkAtPosition.X);
            SidewaysJumpThru jumpthru = self.CollideFirstOutside<SidewaysJumpThru>(checkAtPosition);
            return jumpthru != null && (self is Player || self is Seeker) && (jumpthru.AllowLeftToRight == collideOnLeftSideOfPlayer
                && (!isWallJump || jumpthru.allowWallJumping) && (!isClimb || jumpthru.allowClimbing))
                && jumpthru.Bottom >= self.Top + checkAtPosition.Y - self.Position.Y + 3;
        }

        private static bool sceneCollideCheckWithSidewaysJumpthrus(Scene self, Vector2 vector, bool isClimb, bool isWallJump) {
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

        private static Platform modSurfaceIndexGetPlatformByPriority(On.Celeste.SurfaceIndex.orig_GetPlatformByPriority orig, List<Entity> platforms) {
            // if vanilla already has platforms to get the sound index from, use those.
            if (platforms.Count != 0) {
                return orig(platforms);
            }

            // check if we are climbing a sideways jumpthru.
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                SidewaysJumpThru jumpThru = player.CollideFirst<SidewaysJumpThru>(player.Center + Vector2.UnitX * (float) player.Facing);
                if (jumpThru != null && jumpThru.surfaceIndex != -1) {
                    // yes we are! pass it off as a Platform so that the game can get its surface index later.
                    return new WallSoundIndexHolder(jumpThru.surfaceIndex);
                }
            }

            return orig(platforms);
        }

        // this is a dummy Platform that is just here to hold a wall surface sound index, that the game will read.
        // it isn't actually used as a platform!
        private class WallSoundIndexHolder : Platform {
            private int wallSoundIndex;

            public WallSoundIndexHolder(int wallSoundIndex) : base(Vector2.Zero, false) {
                this.wallSoundIndex = wallSoundIndex;
            }

            public override void MoveHExact(int move) {
                throw new NotImplementedException();
            }

            public override void MoveVExact(int move) {
                throw new NotImplementedException();
            }

            public override int GetWallSoundIndex(Player player, int side) {
                return wallSoundIndex;
            }
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
        private int surfaceIndex = -1;

        public bool AllowLeftToRight;

        private bool allowClimbing;
        private bool allowWallJumping;

        private bool letSeekersThrough;

        private bool pushPlayer;

        public SidewaysJumpThru(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, int surfaceIndex, bool pushPlayer)
            : this(position, height, allowLeftToRight, overrideTexture, animationDelay, allowClimbing, allowWallJumping, letSeekersThrough, surfaceIndex) {

            this.pushPlayer = pushPlayer;
        }

        public SidewaysJumpThru(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough, int surfaceIndex)
            : this(position, height, allowLeftToRight, overrideTexture, animationDelay, allowClimbing, allowWallJumping, letSeekersThrough) {

            this.surfaceIndex = surfaceIndex;
        }

        public SidewaysJumpThru(Vector2 position, int height, bool allowLeftToRight, string overrideTexture, float animationDelay, bool allowClimbing, bool allowWallJumping, bool letSeekersThrough)
            : this(position, height, allowLeftToRight, overrideTexture, animationDelay) {

            this.allowClimbing = allowClimbing;
            this.allowWallJumping = allowWallJumping;
            this.letSeekersThrough = letSeekersThrough;
        }

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
            : this(data.Position + offset, data.Height, !data.Bool("left"), data.Attr("texture", "default"), data.Float("animationDelay", 0f),
                  data.Bool("allowClimbing", true), data.Bool("allowWallJumping", true), data.Bool("letSeekersThrough", false), data.Int("surfaceIndex", -1),
                  data.Bool("pushPlayer", false)) { }

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

        public override void Update() {
            base.Update();

            // if we are supposed to push the player and the player is hitting us...
            Player p;
            if (pushPlayer && (p = CollideFirst<Player>()) != null) {
                DynData<Player> playerData = new DynData<Player>(p);
                if (AllowLeftToRight) {
                    // player is moving right, not on the ground, not climbing, not blocked => push them to the right
                    if (p.Speed.X >= 0f && !playerData.Get<bool>("onGround") && (p.StateMachine.State != 1 || playerData.Get<int>("lastClimbMove") == -1)
                        && !((bool) playerJumpthruBoostBlockedCheck.Invoke(p, new object[0]))) {

                        p.MoveH(40f * Engine.DeltaTime);
                    }
                } else {
                    // player is moving left, not on the ground, not climbing, not blocked => push them to the left
                    if (p.Speed.X <= 0f && !playerData.Get<bool>("onGround") && (p.StateMachine.State != 1 || playerData.Get<int>("lastClimbMove") == -1)
                        && !((bool) playerJumpthruBoostBlockedCheck.Invoke(p, new object[0]))) {

                        p.MoveH(-40f * Engine.DeltaTime);
                    }
                }
            }
        }
    }
}