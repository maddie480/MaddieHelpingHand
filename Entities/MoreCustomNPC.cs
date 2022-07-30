using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // Everest Custom NPC but with some more options
    [CustomEntity("MaxHelpingHand/MoreCustomNPC", "MaxHelpingHand/CustomNPCSprite")]
    [Tracked]
    public class MoreCustomNPC : CustomNPC {
        private readonly Rectangle? talkerZone;
        private readonly bool hasDialogue;

        private readonly string onlyIfFlag;
        private readonly string setFlag;
        private bool shouldSetFlag = true;

        private Sprite sprite;
        private string spriteName;

        public MoreCustomNPC(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) {
            hasDialogue = !string.IsNullOrEmpty(data.Attr("dialogId"));

            onlyIfFlag = data.Attr("onlyIfFlag");
            setFlag = data.Attr("setFlag");

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
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (talkerZone.HasValue && Talker != null) {
                // apply the talker zone now (the talker is initialized in Added).
                Talker.Bounds = talkerZone.Value;
            }

            if (!hasDialogue && Talker != null) {
                Remove(Talker);
                shouldSetFlag = false;
            } else if (Talker == null) {
                shouldSetFlag = false;
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
                Talker.Enabled = SceneAs<Level>().Session.GetFlag(onlyIfFlag);
            }
            if (shouldSetFlag && !string.IsNullOrEmpty(setFlag) && Talker?.Entity == null) {
                SceneAs<Level>().Session.SetFlag(setFlag);
                shouldSetFlag = false;
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
    }
}
