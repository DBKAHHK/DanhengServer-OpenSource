using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;

public class PacketMultiplayerGetFightGateScRsp : BasePacket
{
    public PacketMultiplayerGetFightGateScRsp() : base(CmdIds.MultiplayerGetFightGateScRsp)
    {
        var proto = new MultiplayerGetFightGateScRsp
        {
            Ip = ConfigManager.Config.GameServer.PublicAddress,
            Port = ConfigManager.Config.GameServer.Port
        };

        SetData(proto);
    }
}