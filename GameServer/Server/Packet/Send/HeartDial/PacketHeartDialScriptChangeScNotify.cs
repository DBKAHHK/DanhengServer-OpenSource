using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.HeartDial
{
    public class PacketHeartDialScriptChangeScNotify : BasePacket
    {
        public PacketHeartDialScriptChangeScNotify(HeartDialUnlockStatus status) : base(CmdIds.HeartDialScriptChangeScNotify)
        {
            var proto = new HeartDialScriptChangeScNotify
            {
                UnlockStatus = status
            };

            SetData(proto);
        }
    }
}
