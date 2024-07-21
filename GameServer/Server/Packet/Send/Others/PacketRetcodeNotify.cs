using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Others
{
    public class PacketRetcodeNotify : BasePacket
    {
        public PacketRetcodeNotify(Retcode retcode) : base(CmdIds.RetcodeNotify)
        {
            var proto = new RetcodeNotify
            {
                Retcode = retcode  // original proto is uint, i modify it to Retcode enum
            };

            SetData(proto);
        }
    }
}
