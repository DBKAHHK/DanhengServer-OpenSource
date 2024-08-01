using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketSetAvatarPathScRsp : BasePacket
{
    public PacketSetAvatarPathScRsp(int avatarId) : base(CmdIds.SetAvatarPathScRsp)
    {
        var proto = new SetAvatarPathScRsp
        {
            AvatarId = (MultiPathAvatarType)avatarId
        };

        SetData(proto);
    }
}