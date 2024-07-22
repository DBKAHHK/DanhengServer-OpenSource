namespace EggLink.DanhengServer.Server.Packet.Send.Gacha;

public class PacketGetFriendApplyListInfoCsReq : BasePacket
{
    public PacketGetFriendApplyListInfoCsReq(Connection connection) : base(CmdIds.GetFriendApplyListInfoScRsp)
    {
        SetData(connection.Player!.FriendManager!.ToApplyListProto());
    }
}