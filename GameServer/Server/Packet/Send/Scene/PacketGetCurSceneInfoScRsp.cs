using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Scene;

public class PacketGetCurSceneInfoScRsp : BasePacket
{
    public PacketGetCurSceneInfoScRsp(PlayerInstance player) : base(CmdIds.GetCurSceneInfoScRsp)
    {
        var proto = new GetCurSceneInfoScRsp
        {
            Scene = player.SceneInstance!.ToProto()
        };

        SetData(proto);
    }
}