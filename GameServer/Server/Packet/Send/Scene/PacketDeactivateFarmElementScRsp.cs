using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Scene;

public class PacketDeactivateFarmElementScRsp : BasePacket
{
    public PacketDeactivateFarmElementScRsp(uint id) : base(CmdIds.DeactivateFarmElementScRsp)
    {
        var proto = new DeactivateFarmElementScRsp
        {
            EntityId = id
        };

        SetData(proto);
    }
}