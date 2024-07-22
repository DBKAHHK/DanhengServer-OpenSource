using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Raid;

public class PacketGetAllSaveRaidScRsp : BasePacket
{
    public PacketGetAllSaveRaidScRsp(PlayerInstance player) : base(CmdIds.GetAllSaveRaidScRsp)
    {
        var proto = new GetAllSaveRaidScRsp();

        foreach (var dict in player.RaidManager!.RaidData.RaidRecordDatas.Values)
        foreach (var record in dict.Values)
            proto.SavedData.Add(new RaidSavedData
            {
                RaidId = (uint)record.RaidId,
                WorldLevel = (uint)record.WorldLevel
            });

        SetData(proto);
    }
}