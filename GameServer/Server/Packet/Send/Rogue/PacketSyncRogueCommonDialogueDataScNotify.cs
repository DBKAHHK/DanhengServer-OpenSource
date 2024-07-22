using EggLink.DanhengServer.Game.Rogue.Event;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueCommonDialogueDataScNotify : BasePacket
{
    public PacketSyncRogueCommonDialogueDataScNotify(RogueEventInstance rogueEvent) : base(
        CmdIds.SyncRogueCommonDialogueDataScNotify)
    {
        var proto = new SyncRogueCommonDialogueDataScNotify();

        proto.DialogueEventList.Add(rogueEvent.ToProto());

        SetData(proto);
    }
}