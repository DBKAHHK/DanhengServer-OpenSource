using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;

public class PacketGridFightGetDataScRsp : BasePacket
{
    public PacketGridFightGetDataScRsp(PlayerInstance player) : base(CmdIds.GridFightGetDataScRsp)
    {
        var proto = new GridFightGetDataScRsp
        {
            RogueGetInfo = player.GridFightManager!.ToProto(),
            //FightCurrentInfo = player.GridFightManager!.ToCurrentInfo()
        };

        SetData(proto);
    }
}