using System;
using System.IO;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using EggLink.DanhengServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Server.Packet.Send.Others;
namespace EggLink.DanhengServer.Server.Packet.Recv.Player
{
    [Opcode(CmdIds.SetClientPausedCsReq)]
    public class HandlerSetClientPausedCsReq : Handler
    {
        private static readonly Logger Logger = new("GameServer");
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = SetClientPausedCsReq.Parser.ParseFrom(data);
            var paused = req.Paused;
            connection.SendPacket(new PacketSetClientPausedScRsp(paused));
            if (ConfigManager.Config.ServerOption.ServerAnnounce.EnableAnnounce)
                connection.SendPacket(new PacketServerAnnounceNotify());

            // send the new client data, DO NOT REMOVE THIS CODE
            connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkNCiAgICBsb2NhbCB1aWQgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlZlcnNpb25UZXh0Iik6R2V0Q29tcG9uZW50KCJUZXh0IikNCiAgICBpZiBub3Qgc3RyaW5nLm1hdGNoKHVpZC50ZXh0LCAi5q2k5pyN5Yqh56uvIikgdGhlbg0KICAgICAgICB1aWQudGV4dCA9ICLmraTmnI3liqHnq6/ku4XnlKjkvZzlrabkuaDkuqTmtYHvvIzor7fmlK/mjIHmraPniYjmuLjmiI9cbiIgLi4gdWlkLnRleHQNCiAgICAgICAgdWlkLmZvbnRTaXplID0gNzYuMA0KICAgIGVuZA0KICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQ0KZW5kDQoNCnZlcnNpb25fdGV4dCgp")));
        }
    }
}
