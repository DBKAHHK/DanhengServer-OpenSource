using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketGetRogueInitialScoreScRsp : BasePacket
{
    public PacketGetRogueInitialScoreScRsp(PlayerInstance player) : base(CmdIds.GetRogueInitialScoreScRsp)
    {
        var proto = new GetRogueInitialScoreScRsp
        {
            RogueScoreRewardInfo = player.RogueManager!.ToRewardProto()
        };

        SetData(proto);
    }
}