using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Challenge;
using EggLink.DanhengServer.Game.Battle;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Game.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Spectre.Console;
using System.Text.Json.Serialization;

namespace EggLink.DanhengServer.Game.Challenge
{
    public class ChallengeInstance
    {
        public Position StartPos { get; set; } = new();
        public Position StartRot { get; set; } = new();
        public int ChallengeId { get; set; }
        public int CurrentStage { get; set; }
        public int CurrentExtraLineup { get; set; }
        public int Status { get; set; }
        public bool HasAvatarDied { get; set; }

        public int SavedMp { get; set; }
        public int RoundsLeft { get; set; }
        public int Stars { get; set; }
        public int ScoreStage1 { get; set; }
        public int ScoreStage2 { get; set; }

        [JsonIgnore]
        public List<BattleTarget>? BossTarget1 { get; set; }
        [JsonIgnore]
        public List<BattleTarget>? BossTarget2 { get; set; }
        [JsonIgnore]
        PlayerInstance Player { get; set; }
        [JsonIgnore]
        public ChallengeConfigExcel Excel { get; set; }

        public List<int> StoryBuffs { get; set; } = [];
        public List<int> BossBuffs { get; set; } = [];

        public ChallengeInstance(PlayerInstance player, ChallengeConfigExcel excel)
        {
            Player = player;
            Excel = excel;
            ChallengeId = excel.GetId();
            StartPos = new Position();
            StartRot = new Position();
            CurrentStage = 1;
            RoundsLeft = Excel.IsStory() ? 5 : Excel.ChallengeCountDown;
            SetStatus(ChallengeStatus.ChallengeDoing);
            SetCurrentExtraLineup(ExtraLineupType.LineupChallenge);
        }

        public ChallengeInstance(PlayerInstance player, ChallengeConfigExcel excel, ChallengeInstanceData data)
        {
            Player = player;
            Excel = excel;

            StartPos = data.StartPos;
            ChallengeId = data.ChallengeId;
            CurrentStage = data.CurrentStage;
            CurrentExtraLineup = data.CurrentExtraLineup;
            Status = data.Status;
            HasAvatarDied = data.HasAvatarDied;
            SavedMp = data.SavedMp;
            RoundsLeft = data.RoundsLeft;
            Stars = data.Stars;
            ScoreStage1 = data.ScoreStage1;
            ScoreStage2 = data.ScoreStage2;
            StoryBuffs = data.StoryBuffs;
            BossBuffs = data.BossBuffs;
        }

        public SceneInstance GetScene()
        {
            return Player.SceneInstance!;
        }

        public int GetChallengeId()
        {
            return Excel.GetId();
        }

        public bool IsStory()
        {
            return Excel.IsStory();
        }

        // Early implementation for 2.3
        /* public bool IsBoss()
        {
            return Excel.IsBoss();
        } */

        public void SetStatus(ChallengeStatus status)
        {
            Status = (int)status;
        }

        public void SetCurrentExtraLineup(ExtraLineupType type)
        {
            CurrentExtraLineup = (int)type;
        }

        public int GetRoundsElapsed()
        {
            return Excel.ChallengeCountDown - RoundsLeft;
        }

        public int GetTotalScore()
        {
            return ScoreStage1 + ScoreStage2;
        }

        public bool IsWin()
        {
            return Status == (int)ChallengeStatus.ChallengeFinish;
        }

        #region Management

        public void OnBattleStart(BattleInstance battle)
        {
            battle.RoundLimit = RoundsLeft;
            
            if (StoryBuffs != null)
            {
                battle.Buffs.Add(new MazeBuff(Excel.MazeBuffID, -1, -1));

                if (StoryBuffs.Count >= CurrentStage)
                {
                    int buffId = CurrentStage - 1;
                    battle.Buffs.Add(new MazeBuff(buffId, -1, -1));
                }
            }

            if (Excel.StoryExcel != null)
            {
                battle.AddBattleTarget(1, 10001, GetTotalScore());

                foreach (var id in Excel.StoryExcel.BattleTargetID!)
                {
                    Console.WriteLine(id);
                    battle.AddBattleTarget(5, id, GetTotalScore());
                }
            }
        }

        public void OnUpdate()
        {
            // End challenge if its done
            if (Status != (int)ChallengeStatus.ChallengeDoing)
            {
                Player.ChallengeManager!.ChallengeInstance = null;
            }
        }

        #endregion

        #region Serialization

        public CurChallenge ToProto()
        {
            CurChallenge proto = new CurChallenge()
            {
                ChallengeId = (uint)Excel.GetId(),
                Status = (ChallengeStatus)Status,
                ScoreId = (uint)ScoreStage1,
                ScoreTwo = (uint)ScoreStage2,
                RoundCount = (uint)GetRoundsElapsed(),
                ExtraLineupType = (ExtraLineupType)CurrentExtraLineup
            };

            if (StoryBuffs != null && StoryBuffs.Count >= CurrentStage)
            {
                proto.PlayerInfo = new ChallengeStoryInfo() { CurStoryBuff = new ChallengeStoryBuffInfo() { } };
                proto.PlayerInfo.CurStoryBuff.BuffList.Add((uint)StoryBuffs[CurrentStage - 1]);
            }

            return proto;
        }

        #endregion
    }
}
