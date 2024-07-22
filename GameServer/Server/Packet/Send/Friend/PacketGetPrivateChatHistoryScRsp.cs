using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Friend;

public class PacketGetPrivateChatHistoryScRsp : BasePacket
{
    public PacketGetPrivateChatHistoryScRsp(uint contactId, PlayerInstance player) : base(
        CmdIds.GetPrivateChatHistoryScRsp)
    {
        var proto = new GetPrivateChatHistoryScRsp
        {
            ContactId = contactId
        };

        var infos = player.FriendManager!.GetHistoryInfo((int)contactId);
        proto.ChatMessageList.AddRange(infos);

        SetData(proto);
    }
}