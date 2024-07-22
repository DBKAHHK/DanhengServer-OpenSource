using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Mission;

public class PacketFinishTalkMissionScRsp : BasePacket
{
    public PacketFinishTalkMissionScRsp(string talkStr) : base(CmdIds.FinishTalkMissionScRsp)
    {
        var proto = new FinishTalkMissionScRsp
        {
            TalkStr = talkStr
        };

        SetData(proto);
    }
}