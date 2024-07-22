using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Mission;

public class PacketGetNpcStatusScRsp : BasePacket
{
    public PacketGetNpcStatusScRsp(PlayerInstance player) : base(CmdIds.GetNpcStatusScRsp)
    {
        var proto = new GetNpcStatusScRsp();

        foreach (var item in GameData.MessageContactsConfigData.Values)
            proto.NpcStatusList.Add(new NpcStatus
            {
                NpcId = (uint)item.ID,
                IsFinish = player.MessageManager!.GetMessageGroup(item.ID).Count > 0
            });

        SetData(proto);
    }
}