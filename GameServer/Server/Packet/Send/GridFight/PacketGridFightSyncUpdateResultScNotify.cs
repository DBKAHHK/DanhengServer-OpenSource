using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using System.Linq;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;

public class PacketGridFightSyncUpdateResultScNotify : BasePacket
{
    public PacketGridFightSyncUpdateResultScNotify(List<BaseGridFightSyncData> data) : base(CmdIds.GridFightSyncUpdateResultScNotify)
    {
        Dictionary<GridFightSrc, List<BaseGridFightSyncData>> srcDict = [];

        foreach (var syncData in data)
        {
            srcDict.TryAdd(syncData.Src, []);
            srcDict[syncData.Src].Add(syncData);
        }

        var proto = new GridFightSyncUpdateResultScNotify
        {
            SyncResultDataList =
            {
                srcDict.Select(x => new GridFightSyncResultData
                {
                    GridUpdateSrc = x.Key,
                    UpdateDynamicList = { x.Value.Select(j => j.ToProto()) },
                    ONMDGNHMABO = { (uint)x.Value.Count }
                })
            }
        };

        SetData(proto);
    }

    public PacketGridFightSyncUpdateResultScNotify( params BaseGridFightSyncData[] data) : this(data.ToList())
    {
    }
}