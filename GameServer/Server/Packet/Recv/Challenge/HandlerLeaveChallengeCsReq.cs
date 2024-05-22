using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.Server.Packet.Recv.Challenge
{
    [Opcode(CmdIds.LeaveChallengeCsReq)]
    public class HandlerLeaveChallengeCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var player = connection.Player!;

            // TODO: check for plane type
            if (player.SceneInstance != null)
            {
                // TODO: force quit battle

                // Reset lineup
                player.LineupManager!.SetExtraLineup(ExtraLineupType.LineupChallenge, []);
                player.LineupManager.SetExtraLineup(ExtraLineupType.LineupChallenge2, []);

                player.ChallengeInstance = null;
                player.ChallengeManager!.ClearInstance();

                // Leave scene
                player.LineupManager.SetCurLineup(0);

                int leaveEntryId = GameConstants.CHALLENGE_ENTRANCE;
                if (player.SceneInstance.LeaveEntityId != 0) leaveEntryId = player.SceneInstance.LeaveEntityId;
                player.EnterScene(leaveEntryId, 0, true);
            }

            connection.SendPacket(CmdIds.LeaveChallengeScRsp);
        }
    }
}
