using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;

public class MarbleGameInfoSyncData(
    MarbleNetWorkMsgEnum type,
    MarbleSyncType syncType,
    MarbleGameRoomInstance room,
    List<MarbleGameSealSyncData> syncDatas) : MarbleGameBaseSyncData(type)
{
    public override FightGameInfo ToProto()
    {
        return new FightGameInfo
        {
            GameMessageType = (uint)MessageType,
            MarbleGameSyncInfo = new MarbleGameSyncInfo
            {
                MarbleSyncType = syncType,
                CurRound = (uint)room.CurRound,
                AllowedMoveSealList =
                {
                    (room.Players[(int)room.CurMoveTeamType - 1] as MarbleGamePlayerInstance)!.AllowMoveSealList.Select(
                        x =>
                            (uint)x)
                },
                MarbleGameSyncData = { syncDatas.Select(x => x.ToProto()) },
                FirstPlayerActionEnd = room.CurMoveTeamType == MarbleTeamType.TeamA
            }
        };
    }
}

public class MarbleGameInfoLaunchingSyncData(
    MarbleNetWorkMsgEnum type,
    MarbleSyncType syncType,
    float time,
    int itemId,
    List<BaseMarbleGameSyncData> syncDatas) : MarbleGameBaseSyncData(type)
{
    public override FightGameInfo ToProto()
    {
        return new FightGameInfo
        {
            GameMessageType = (uint)MessageType,
            MarbleGameSyncInfo = new MarbleGameSyncInfo
            {
                Launching = true,
                MarbleSyncType = syncType,
                MoveTotalTime = time,
                QueuePosition = (uint)itemId,
                MarbleGameSyncData = { syncDatas.Select(x => x.ToProto()) }
            }
        };
    }
}