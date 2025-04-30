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
            if (!proto.CurAvatarPath.ContainsKey((uint)multiPathAvatar.BaseAvatarID))
            {
                var avatar = player.AvatarManager!.GetFormalAvatar(multiPathAvatar.BaseAvatarID);
                if (avatar != null)
                {
                    if (avatar.BaseAvatarId == 8001) // only add main character
                        proto.BasicTypeIdList.Add((uint)avatar.AvatarId);
                    var pathId = avatar.AvatarId;

                    proto.CurAvatarPath.Add((uint)avatar.BaseAvatarId, (MultiPathAvatarType)pathId);
                    if (avatar.BaseAvatarId == multiPathAvatar.BaseAvatarID)
                        proto.MultiPathAvatarInfoList.Add(avatar.ToAvatarPathProto());
                }
            }

        SetData(proto);
    }
}