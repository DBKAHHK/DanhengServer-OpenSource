using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Others;

public class PacketRetcodeNotify : BasePacket
{
    public PacketRetcodeNotify(Retcode retcode) : base(CmdIds.RetcodeNotify)
    {
        var proto = new RetcodeNotify
        {
            Retcode = retcode // original proto is uint, i modify it to Retcode enum
        };

        SetData(proto);
    }
}