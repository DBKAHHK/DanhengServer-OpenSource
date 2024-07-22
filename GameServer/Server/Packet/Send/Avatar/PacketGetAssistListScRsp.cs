using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Avatar;

public class PacketGetAssistListScRsp : BasePacket
{
    public PacketGetAssistListScRsp() : base(CmdIds.GetAssistListScRsp)
    {
        var proto = new GetAssistListScRsp();

        SetData(proto);
    }
}