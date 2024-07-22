using EggLink.DanhengServer.Game.ChessRogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

public class PacketEnhanceChessRogueBuffScRsp : BasePacket
{
    public PacketEnhanceChessRogueBuffScRsp(ChessRogueInstance rogue, uint buffId) : base(
        CmdIds.EnhanceChessRogueBuffScRsp)
    {
        var proto = new EnhanceChessRogueBuffScRsp
        {
            IsSuccess = true,
            RogueBuff = new RogueCommonBuff
            {
                BuffId = buffId,
                BuffLevel = 2
            },
            BuffEnhanceInfo = rogue.ToChessEnhanceInfo()
        };

        SetData(proto);
    }
}