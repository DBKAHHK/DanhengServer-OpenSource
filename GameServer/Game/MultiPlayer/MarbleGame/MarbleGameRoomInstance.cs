using EggLink.DanhengServer.Enums.Fight;
using EggLink.DanhengServer.GameServer.Game.Lobby;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;
using EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame;

public class MarbleGameRoomInstance : BaseMultiPlayerGameRoomInstance
{
    public MarbleTeamType CurMoveTeamType { get; set; }
    public int CurRound { get; set; }

    public MarbleGameRoomInstance(long roomId, LobbyRoomInstance parentLobby) : base(roomId, parentLobby)
    {
        // random move team type
        CurMoveTeamType = (MarbleTeamType)Random.Shared.Next(1, 3);
        // set player
        foreach (var player in parentLobby.Players)
        {
            Players.Add(new MarbleGamePlayerInstance(player, (MarbleTeamType)(parentLobby.Players.IndexOf(
                player) + 1)));
        }
    }

    public async ValueTask BroadCastToRoom(BasePacket packet)
    {
        foreach (var player in Players)
        {
            await player.SendPacket(packet);
        }
    }

    public async ValueTask EnterGame(int uid)
    {
        var player = GetPlayerById(uid);
        if (player == null) return;
        player.EnterGame = true;

        if (player is MarbleGamePlayerInstance marblePlayer)
        {
            marblePlayer.Phase = MarblePlayerPhaseEnum.EnterGame;
        }

        if (Players.All(x => x.EnterGame))
        {
            // send basic info
            await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
                MarbleNetWorkMsgEnum.GameStart, this));
        }
    }

    #region Handler

    public async ValueTask HandleGeneralRequest(MarbleGamePlayerInstance player, uint msgType, byte[] reqData)
    {
        var messageType = (MarbleNetWorkMsgEnum)msgType;

        switch (messageType)
        {
            case MarbleNetWorkMsgEnum.LoadFinish:
                await LoadFinish(player);
                break;
            case MarbleNetWorkMsgEnum.PerformanceFinish:
                await PerformanceFinish(player);
                break;
            default:
                break;
        }
    }

    public async ValueTask LoadFinish(MarbleGamePlayerInstance player)
    {
        player.Phase = MarblePlayerPhaseEnum.LoadFinish;
        if (Players.OfType<MarbleGamePlayerInstance>().ToList().All(x => x.Phase == MarblePlayerPhaseEnum.LoadFinish))
        {
            // next phase (performance)
            await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
                [new MarblePerformanceSyncData(MarbleNetWorkMsgEnum.SyncNotify)]));
        }
    }

    public async ValueTask PerformanceFinish(MarbleGamePlayerInstance player)
    {
        player.Phase = MarblePlayerPhaseEnum.PerformanceFinish;
        if (Players.OfType<MarbleGamePlayerInstance>().ToList().All(x => x.Phase == MarblePlayerPhaseEnum.PerformanceFinish))
        {
            // next phase (round start)
            await RoundStart();
        }
    }

    #endregion

    #region Round

    public async ValueTask RoundStart()
    {
        CurRound++;
        foreach (var player in Players.OfType<MarbleGamePlayerInstance>())
        {
            player.ChangeRound();
        }

        await BroadCastToRoom(new PacketFightGeneralScNotify(MarbleNetWorkMsgEnum.SyncBatch,
        [
            new MarbleGameInfoSyncData(MarbleNetWorkMsgEnum.SyncNotify, MarbleSyncType.RoundStart, this,
                Players.OfType<MarbleGamePlayerInstance>().SelectMany(x => x.SealList.Values)
                    .Select(x => new MarbleGameSealSyncData(x, MarbleFrameType.RoundStart)).ToList())
        ]));
    }

    #endregion

    public MarbleGameInfo ToProto()
    {
        return new MarbleGameInfo
        {
            LobbyBasicInfo = { ParentLobby.Players.Select(x => x.ToProto()) },
            CurActionTeamType = CurMoveTeamType,
            LevelId = (uint)Random.Shared.Next(100, 103),
            TeamAPlayer = (uint)Players[0].LobbyPlayer.Player.Uid,
            TeamBPlayer = (uint)Players[1].LobbyPlayer.Player.Uid,
            TeamARank = 1,
            TeamBRank = 1,
            TeamASealList = { (Players[0] as MarbleGamePlayerInstance)!.SealList.Select(x => (uint)x.Value.SealId) },
            TeamBSealList = { (Players[1] as MarbleGamePlayerInstance)!.SealList.Select(x => (uint)x.Value.SealId) },
            PlayerAScore = (uint)(Players[0] as MarbleGamePlayerInstance)!.Score,
            PlayerBScore = (uint)(Players[1] as MarbleGamePlayerInstance)!.Score,
            ControlByServer = true
        };
    }
}