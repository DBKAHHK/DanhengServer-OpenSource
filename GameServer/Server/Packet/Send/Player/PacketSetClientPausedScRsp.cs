using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketSetClientPausedScRsp : BasePacket
{
    public PacketSetClientPausedScRsp(bool paused) : base(CmdIds.SetClientPausedScRsp)
    {
        var rsp = new SetClientPausedScRsp
        {
            Paused = paused
        };
        SetData(rsp);
    }
}