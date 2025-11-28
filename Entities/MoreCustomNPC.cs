using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // Everest Custom NPC but with some more options
    [CustomEntity("MaxHelpingHand/MoreCustomNPC", "MaxHelpingHand/CustomNPCSprite")]
    [Tracked]
    public class MoreCustomNPC : CustomNPC {
        private static Type customNPCTalkCoroutineType;
        private static ILHook hookCustomNPCTalk;

        public static void Load() {
            MethodInfo customNPCTalkCoroutine = typeof(CustomNPC).GetMethod("Talk", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();

            customNPCTalkCoroutineType = customNPCTalkCoroutine.DeclaringType;
            hookCustomNPCTalk = new ILHook(customNPCTalkCoroutine, modTalkCutscene);
        }

        public static void Unload() {
            customNPCTalkCoroutineType = null;
            hookCustomNPCTalk?.Dispose();
            hookCustomNPCTalk = null;
        }

        private static IEnumerator talkWithDifferentFont(IEnumerator talk, string font) {
            // we need to make the Talk coroutine make a step, so that it "becomes" Textbox.Say.
            // ExtendedDialogCutsceneTrigger.ReplaceFancyTextFontFor can take care of the font change afterward.
            if (talk.MoveNext()) yield return talk.Current;
            yield return new SwapImmediately(ExtendedDialogCutsceneTrigger.ReplaceFancyTextFontFor(talk, font));
        }

        private static void modTalkCutscene(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdnull(), instr => instr.MatchCall<Textbox>("Say"))) {
                cursor.Index++;

                Logger.Log("MaxHelpingHand/MoreCustomNPC", $"Injecting auto-skip at {cursor.Index} in IL for CustomNPC.Talk");

                // ldarg.0 (aka "this") gives the state machine object, known as CustomNPC.<Talk>d__28 in ILSpy.
                // To get the actual CustomNPC we are in, we need to read the <>4__this field that is in that state machine object.
                // ... IEnumerators are weird.
                Action emitThis = () => {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldfld, customNPCTalkCoroutineType.GetField("<>4__this"));
                };

                emitThis();
                cursor.EmitDelegate<Func<Func<IEnumerator>[], CustomNPC, Func<IEnumerator>[]>>(registerTriggers);

                // intercept the Textbox.Say call right before it's yield returned if necessary!
                cursor.Index++;
                emitThis();
                cursor.EmitDelegate<Func<IEnumerator, CustomNPC, IEnumerator>>(modTextboxSay);
            }
        }

        private static Func<IEnumerator>[] registerTriggers(Func<IEnumerator>[] orig, CustomNPC self) {
            if (self is MoreCustomNPC customNPC && customNPC.autoSkipEnabled) {
                // we want to register {trigger 0} and {trigger 1} to start and stop auto-skip in this dialogue.
                return new Func<IEnumerator>[] { customNPC.startSkipping, customNPC.stopSkipping };
            }

            // don't mess with "vanilla" Everest. orig should be null (considering we're injecting ourselves just after an ldnull), but hey.
            return orig;
        }

        private static IEnumerator modTextboxSay(IEnumerator orig, CustomNPC self) {
            if (self is MoreCustomNPC npc && !string.IsNullOrEmpty(npc.customFont)) {
                return ExtendedDialogCutsceneTrigger.ReplaceFancyTextFontFor(orig, npc.customFont);
            }
            return orig;
        }


        private readonly Rectangle? talkerZone;
        private readonly bool hasDialogue;

        private readonly string onlyIfFlag;
        private readonly string setFlag;
        private readonly bool onlyIfFlagInverted;
        private readonly bool setFlagInverted;


        private readonly bool autoSkipEnabled;
        private readonly string customFont;

        private Sprite sprite;
        private string spriteName;

        public MoreCustomNPC(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) {
            hasDialogue = !string.IsNullOrEmpty(data.Attr("dialogId"));

            onlyIfFlag = data.Attr("onlyIfFlag");
            setFlag = data.Attr("setFlag");
            onlyIfFlagInverted = data.Bool("onlyIfFlagInverted");
            setFlagInverted = data.Bool("setFlagInverted");

            autoSkipEnabled = data.Bool("autoSkipEnabled");
            customFont = data.Attr("customFont");

            DynData<CustomNPC> npcData = new DynData<CustomNPC>(this);

            spriteName = data.Attr("spriteName", "");
            if (!string.IsNullOrEmpty(spriteName)) {
                // replace the NPC texture with a sprite.
                npcData["textures"] = null;

                sprite = GFX.SpriteBank.Create(spriteName);
                sprite.Scale = npcData.Get<Vector2>("scale");
                Add(sprite);
            }

            string frames = data.Attr("frames", "");
            if (!string.IsNullOrEmpty(frames)) {
                // "textures" currently contains all frames, but we only want some.
                List<MTexture> npcTextures = npcData.Get<List<MTexture>>("textures");
                List<MTexture> allTextures = new List<MTexture>(npcTextures);

                // clear the texture list, then only add back the textures we want!
                npcTextures = new List<MTexture>();
                npcData["textures"] = npcTextures;
                foreach (int frame in Calc.ReadCSVIntWithTricks(frames)) {
                    npcTextures.Add(allTextures[frame]);
                }
            }

            if (data.Nodes.Length >= 2) {
                // the nodes define a "talker zone", with one being the top left, and the other the bottom right.
                // we're adding 8 to bottom and right because nodes are rendered as 8x8 rectangles in Ahorn,
                // and we want to take the bottom/right of those rectangles.
                Vector2[] nodesOffset = data.NodesOffset(offset);
                float top = Math.Min(nodesOffset[0].Y, nodesOffset[1].Y);
                float bottom = Math.Max(nodesOffset[0].Y, nodesOffset[1].Y) + 8;
                float left = Math.Min(nodesOffset[0].X, nodesOffset[1].X);
                float right = Math.Max(nodesOffset[0].X, nodesOffset[1].X) + 8;

                talkerZone = new Rectangle((int) (left - Position.X), (int) (top - Position.Y), (int) (right - left), (int) (bottom - top));
            }

            if (!string.IsNullOrEmpty(setFlag)) {
                OnEnd += () => {
                    Session session = SceneAs<Level>().Session;
                    List<Component> toRemove = new DynData<ComponentList>(Components).Get<List<Component>>("toRemove");

                    // for dialogue that plays only once: the talker (speech bubble) is removed when it is over.
                    // for dialogue that loops: the session counter is reset to 0 once all dialog IDs have been played.
                    if (toRemove.Contains(Talker) || Talker.Entity == null || session.GetCounter(id.ToString() + "DialogCounter") == 0) {
                        session.SetFlag(setFlag, !setFlagInverted);
                    }
                };
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (talkerZone.HasValue && Talker != null) {
                // apply the talker zone now (the talker is initialized in Added).
                Talker.Bounds = talkerZone.Value;
            }

            if (!hasDialogue && Talker != null) {
                Remove(Talker);
            }
        }

        public override void Render() {
            base.Render();

            // CustomNPC overrides Render without calling base, so we need to manually render the sprite.
            sprite?.Render();
        }

        public override void Update() {
            base.Update();

            if (!string.IsNullOrEmpty(onlyIfFlag) && Talker?.Entity != null) {
                Talker.Enabled = (SceneAs<Level>().Session.GetFlag(onlyIfFlag) != onlyIfFlagInverted);
            }
        }

        public static MoreCustomNPC GetNPC(string spriteName) {
            foreach (MoreCustomNPC result in Engine.Scene.Tracker.GetEntities<MoreCustomNPC>()) {
                if (result.spriteName == spriteName) {
                    return result;
                }
            }

            Logger.Log(LogLevel.Warn, "MaxHelpingHand/MoreCustomNPC", $"Custom NPC with sprite name {spriteName} not found!");
            return null;
        }

        public void PlayAnimation(string name) {
            if (sprite != null) {
                if (sprite.Has(name)) {
                    sprite.Play(name);
                } else {
                    Logger.Log(LogLevel.Warn, "MaxHelpingHand/MoreCustomNPC", $"Tried to play non-existent animation {name}!");
                }
            } else {
                Logger.Log(LogLevel.Warn, "MaxHelpingHand/MoreCustomNPC", "Tried to play an animation on a non-sprite-based custom NPC!");
            }
        }

        public void SetHorizontalScale(float scale) {
            if (sprite == null) {
                DynData<CustomNPC> thisData = new DynData<CustomNPC>(this);
                thisData["scale"] = new Vector2(scale, thisData.Get<Vector2>("scale").Y);
            } else {
                sprite.Scale.X = scale;
            }
        }

        public Sprite GetSprite() {
            if (sprite == null) {
                Logger.Log(LogLevel.Warn, "MaxHelpingHand/MoreCustomNPC", "This NPC does not use a sprite!");
            }
            return sprite;
        }

        // this is triggered with {trigger 0}
        private IEnumerator startSkipping() {
            new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = !MaxHelpingHandModule.Instance.Settings.DisableDialogueAutoSkip;
            yield break;
        }

        // this is triggered with {trigger 1}
        private IEnumerator stopSkipping() {
            new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = false;
            yield break;
        }
    }
}
