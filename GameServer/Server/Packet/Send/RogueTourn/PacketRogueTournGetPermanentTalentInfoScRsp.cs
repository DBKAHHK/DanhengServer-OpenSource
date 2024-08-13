using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueTourn;

public class PacketRogueTournGetPermanentTalentInfoScRsp : BasePacket
{
    public PacketRogueTournGetPermanentTalentInfoScRsp(PlayerInstance player) : base(
        CmdIds.RogueTournGetPermanentTalentInfoScRsp)
    {
        var proto = new RogueTournGetPermanentTalentInfoScRsp
        {
            InspirationCircuit = player.RogueTournManager!.ToInspirationCircuitProto()
        };

        SetData(proto);
    }
}