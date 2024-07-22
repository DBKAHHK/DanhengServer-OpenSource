using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.Server.Packet.Send.Others;

public class PacketServerAnnounceNotify : BasePacket
{
    public PacketServerAnnounceNotify() : base(CmdIds.ServerAnnounceNotify)
    {
        var proto = new ServerAnnounceNotify();

        proto.AnnounceDataList.Add(new AnnounceData
        {
            BeginTime = Extensions.GetUnixSec(),
            EndTime = Extensions.GetUnixSec() + 3600,
            ConfigId = 1,
            CHJPFPLHJBJ = ConfigManager.Config.ServerOption.ServerAnnounce.AnnounceContent
        });

        if (ConfigManager.Config.ServerOption.ServerAnnounce.EnableAnnounce) SetData(proto);
    }
}