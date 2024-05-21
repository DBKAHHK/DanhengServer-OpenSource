using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Battle
{
    [Opcode(CmdIds.StartChallengeCsReq)]
    public class HandlerStartChallengeCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = StartChallengeCsReq.Parser.ParseFrom(data);

            HDLDGEPFMGL? buffInfo = null;
            if (req.PlayerInfo != null && req.PlayerInfo.GGIAHBJHKGE != null)
            {
                buffInfo = req.PlayerInfo.GGIAHBJHKGE;
            };

            connection.Player!.ChallengeManager!.StartChallenge((int)req.ChallengeId, buffInfo);
        }
    }
}
