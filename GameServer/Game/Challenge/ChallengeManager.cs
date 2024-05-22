using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Database.Challenge;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Challenge;

namespace EggLink.DanhengServer.Game.Challenge
{
    public class ChallengeManager(PlayerInstance player) : BasePlayerManager(player)
    {
        #region Properties

        public ChallengeInstance? ChallengeInstance { get; set; }
        public ChallengeData ChallengeData { get; private set; } = DatabaseHelper.Instance!.GetInstanceOrCreateNew<ChallengeData>(player.Uid);

        #endregion

        #region Management

        public void StartChallenge(int challengeId, StartChallengeStoryBuffInfo? storyBuffs)
        {
            // Get challenge excel
            if (!GameData.ChallengeConfigData.ContainsKey(challengeId))
            {
                Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeNotExist));
                return;
            }
            ChallengeConfigExcel Excel = GameData.ChallengeConfigData[challengeId];

            // Sanity check lineups
            if (Excel.StageNum > 0)
            {
                // Get lineup
                var Lineup = Player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge)!;

                // Make sure this lineup has avatars set
                if (Lineup.AvatarData!.Avatars.Count == 0)
                {
                    Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeLineupEmpty));
                    return;
                }

                // Reset hp/sp
                foreach (var avatar in Lineup.AvatarData!.Avatars)
                {
                    avatar.SetCurHp(10000, true);
                    avatar.SetCurSp(avatar.GetCurSp(true) / 2, true);
                }

                // Set technique points to full
                Lineup.Mp = 5; // Max Mp
            }

            if (Excel.StageNum >= 2)
            {
                // Get lineup
                var Lineup = Player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge2)!;

                // Make sure this lineup has avatars set
                if (Lineup.AvatarData!.Avatars.Count == 0)
                {
                    Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeLineupEmpty));
                    return;
                }

                // Reset hp/sp
                foreach (var avatar in Lineup.AvatarData!.Avatars)
                {
                    avatar.SetCurHp(10000, true);
                    avatar.SetCurSp(avatar.GetCurSp(true) / 2, true);
                }

                // Set technique points to full
                Lineup.Mp = 5; // Max Mp
            }

            // Set challenge data for player
            ChallengeInstance instance = new ChallengeInstance(Player, Excel);
            ChallengeInstance = instance;

            // Set first lineup before we enter scenes
            Player.LineupManager!.SetCurLineup(instance.CurrentExtraLineup);

            // Enter scene
            try
            {
                Player.EnterScene(Excel.MapEntranceID, 0, true);
            }
            catch
            {
                // Reset lineup/instance if entering scene failed
                this.ChallengeInstance = null;

                // Send error packet
                Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeNotExist));
                return;
            }

            // Save start positions
            instance.StartPos = Player.LastPos!;
            instance.StartRot = Player.LastRot!;
            instance.SavedMp = Player.LineupManager.GetCurLineup()!.Mp;

            if (Excel.IsStory() && storyBuffs != null)
            {
                instance.StoryBuffs.Add((int)storyBuffs.StoryBuffOne);
                instance.StoryBuffs.Add((int)storyBuffs.StoryBuffTwo);
            }

            // Early implementation for 2.3
            /* if (BossBuffs != null)
            {
                instance.AddBossBuff((int)BossBuffs.BossBuffOne);
                instance.AddBossBuff((int)BossBuffs.BossBuffTwo);
            } */

            // Send packet
            Player.SendPacket(new PacketStartChallengeScRsp(Player));

            // Save instance
            SaveInstance(instance);
        }

        public void SaveInstance(ChallengeInstance instance)
        {
            ChallengeData.Instance.StartPos = instance.StartPos;
            ChallengeData.Instance.ChallengeId = instance.ChallengeId;
            ChallengeData.Instance.CurrentStage = instance.CurrentStage;
            ChallengeData.Instance.CurrentExtraLineup = instance.CurrentExtraLineup;
            ChallengeData.Instance.Status = instance.Status;
            ChallengeData.Instance.HasAvatarDied = instance.HasAvatarDied;
            ChallengeData.Instance.SavedMp = instance.SavedMp;
            ChallengeData.Instance.RoundsLeft = instance.RoundsLeft;
            ChallengeData.Instance.Stars = instance.Stars;
            ChallengeData.Instance.ScoreStage1 = instance.ScoreStage1;
            ChallengeData.Instance.ScoreStage2 = instance.ScoreStage2;
            ChallengeData.Instance.StoryBuffs = instance.StoryBuffs;
            ChallengeData.Instance.BossBuffs = instance.BossBuffs;
        }

        public void ClearInstance()
        {
            ChallengeData.Instance.ChallengeId = 0;
        }

        public void ResurrectInstance()
        {
            if (ChallengeData.Instance != null && ChallengeData.Instance.ChallengeId != 0)
            {
                int ChallengeId = ChallengeData.Instance.ChallengeId;
                if (GameData.ChallengeConfigData.ContainsKey(ChallengeId))
                {
                    ChallengeConfigExcel Excel = GameData.ChallengeConfigData[ChallengeId];
                    ChallengeInstance instance = new ChallengeInstance(Player, Excel, ChallengeData.Instance);
                    this.ChallengeInstance = instance;
                }
            }
        }

        #endregion
    }

    // WatchAndyTW was here
}
