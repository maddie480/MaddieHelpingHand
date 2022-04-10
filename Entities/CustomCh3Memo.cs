using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomCh3Memo")]
    public class CustomCh3Memo : Trigger {
        private readonly string paperSpriteFolderName;
        private readonly string dialogId;

        private TalkComponent talker;

        public CustomCh3Memo(EntityData data, Vector2 offset) : base(data, offset) {
            paperSpriteFolderName = data.Attr("paperSpriteFolderName");
            dialogId = data.Attr("dialogId");

            Collider = new Hitbox(data.Width, data.Height);

            Vector2 drawAt = new Vector2(data.Width / 2, 0f);
            if (data.Nodes.Length != 0) {
                drawAt = data.Nodes[0] - data.Position;
            }

            Add(talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, OnTalk));
            talker.PlayerMustBeFacing = false;
        }

        public void OnTalk(Player player) {
            Scene.Add(new CustomCh3MemoCutscene(player, paperSpriteFolderName, dialogId));
        }
    }
}
