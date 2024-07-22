using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Mission;

public class PacketGetStoryLineInfoScRsp : BasePacket
{
    public PacketGetStoryLineInfoScRsp(PlayerInstance player) : base(CmdIds.GetStoryLineInfoScRsp)
    {
        var proto = new GetStoryLineInfoScRsp
        {
            CurStoryLineId = (uint)player.StoryLineManager!.StoryLineData.CurStoryLineId,
            RunningStoryLineIdList =
                { player.StoryLineManager!.StoryLineData.RunningStoryLines.Keys.Select(x => (uint)x) }
        };

        SetData(proto);
    }
}