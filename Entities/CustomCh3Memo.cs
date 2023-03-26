using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomCh3Memo", "MaxHelpingHand/CustomCh3MemoOnFlagController")]
    public class CustomCh3Memo : Trigger {
        // common options
        private readonly string paperSpriteFolderName;
        private readonly string dialogId;
        private readonly string dialogBeforeId;
        private readonly string dialogAfterId;
        private readonly string flagOnCompletion;
        private readonly bool dialogBeforeOnlyOnce;
        private readonly bool dialogAfterOnlyOnce;
        private readonly EntityID id;

        // controller options
        private readonly string flag;
        private readonly bool flagInverted;
        private readonly bool flagReusable;
        private readonly bool onlyOnce;

        private TalkComponent talker;

        public CustomCh3Memo(EntityData data, Vector2 offset, EntityID gid) : base(data, offset) {
            // common options
            paperSpriteFolderName = data.Attr("paperSpriteFolderName");
            dialogId = data.Attr("dialogId");
            dialogBeforeId = data.Attr("dialogBeforeId");
            dialogAfterId = data.Attr("dialogAfterId");
            flagOnCompletion = data.Attr("flagOnCompletion");
            dialogBeforeOnlyOnce = data.Bool("dialogBeforeOnlyOnce");
            dialogAfterOnlyOnce = data.Bool("dialogAfterOnlyOnce");
            id = gid;

            // controller options
            flag = data.Attr("flag");
            flagInverted = data.Bool("flagInverted");
            flagReusable = data.Bool("flagReusable");
            onlyOnce = data.Bool("onlyOnce");

            Collider = new Hitbox(data.Width, data.Height);

            if (data.Name == "MaxHelpingHand/CustomCh3Memo") {
                // spawn the talker
                Vector2 drawAt = new Vector2(data.Width / 2, 0f);
                if (data.Nodes.Length != 0) {
                    drawAt = data.Nodes[0] - data.Position;
                }
                Add(talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, OnTalk));
                talker.PlayerMustBeFacing = false;
            }
        }

        public override void Update() {
            base.Update();

            if (SceneAs<Level>().Session.GetFlag(flag) != flagInverted) {
                // cutscene should be triggered!
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null) {
                    OnTalk(player);

                    if (flagReusable && !onlyOnce) {
                        // disable the flag to avoid repeating the cutscene, but still be able to trigger it again later
                        SceneAs<Level>().Session.SetFlag(flag, flagInverted);
                    } else {
                        if (onlyOnce) {
                            // prevent the game from loading the controller again
                            SceneAs<Level>().Session.DoNotLoad.Add(id);
                        }

                        // remove the controller to avoid repeating the cutscene without messing with the flag
                        RemoveSelf();
                    }
                }
            }
        }

        public void OnTalk(Player player) {
            Scene.Add(new CustomCh3MemoCutscene(player, paperSpriteFolderName, dialogId, dialogBeforeId, dialogAfterId, flagOnCompletion, dialogBeforeOnlyOnce, dialogAfterOnlyOnce, id));
        }
    }
}
