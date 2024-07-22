using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketGetRogueScoreRewardInfoScRsp : BasePacket
{
    public PacketGetRogueScoreRewardInfoScRsp(PlayerInstance player) : base(CmdIds.GetRogueScoreRewardInfoScRsp)
    {
        var proto = new GetRogueScoreRewardInfoScRsp
        {
            Info = player.RogueManager!.ToRewardProto()
        };

        SetData(proto);
    }
}