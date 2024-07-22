using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Player;

public class PacketInteractChargerScRsp : BasePacket
{
    public PacketInteractChargerScRsp(ChargerInfo chargerInfo) : base(CmdIds.InteractChargerScRsp)
    {
        var proto = new InteractChargerScRsp
        {
            ChargerInfo = chargerInfo
        };

        SetData(proto);
    }
}