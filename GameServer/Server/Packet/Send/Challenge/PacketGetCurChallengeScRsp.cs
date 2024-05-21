using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Battle
{
    public class PacketGetCurChallengeScRsp : BasePacket
    {
        public PacketGetCurChallengeScRsp(PlayerInstance player) : base(CmdIds.GetCurChallengeScRsp)
        {
            var proto = new GetCurChallengeScRsp() { };

            if (player.ChallengeInstance != null)
            {
                proto.CurChallenge = player.ChallengeInstance.ToProto();
            }
            else
            {
                proto.Retcode = 0;
            }

            SetData(proto);
        }
    }
}
