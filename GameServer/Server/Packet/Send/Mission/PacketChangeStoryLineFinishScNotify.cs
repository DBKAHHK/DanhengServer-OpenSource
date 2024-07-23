using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Mission;

public class PacketChangeStoryLineFinishScNotify : BasePacket
{
    public PacketChangeStoryLineFinishScNotify(int curId, ChangeStoryLineAction reason) : base(
        CmdIds.ChangeStoryLineFinishScNotify)
    {
        var proto = new ChangeStoryLineFinishScNotify
        {
            ActionType = reason,
            CurStoryLineId = (uint)curId
        };

        SetData(proto);
    }
}