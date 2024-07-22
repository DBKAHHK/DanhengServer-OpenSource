using EggLink.DanhengServer.Game.ChessRogue.Dice;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueReRollDiceScRsp : BasePacket
{
    public PacketChessRogueReRollDiceScRsp(ChessRogueDiceInstance dice) : base(CmdIds.ChessRogueReRollDiceScRsp)
    {
        var proto = new ChessRogueReRollDiceScRsp
        {
            RogueDiceInfo = dice.ToProto()
        };

        SetData(proto);
    }
}