using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketGetRogueInfoScRsp : BasePacket
{
    public PacketGetRogueInfoScRsp(PlayerInstance player) : base(CmdIds.GetRogueInfoScRsp)
    {
        var proto = new GetRogueInfoScRsp
        {
            RogueInfo = player.RogueManager!.ToProto()
        };

        SetData(proto);
    }
}