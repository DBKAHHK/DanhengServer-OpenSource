using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.Server.Packet.Send.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene
{
    [Opcode(CmdIds.StartRaidCsReq)]
    public class HandlerStartRaidCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = StartRaidCsReq.Parser.ParseFrom(data);
            var player = connection.Player!;

            player.RaidManager!.EnterRaid((int)req.RaidId, (int)req.WorldLevel, req.AvatarList.Select(x => (int)x).ToList());

            connection.SendPacket(CmdIds.StartRaidScRsp);
        }
    }
}
