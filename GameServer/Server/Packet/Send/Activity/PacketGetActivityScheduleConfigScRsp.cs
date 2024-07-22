using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Activity;

public class PacketGetActivityScheduleConfigScRsp : BasePacket
{
    public PacketGetActivityScheduleConfigScRsp(PlayerInstance player) : base(CmdIds.GetActivityScheduleConfigScRsp)
    {
        var proto = new GetActivityScheduleConfigScRsp();

        proto.ScheduleData.AddRange(player.ActivityManager!.ToProto());

        SetData(proto);
    }
}