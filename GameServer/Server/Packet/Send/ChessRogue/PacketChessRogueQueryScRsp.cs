using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueQueryScRsp : BasePacket
{
    public PacketChessRogueQueryScRsp(PlayerInstance player) : base(CmdIds.ChessRogueQueryScRsp)
    {
        var proto = new ChessRogueQueryScRsp
        {
            RogueGetInfo = player.ChessRogueManager!.ToGetInfo(),
            Info = player.ChessRogueManager!.ToCurrentInfo(),
            QueryInfo = player.ChessRogueManager!.ToQueryInfo()
        };

        SetData(proto);
    }
}