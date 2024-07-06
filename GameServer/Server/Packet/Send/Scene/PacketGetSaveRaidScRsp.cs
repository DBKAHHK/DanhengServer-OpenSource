using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene
{
    public class PacketGetSaveRaidScRsp : BasePacket
    {
        public PacketGetSaveRaidScRsp(PlayerInstance player, int raidId) : base(CmdIds.GetSaveRaidScRsp)
        {
            var proto = new GetSaveRaidScRsp();

            if (player.RaidManager!.RaidData.RaidRecordData.TryGetValue(raidId, out var record))
            {
                proto.RaidId = (uint)record.RaidId;
                proto.WorldLevel = (uint)record.WorldLevel;
            } 
            else
            {
                proto.Retcode = (uint)Retcode.RetRaidNoSave;
            }

            SetData(proto);
        }
    }
}
