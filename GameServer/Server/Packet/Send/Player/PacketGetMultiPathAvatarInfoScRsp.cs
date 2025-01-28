using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketGetMultiPathAvatarInfoScRsp : BasePacket
{
    public PacketGetMultiPathAvatarInfoScRsp(PlayerInstance player) : base(CmdIds.GetMultiPathAvatarInfoScRsp)
    {
        var proto = new GetMultiPathAvatarInfoScRsp();

        foreach (var multiPathAvatar in GameData.MultiplePathAvatarConfigData.Values)
        {
            if (multiPathAvatar.AvatarID != multiPathAvatar.BaseAvatarID) continue;

            var avatar = player.AvatarManager!.GetAvatar(multiPathAvatar.BaseAvatarID);
            if (avatar == null) continue;

            if (avatar.BaseAvatarId == 8001)
                proto.BasicTypeIdList.Add((uint)avatar.CurAvatarId);
            proto.CurAvatarPath.Add((uint)avatar.BaseAvatarId, (MultiPathAvatarType)avatar.CurAvatarId);
            proto.MultiPathAvatarInfoList.Add(avatar.ToAvatarPathProto());
        }

        SetData(proto);
    }
}