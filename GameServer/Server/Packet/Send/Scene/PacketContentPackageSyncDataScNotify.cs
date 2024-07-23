using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketContentPackageSyncDataScNotify : BasePacket
{
    public PacketContentPackageSyncDataScNotify() : base(CmdIds.ContentPackageSyncDataScNotify)
    {
        var proto = new ContentPackageSyncDataScNotify
        {
            Data = new ContentPackageData
            {
                ContentInfoList =
                {
                    GameData.ContentPackageConfigData.Select(x => new ContentInfo
                    {
                        ContentId = (uint)x.Key,
                        Status = ContentPackageStatus.Finished
                    })
                }
            }
        };

        SetData(proto);
    }
}