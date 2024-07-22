namespace EggLink.DanhengServer.Server.Packet.Send.Gacha;

public class PacketGetFriendListInfoScRsp : BasePacket
{
    public PacketGetFriendListInfoScRsp(Connection connection) : base(CmdIds.GetFriendListInfoScRsp)
    {
        SetData(connection.Player!.FriendManager!.ToProto());
    }
}