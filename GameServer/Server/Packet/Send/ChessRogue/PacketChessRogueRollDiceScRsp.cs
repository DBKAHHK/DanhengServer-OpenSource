using EggLink.DanhengServer.Game.ChessRogue.Dice;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueRollDiceScRsp : BasePacket
{
    public PacketChessRogueRollDiceScRsp(ChessRogueDiceInstance dice) : base(CmdIds.ChessRogueRollDiceScRsp)
    {
        var proto = new ChessRogueRollDiceScRsp
        {
            RogueDiceInfo = dice.ToProto()
        };

        SetData(proto);
    }
}