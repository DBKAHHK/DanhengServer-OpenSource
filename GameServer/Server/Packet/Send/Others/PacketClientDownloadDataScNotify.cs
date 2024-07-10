
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Google.Protobuf;
using System;

namespace EggLink.DanhengServer.Server.Packet.Send.Others
{
    public class PacketClientDownloadDataScNotify : BasePacket
    {
        public PacketClientDownloadDataScNotify(byte[] data) : base(CmdIds.ClientDownloadDataScNotify)
        {
            var downloadData = new ClientDownloadData
            {
                Data = ByteString.CopyFrom(data),
                Version = 81,
                Time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
            };
            var notify = new ClientDownloadDataScNotify
            {
                DownloadData = downloadData
            };

            SetData(notify);
        }

        public PacketClientDownloadDataScNotify(string base64) : base(CmdIds.ClientDownloadDataScNotify)
        {
            SetData(Convert.FromBase64String(base64));
        }
    }
}
