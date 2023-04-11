using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/MadelineSilhouetteTrigger")]
    public class MadelineSilhouetteTrigger : Trigger {
        private static Color silhouetteOutOfStaminaZeroDashBlinkColor = Calc.HexToColor("348DC1");

        public static void Load() {
            On.Celeste.Player.Render += PlayerRenderHook;
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.Player.ResetSprite += onPlayerResetSprite;

            using (new DetourContext() { Before = { "*" } }) { // we won't break Spring Collab 2020, but it will break us if it goes first.
                IL.Celeste.Player.Render += patchPlayerRender;
            }
        }

        public static void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
            IL.Celeste.Player.Render -= patchPlayerRender;
            On.Celeste.Player.ResetSprite -= onPlayerResetSprite;
            On.Celeste.Player.Render -= PlayerRenderHook;
        }
        
        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            new DynData<PlayerSprite>(self)["MadelineIsSilhouette"] = MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette;
            orig(self, mode);
        }

        private static void patchPlayerRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the usage of the Red color
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_Red"))) {
                Logger.Log("MaxHelpingHand/MadelineSilhouetteTrigger", $"Patching silhouette hair color at {cursor.Index} in IL code for Player.Render()");

                // when Madeline blinks red, make the hair blink red.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Player, Color>>((color, player) => {
                    if (MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette) {
                        if (player.Dashes == 0) {
                            // blink to another shade of blue instead, to avoid red/blue flashes that are hard on the eyes.
                            color = silhouetteOutOfStaminaZeroDashBlinkColor;
                        }
                        player.Hair.Color = color;
                    }
                    return color;
                });
            }

            // jump to the usage of the White color
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("MaxHelpingHand/MadelineSilhouetteTrigger", $"Patching silhouette color at {cursor.Index} in IL code for Player.Render()");

                // intercept Color.White (or whatever Spring Collab 2020 returned) and mod it if required.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Player, Color>>((orig, self) => {
                    if (MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette) {
                        return self.Hair.Color;
                    } else {
                        return orig;
                    }
                });
            }
        }

        private static void onPlayerAdded() { /* this is completely useless but Extended Variants 0.19.6 and earlier will crash if they don't find this method. */  }

        private static void refreshPlayerSpriteMode(Player player, bool enableSilhouette) {
            PlayerSpriteMode targetSpriteMode;
            if (enableSilhouette) {
                targetSpriteMode = PlayerSpriteMode.Playback;
            } else {
                targetSpriteMode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
            }

            if (player.Active) {
                player.ResetSpriteNextFrame(targetSpriteMode);
            } else {
                player.ResetSprite(targetSpriteMode);
            }
        }

        private static void onPlayerResetSprite(On.Celeste.Player.orig_ResetSprite orig, Player self, PlayerSpriteMode mode) {
            // filter all calls to ResetSprite when MadelineIsSilhouette is enabled, only the ones with Playback will go through.
            // this prevents Madeline from turning back into normal when the Other Self variant is toggled.
            if (!MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette || mode == PlayerSpriteMode.Playback) {
                orig(self, mode);
            }
        }


        private bool enable;

        public MadelineSilhouetteTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enable = data.Bool("enable", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            bool oldValue = MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette;
            MaxHelpingHandModule.Instance.Session.MadelineIsSilhouette = enable;

            // if the value changed...
            if (oldValue != enable) {
                // switch modes right now. this uses the same way as turning the "Other Self" variant on.
                refreshPlayerSpriteMode(player, enable);
            }
        }


        private static void PlayerRenderHook(On.Celeste.Player.orig_Render orig, Player self) {
            if (self == null) { orig(self); return; }

            DynData<PlayerSprite> spriteData = new DynData<PlayerSprite>(self.Sprite);
            if (spriteData["MadelineIsSilhouette"] != null && spriteData.Get<bool?>("MadelineIsSilhouette") == true) {
                spriteData["MadelineIsSilhouette"] = null;

                if (self.StateMachine.State == Player.StIntroRespawn || self.Sprite.Mode == PlayerSpriteMode.Madeline || self.Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack || self.Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline) {
                    refreshPlayerSpriteMode(self, true);
                }
            }
            orig(self);
        }
    }
}
