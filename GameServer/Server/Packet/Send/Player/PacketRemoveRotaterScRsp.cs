using EggLink.DanhengServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketRemoveRotaterScRsp : BasePacket
{
    public PacketRemoveRotaterScRsp(PlayerInstance player, RemoveRotaterCsReq req) : base(CmdIds.RemoveRotaterScRsp)
    {
        var proto = new RemoveRotaterScRsp
        {
            EnergyInfo = new RotatorEnergyInfo
            {
                CurNum = (uint)player.ChargerNum,
                MaxNum = 5
            },
            RotaterData = req.RotaterData
        };

        SetData(proto);
    }
}
