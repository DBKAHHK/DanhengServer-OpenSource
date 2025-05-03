using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public class MarbleGameInfoSyncData(MarbleNetWorkMsgEnum type, MarbleSyncType syncType, MarbleGameRoomInstance room, List<MarbleGameSealSyncData> syncDatas) : MarbleGameBaseSyncData(type)
{public override MarbleGameSyncInfo ToProto()
    {
        return new MarbleGameSyncInfo
        {
            MarbleSyncType = syncType,
            CurRound = (uint)room.CurRound,
            AllowedMoveSealList = { (room.Players[(int)room.CurMoveTeamType - 1] as MarbleGamePlayerInstance)!.AllowMoveSealList.Select(x => (uint)x) },
            MarbleGameSyncData = { syncDatas.Select(x => x.ToProto()) }
        };
    }
}