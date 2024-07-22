using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueSelectCellScRsp : BasePacket
{
    public PacketChessRogueSelectCellScRsp(int cellId) : base(CmdIds.ChessRogueSelectCellScRsp)
    {
        var proto = new ChessRogueSelectCellScRsp
        {
            CellId = (uint)cellId
        };

        SetData(proto);
    }
}