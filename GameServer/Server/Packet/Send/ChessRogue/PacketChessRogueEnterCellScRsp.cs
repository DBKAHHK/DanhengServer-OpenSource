using EggLink.DanhengServer.Game.ChessRogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueEnterCellScRsp : BasePacket
{
    public PacketChessRogueEnterCellScRsp(uint cellId, ChessRogueInstance rogue) : base(CmdIds.ChessRogueEnterCellScRsp)
    {
        var proto = new ChessRogueEnterCellScRsp
        {
            CellId = cellId,
            Info = rogue.ToProto(),
            PlayerInfo = rogue.ToPlayerProto(),
            RogueCurrentInfo = rogue.ToRogueGameInfo()
        };

        SetData(proto);
    }
}