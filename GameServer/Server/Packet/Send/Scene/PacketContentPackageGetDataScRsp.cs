using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketContentPackageGetDataScRsp : BasePacket
{
    public PacketContentPackageGetDataScRsp(uint id) : base(CmdIds.ContentPackageGetDataScRsp)
    {
        var proto = new ContentPackageGetDataScRsp
        {
            Data = new ContentPackageData
                { ContentInfoList = { new ContentInfo { ContentId = id, Status = ContentPackageStatus.Finished } } }
        };

        SetData(proto);
    }
}