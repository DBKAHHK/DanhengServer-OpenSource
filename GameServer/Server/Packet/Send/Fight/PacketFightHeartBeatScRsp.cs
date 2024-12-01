using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightHeartBeatScRsp : BasePacket
{
    public PacketFightHeartBeatScRsp(ulong clientTimeMs) : base(CmdIds.FightHeartBeatScRsp)
    {
        var proto = new FightHeartBeatScRsp
        {
            ClientTimeMs = clientTimeMs,
            ServerTimeMs = (ulong)System.DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };

        SetData(proto);
    }
}