using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketChessRogueUpdateLevelBaseInfoScNotify : BasePacket
{
    public PacketChessRogueUpdateLevelBaseInfoScNotify(ChessRogueLevelStatusType status) : base(
        CmdIds.ChessRogueUpdateLevelBaseInfoScNotify)
    {
        var proto = new ChessRogueUpdateLevelBaseInfoScNotify
        {
            LevelStatus = status
        };

        SetData(proto);
    }
}