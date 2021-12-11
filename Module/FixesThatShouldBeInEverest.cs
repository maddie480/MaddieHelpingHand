using Celeste.Mod.Meta;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Net;

namespace Celeste.Mod.MaxHelpingHand.Module {
    // these fixes would totally fit in Everest, but since it is uncertain that pull requests are even going to be reviewed at this point,
    // I'm implementing them in Helping Hand in the meantime.
    public class FixesThatShouldBeInEverest {
        private static ILHook hookOrigLoadLevel;

        public static void Load() {
            IL.Celeste.Mod.Helpers.ModUpdaterHelper.DownloadModUpdateList += useCompressedWebClient;

            hookOrigLoadLevel = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), fixHeartsDisappearingAfterSaveAndQuit);
        }

        public static void Unload() {
            IL.Celeste.Mod.Helpers.ModUpdaterHelper.DownloadModUpdateList -= useCompressedWebClient;

            hookOrigLoadLevel?.Dispose();
            hookOrigLoadLevel = null;
        }


        // === reimplementation of https://github.com/EverestAPI/Everest/pull/400

        private class CompressedWebClient : WebClient {
            protected override WebRequest GetWebRequest(Uri address) {
                // In order to compress the response, Accept-Encoding and User-Agent both have to contain "gzip":
                // https://cloud.google.com/appengine/docs/standard/java/how-requests-are-handled#response_compression
                HttpWebRequest request = (HttpWebRequest) base.GetWebRequest(address);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.UserAgent = "Everest/" + Everest.VersionString + "; MaxHelpingHand; gzip";

                return request;
            }
        }

        private static void useCompressedWebClient(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchNewobj<WebClient>())) {
                Logger.Log("MaxHelpingHand/FixesThatShouldBeInEverest", $"Replacing updater web client at {cursor.Index} in DownloadModUpdateList");
                cursor.Next.Operand = typeof(CompressedWebClient).GetConstructor(new Type[0]);
            }
        }


        // === if you save & quit in a level after collecting the heart, the heart will disappear based on whether it's a A-side or not, instead of whether it ends the level or not.

        private static void fixHeartsDisappearingAfterSaveAndQuit(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // if (Session.Area.Mode != 0)
            while (cursor.TryGotoNext(MoveType.After,
                    instr => instr.MatchLdfld<Level>("Session"),
                    instr => instr.MatchLdflda<Session>("Area"),
                    instr => instr.MatchLdfld<AreaKey>("Mode"),
                    instr => instr.OpCode == OpCodes.Brfalse)) {

                Logger.Log("MaxHelpingHand/FixesThatShouldBeInEverest", $"Fixing hearts disappearing after save & quit at {cursor.Index} in Level.LoadLevel");

                cursor.Index--;

                // hearts disappear if they have been collected and we are in an A-side...
                // this is a rule that only works for vanilla levels, because "heart ends level" == "level is not an A-side".
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<AreaMode, Level, AreaMode>>((orig, self) => {
                    if (self.Session.Area.GetLevelSet() == "Celeste") {
                        // it DOES work with vanilla.
                        return orig;
                    }

                    MapMetaModeProperties properties = self.Session.MapData.GetMeta();
                    if (properties != null && (properties.HeartIsEnd ?? false)) {
                        // heart is end: this is like vanilla B- and C-sides.
                        return AreaMode.BSide;
                    } else {
                        // heart is end: this is like vanilla A-sides.
                        return AreaMode.Normal;
                    }
                });
            }
        }
    }
}
