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
        public ChallengeData ChallengeData { get; private set; } = DatabaseHelper.Instance!.GetInstanceOrCreateNew<ChallengeData>(player.Uid);

        #region Management

        public void StartChallenge(int challengeId, HDLDGEPFMGL? /*StartChallengeStoryBuffInfo*/ storyBuffs)
        {
            // Get challenge excel
            if (!GameData.ChallengeConfigData.ContainsKey(challengeId))
            {
                Player.SendPacket(new PacketStartChallengeScRsp(2801 /*CHALLENGE_NOT_EXIST*/));
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
                    Player.SendPacket(new PacketStartChallengeScRsp(2805));
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
                    Player.SendPacket(new PacketStartChallengeScRsp(2805));
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
            Player.ChallengeInstance = instance;

            // Set first lineup before we enter scenes
            instance.SetCurrentExtraLineup(ExtraLineupType.LineupChallenge);
            Player.LineupManager!.SetCurLineup(instance.CurrentExtraLineup);

            // Enter scene
            try
            {
                Player.EnterScene(Excel.MapEntranceID, 0, true);
            }
            catch
            {
                // Reset lineup/instance if entering scene failed
                Player.ChallengeInstance = null;

                // Send error packet
                Player.SendPacket(new PacketStartChallengeScRsp(2801));
                return;
            }

            // Save start positions
            instance.StartPos = Player.LastPos!;
            instance.StartRot = Player.LastRot!;
            instance.SavedMp = Player.LineupManager.GetCurLineup()!.Mp;

            if (Excel.IsStory() && storyBuffs != null)
            {
                instance.StoryBuffs.Add((int)storyBuffs.GPPEGLNNGNJ); // StoryBuffOne
                instance.StoryBuffs.Add((int)storyBuffs.AKEOMNPOJCE); // StoryBuffTwo
            }

            // Early implementation for 2.3
            /* if (BossBuffs != null)
            {
                instance.AddBossBuff((int)BossBuffs.AKEOMNPOJCE);
                instance.AddBossBuff((int)BossBuffs.GPPEGLNNGNJ);
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

            DatabaseHelper.Instance?.UpdateInstance(ChallengeData);
        }

        public void ClearInstance()
        {
            ChallengeData.Instance.ChallengeId = 0;
            DatabaseHelper.Instance?.UpdateInstance(ChallengeData);
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
                    Player.ChallengeInstance = instance;
                }
            }
        }

        #endregion
    }

    // WatchAndyTW was here
}
