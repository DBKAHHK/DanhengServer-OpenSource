using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Battle
{
    [Opcode(CmdIds.StartChallengeCsReq)]
    public class HandlerStartChallengeCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = StartChallengeCsReq.Parser.ParseFrom(data);

            StartChallengeStoryBuffInfo? storyBuffInfo = null;
            if (req.PlayerInfo != null && req.PlayerInfo.StoryBuffInfo != null)
            {
                storyBuffInfo = req.PlayerInfo.StoryBuffInfo;
            };

            StartChallengeBossBuffInfo? bossBuffInfo = null;
            if (req.PlayerInfo != null && req.PlayerInfo.BossBuffInfo != null)
            {
                bossBuffInfo = req.PlayerInfo.BossBuffInfo;
            };
            
            if (req.TeamOne.Count > 0)
            {
                List<int> team = new();
                foreach (int id in req.TeamOne)
                {
                    team.Add((int)id);
                };
                connection.Player!.LineupManager!.ReplaceLineup(0, team, ExtraLineupType.LineupChallenge);
            }

            if (req.TeamTwo.Count > 0)
            {
                List<int> team = new();
                foreach (int id in req.TeamTwo)
                {
                    team.Add((int)id);
                };
                connection.Player!.LineupManager!.ReplaceLineup(0, team, ExtraLineupType.LineupChallenge2);
            }

            connection.Player!.ChallengeManager!.StartChallenge((int)req.ChallengeId, storyBuffInfo, bossBuffInfo);
        }
    }
}
