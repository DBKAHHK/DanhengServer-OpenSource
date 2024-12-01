using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;

public class PacketGetCrossInfoScRsp : BasePacket
{
    public PacketGetCrossInfoScRsp() : base(CmdIds.GetCrossInfoScRsp)
    {
        var proto = new GetCrossInfoScRsp
        {
            FightGameMode = FightGameMode.Match3,
            RoomId = 0
        };

        SetData(proto);
    }
}