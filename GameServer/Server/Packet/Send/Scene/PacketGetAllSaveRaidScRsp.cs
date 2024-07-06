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
    public class PacketGetAllSaveRaidScRsp : BasePacket
    {
        public PacketGetAllSaveRaidScRsp(PlayerInstance player) : base(CmdIds.GetAllSaveRaidScRsp)
        {
            var proto = new GetAllSaveRaidScRsp()
            {
                OLOBGMGDENG = { player.RaidManager!.RaidData.RaidRecordData.Select(x => new MFGGIEBAMFG() { RaidId = (uint)x.Value.RaidId, WorldLevel = (uint)x.Value.WorldLevel }) }
            };

            SetData(proto);
        }
    }
}
