using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public class MarblePerformanceSyncData(MarbleNetWorkMsgEnum type) : MarbleGameBaseSyncData(type)
{
    public override MarbleGameSyncInfo ToProto()
    {
        return new MarbleGameSyncInfo
        {
            MarbleSyncType = MarbleSyncType.Performance
        };
    }
}