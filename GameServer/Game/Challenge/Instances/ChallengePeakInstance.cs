using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Challenge.Definitions;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.ChallengePeak;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Challenge.Instances;

public class ChallengePeakInstance(PlayerInstance player, ChallengeDataPb data) : BaseChallengeInstance(player, data)
{

    #region Properties

    public ChallengePeakConfigExcel Config { get; } = GameData.ChallengePeakConfigData[(int)data.Peak.CurrentPeakLevelId];
    public bool IsWin { get; private set; }

    #endregion

    #region Setter & Getter

    public override Dictionary<int, List<ChallengeConfigExcel.ChallengeMonsterInfo>> GetStageMonsters()
    {
        if (!Data.Peak.IsHard || Config.BossExcel == null) return Config.ChallengeMonsters;

        Dictionary<int, List<ChallengeConfigExcel.ChallengeMonsterInfo>> monsters = [];

        monsters.Add(Config.MazeGroupID, []);
        for (var i = 0; i < Config.ConfigIDList.Count; i++)
        {
            monsters[Config.MazeGroupID].Add(new ChallengeConfigExcel.ChallengeMonsterInfo(Config.ConfigIDList[i],
                Config.NpcMonsterIDList[i], Config.BossExcel.HardEventIDList[i]));
        }

        return monsters;
    }

    #endregion

    //#region Serialization

    //#endregion

    #region Handlers

    public override void OnBattleStart(BattleInstance battle)
    {
        foreach (var peakBuff in Data.Peak.Buffs)
        {
            battle.Buffs.Add(new MazeBuff((int)peakBuff, 1, -1)
            {
                WaveFlag = -1
            });
        }

        if (Data.Peak.IsHard && Config.BossExcel != null)
        {
            var excel = GameData.BattleTargetConfigData.GetValueOrDefault(Config.BossExcel.HardTarget);
            if (excel != null)
                battle.AddBattleTarget(5, excel.ID, 0, excel.TargetParam);
        }

        foreach (var targetId in Config.NormalTargetList)
        {
            var excel = GameData.BattleTargetConfigData.GetValueOrDefault(targetId);
            if (excel != null)
                battle.AddBattleTarget(5, excel.ID, 0, excel.TargetParam);
        }
    }

    public override async ValueTask OnBattleEnd(BattleInstance battle, PVEBattleResultCsReq req)
    {
        switch (req.EndStatus)
        {
            case BattleEndStatus.BattleEndWin:
                // Get monster count in stage
                long monsters = Player.SceneInstance!.Entities.Values.OfType<EntityMonster>().Count();

                if (monsters == 0)
                {
                    Data.Peak.CurStatus = (int)ChallengeStatus.ChallengeFinish;
                    Data.Peak.Stars = CalculateStars(req);
                    IsWin = true;

                    await Player.SendPacket(new PacketChallengePeakSettleScNotify(this));
                    // Call MissionManager
                    await Player.MissionManager!.HandleFinishType(MissionFinishTypeEnum.ChallengeFinish, this);
                }

                // Set saved technique points (This will be restored if the player resets the challenge)
                Data.Peak.SavedMp = (uint)Player.LineupManager!.GetCurLineup()!.Mp;
                break;
            case BattleEndStatus.BattleEndQuit:
                // Reset technique points and move back to start position
                var lineup = Player.LineupManager!.GetCurLineup()!;
                lineup.Mp = (int)Data.Peak.SavedMp;
                if (Data.Peak.StartPos != null && Data.Peak.StartRot != null)
                    await Player.MoveTo(Data.Peak.StartPos.ToPosition(), Data.Peak.StartRot.ToPosition());
                await Player.SendPacket(new PacketSyncLineupNotify(lineup));
                break;
            default:
                // Determine challenge result
                // Fail challenge
                Data.Peak.CurStatus = (int)ChallengeStatus.ChallengeFailed;

                // Send challenge result data
                await Player.SendPacket(new PacketChallengePeakSettleScNotify(this));

                break;
        }
    }

    public uint CalculateStars(PVEBattleResultCsReq req)
    {
        var targets = Config.NormalTargetList;
        var stars = 0u;

        foreach (var targetId in targets)
        {
            var target = req.Stt.BattleTargetInfo[5].BattleTargetList_.FirstOrDefault(x => x.Id == targetId);
            if (target == null) continue;

            if (target.Progress <= target.TotalProgress)
                stars++;
        }

        return Math.Min(stars, 7);
    }

    #endregion
}