using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.MaxHelpingHand.Entities;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public static class SpeedrunToolInterop {
        [ModImportName("SpeedrunTool.SaveLoad")]
        private static class Interop {
            public delegate object RegisterSaveLoadActionDelegate(
                Action<Dictionary<Type, Dictionary<string, object>>, Level> saveState,
                Action<Dictionary<Type, Dictionary<string, object>>, Level> loadState,
                Action clearState,
                Action<Level> beforeSaveState,
                Action<Level> beforeLoadState,
                Action preCloneEntities);

#pragma warning disable CS0649 // those fields are assigned by ModInterop
            public static RegisterSaveLoadActionDelegate RegisterSaveLoadAction;
            public static Action<object> Unregister;
            public static Func<object, object> DeepClone;
#pragma warning restore CS0649
        }

        private static object saveLoadAction = null;

        internal static void Initialize() {
            typeof(Interop).ModInterop();
            if (Interop.RegisterSaveLoadAction is { } register) {
                saveLoadAction = register.Invoke(
                    saveState: (savedValues, _) => {
                        var dict = new Dictionary<string, object> {
                            [nameof(BlackholeCustomColors.colorsMildOverride)] = BlackholeCustomColors.colorsMildOverride,
                            [nameof(MovingFlagTouchSwitch.flagMapping)] = MovingFlagTouchSwitch.flagMapping
                        };
                        savedValues[typeof(SpeedrunToolInterop)] = dict.DeepClone();
                    },
                    loadState: (savedValues, _) => {
                        var dict = savedValues[typeof(SpeedrunToolInterop)].DeepClone();
                        BlackholeCustomColors.colorsMildOverride = (Color[])dict[nameof(BlackholeCustomColors.colorsMildOverride)];
                        MovingFlagTouchSwitch.flagMapping = (Dictionary<Entity, Dictionary<string, object>>)dict[nameof(MovingFlagTouchSwitch.flagMapping)];
                    },
                    null, null, null, null
                );
            }
        }

        internal static void Unload() {
            if (saveLoadAction is not null && Interop.Unregister is { } unregister) {
                unregister.Invoke(saveLoadAction);
            }
        }
        private static T DeepClone<T>(this T from) where T : notnull {
            return (T)Interop.DeepClone(from) ?? default;
        }
    }
}