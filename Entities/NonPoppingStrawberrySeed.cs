using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/NonPoppingStrawberry")]
    [RegisterStrawberry(tracked: true, blocksCollection: false)]
    public class NonPoppingStrawberrySeed : StrawberrySeed {
        public static bool ReplaceSeeds = false; // only true while a NonPoppingStrawberry is being built
        private static bool strawberryPulseEffectOnSeeds; // allows to pass the setting indirectly to the NonPoppingStrawberrySeed constructor

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            ReplaceSeeds = true;
            strawberryPulseEffectOnSeeds = entityData.Bool("pulseEffectOnSeeds", defaultValue: true);
            Entity strawberry = new Strawberry(entityData, offset, new EntityID(levelData.Name, entityData.ID));
            ReplaceSeeds = false;
            return strawberry;
        }

        public static void Load() {
            IL.Celeste.Strawberry.ctor += replaceStrawberrySeeds;
            IL.Celeste.StrawberrySeed.Update += preventLosingSeeds;
        }

        public static void Unload() {
            IL.Celeste.Strawberry.ctor -= replaceStrawberrySeeds;
            IL.Celeste.StrawberrySeed.Update -= preventLosingSeeds;
        }

        private static void replaceStrawberrySeeds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchNewobj<StrawberrySeed>())) {
                Logger.Log("MaxHelpingHand/NonPoppingStrawberrySeed", $"Replacing strawberry seeds at {cursor.Index} in IL for Strawberry constructor");

                // not that much of an easy win. if ReplaceSeeds is true, build a NonPoppingStrawberrySeed instead of a StrawberrySeed.
                cursor.Emit(OpCodes.Ldsfld, typeof(NonPoppingStrawberrySeed).GetField("ReplaceSeeds"));
                cursor.Emit(OpCodes.Brfalse, cursor.Next); // if ReplaceSeeds is false, jump over the code we will insert.
                cursor.Emit(OpCodes.Newobj, typeof(NonPoppingStrawberrySeed).GetConstructor(new Type[] { typeof(Strawberry), typeof(Vector2), typeof(int), typeof(bool) }));
                cursor.Emit(OpCodes.Br, cursor.Next.Next); // jump over newobj StrawberrySeed

                cursor.Index++;
            }
        }

        private static void preventLosingSeeds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_LoseShards"))) {
                Logger.Log("MaxHelpingHand/NonPoppingStrawberrySeed", $"Preventing strawberry seeds from popping at {cursor.Index} in IL for StrawberrySeed.Update");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, StrawberrySeed, bool>>(preventPopping);
            }
        }

        private static bool preventPopping(bool orig, StrawberrySeed self) {
            if (self is NonPoppingStrawberrySeed) {
                return false; // pretend the player is not on ground, so that the seed does not pop.
            }
            return orig;
        }

        private readonly bool pulseEffect;

        public NonPoppingStrawberrySeed(Strawberry strawberry, Vector2 position, int index, bool ghost) : base(strawberry, position, index, ghost) {
            pulseEffect = strawberryPulseEffectOnSeeds;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (!pulseEffect) {
                // remove the sprite OnFrameChange callback, which is responsible for the pulse effect.
                Get<Sprite>().OnFrameChange = null;
            }
        }
    }
}
