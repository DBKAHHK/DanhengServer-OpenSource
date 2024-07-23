using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueCommonActionResultScNotify : BasePacket
{
    public PacketSyncRogueCommonActionResultScNotify(int rogueSubmode, RogueCommonActionResult result,
        RogueActionDisplayType displayType = RogueActionDisplayType.RogueCommonActionResultDisplayTypeNone) : base(
        CmdIds.SyncRogueCommonActionResultScNotify)
    {
        var proto = new SyncRogueCommonActionResultScNotify
        {
            RogueSubMode = (uint)rogueSubmode,
            DisplayType = displayType
        };

        proto.ActionResult.Add(result);

        SetData(proto);
    }

    public PacketSyncRogueCommonActionResultScNotify(int rogueSubmode, List<RogueCommonActionResult> results,
        RogueActionDisplayType displayType = RogueActionDisplayType.RogueCommonActionResultDisplayTypeNone) : base(
        CmdIds.SyncRogueCommonActionResultScNotify)
    {
        var proto = new SyncRogueCommonActionResultScNotify
        {
            RogueSubMode = (uint)rogueSubmode,
            DisplayType = displayType
        };

        proto.ActionResult.AddRange(results);

        SetData(proto);
    }
}