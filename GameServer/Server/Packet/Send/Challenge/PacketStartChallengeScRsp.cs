using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Challenge
{
    public class PacketStartChallengeScRsp : BasePacket
    {
        public PacketStartChallengeScRsp(int Retcode) : base(CmdIds.StartChallengeScRsp)
        {
            StartChallengeScRsp proto = new StartChallengeScRsp
            {
                Retcode = (uint)Retcode,
            };

            SetData(proto);
        }

        public PacketStartChallengeScRsp(PlayerInstance player) : base(CmdIds.StartChallengeScRsp)
        {
            StartChallengeScRsp proto = new StartChallengeScRsp() { };

            if (player.ChallengeInstance != null)
            {
                proto.CurChallenge = player.ChallengeInstance!.ToProto();
                proto.Lineup = player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge)!.ToProto();
            }
            else
            {
                proto.Retcode = 1;
            }

            SetData(proto);
        }
    }
}
