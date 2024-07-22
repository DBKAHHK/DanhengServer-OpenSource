using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Scene;

public class PacketActivateFarmElementScRsp : BasePacket
{
    public PacketActivateFarmElementScRsp(uint entityId, PlayerInstance player) : base(CmdIds.ActivateFarmElementScRsp)
    {
        var proto = new ActivateFarmElementScRsp
        {
            EntityId = entityId,
            WorldLevel = (uint)player.Data.WorldLevel
        };

        SetData(proto);
    }
}