using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.HeartDial;

public class PacketFinishEmotionDialoguePerformanceScRsp : BasePacket
{
    public PacketFinishEmotionDialoguePerformanceScRsp(uint scriptId, uint dialogueId) : base(
        CmdIds.FinishEmotionDialoguePerformanceScRsp)
    {
        var proto = new FinishEmotionDialoguePerformanceScRsp
        {
            DialogueId = dialogueId,
            ScriptId = scriptId,
            RewardList = new ItemList()
        };

        SetData(proto);
    }
}