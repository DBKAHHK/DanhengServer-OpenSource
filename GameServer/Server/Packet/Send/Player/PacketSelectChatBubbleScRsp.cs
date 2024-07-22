using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Player;

public class PacketSelectChatBubbleScRsp : BasePacket
{
    public PacketSelectChatBubbleScRsp(uint bubbleId) : base(CmdIds.SelectChatBubbleScRsp)
    {
        var proto = new SelectChatBubbleScRsp
        {
            CurChatBubble = bubbleId
        };

        SetData(proto);
    }
}