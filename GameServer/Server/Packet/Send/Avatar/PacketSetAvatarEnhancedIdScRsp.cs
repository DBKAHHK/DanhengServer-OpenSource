using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketSetAvatarEnhancedIdScRsp : BasePacket
{
    public PacketSetAvatarEnhancedIdScRsp(Retcode retcode) : base(CmdIds.SetAvatarEnhancedIdScRsp)
    {
        var proto = new SetAvatarEnhancedIdScRsp
        {
            Retcode = (uint)retcode
        };

        SetData(proto);
    }

    public PacketSetAvatarEnhancedIdScRsp(int avatarId) : base(CmdIds.SetAvatarEnhancedIdScRsp)
    {
        var proto = new SetAvatarEnhancedIdScRsp
        {
            AvatarPathId = (uint)avatarId
        };

        SetData(proto);
    }
}