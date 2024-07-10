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
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = SetClientPausedCsReq.Parser.ParseFrom(data);
            var paused = req.Paused;
            connection.SendPacket(new PacketSetClientPausedScRsp(paused));
            if (ConfigManager.Config.ServerOption.ServerAnnounce.EnableAnnounce)
                connection.SendPacket(new PacketServerAnnounceNotify());

            // DO NOT REMOVE THIS CODE
            // This code is responsible for sending the client data to the player
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    //var data = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcuS4uuWNiuW8gOa6kOWFjei0ueacjeWKoeerr1xu5q2k5pyN5Yqh56uv5LuF55So5L2c5a2m5Lmg5Lqk5rWB77yM6K+35pSv5oyB5q2j54mI5ri45oiPICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    connection.SendPacket(new PacketClientDownloadDataScNotify("OuIDCFEQ18u5tAYa1wNsb2NhbCBmdW5jdGlvbiB2ZXJzaW9uX3RleHQoKQogICAgbG9jYWwgdWlkID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJWZXJzaW9uVGV4dCIpOkdldENvbXBvbmVudCgiVGV4dCIpCiAgICBpZiBub3Qgc3RyaW5nLm1hdGNoKHVpZC50ZXh0LCAiRGFuaGVuZ1NlcnZlciIpIHRoZW4KICAgICAgICB1aWQudGV4dCA9ICJEYW5oZW5nU2VydmVy5Li65Y2K5byA5rqQ5YWN6LS55pyN5Yqh56uvXG7mraTmnI3liqHnq6/ku4XnlKjkvZzlrabkuaDkuqTmtYHvvIzor7fmlK/mjIHmraPniYjmuLjmiI8gIiAuLiB1aWQudGV4dAogICAgICAgIHVpZC5mb250U2l6ZSA9IDc2LjAKICAgIGVuZAogICAgbG9jYWwgYmV0YSA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVUlSb290L0Fib3ZlRGlhbG9nL0JldGFIaW50RGlhbG9nKENsb25lKSIpOkdldENvbXBvbmVudCgiVGV4dCIpCmVuZAoKdmVyc2lvbl90ZXh0KCk="));
                    break;
                case "CHT":
                    //var data2 = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcueCuuWNiumWi+a6kOWFjeiyu+S8uuacjeWZqOi7n+mrlFxu5q2k5Ly65pyN5Zmo6Luf6auU5YOF55So5L2c5a2457+S5Lqk5rWB77yM6KuL5pSv5oyB5q2j54mI6YGK5oiyICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    connection.SendPacket(new PacketClientDownloadDataScNotify("Ou4DCFEQ18u5tAYa4wNsb2NhbCBmdW5jdGlvbiB2ZXJzaW9uX3RleHQoKQogICAgbG9jYWwgdWlkID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJWZXJzaW9uVGV4dCIpOkdldENvbXBvbmVudCgiVGV4dCIpCiAgICBpZiBub3Qgc3RyaW5nLm1hdGNoKHVpZC50ZXh0LCAiRGFuaGVuZ1NlcnZlciIpIHRoZW4KICAgICAgICB1aWQudGV4dCA9ICJEYW5oZW5nU2VydmVy54K65Y2K6ZaL5rqQ5YWN6LK75Ly65pyN5Zmo6Luf6auUXG7mraTkvLrmnI3lmajou5/pq5Tlg4XnlKjkvZzlrbjnv5LkuqTmtYHvvIzoq4vmlK/mjIHmraPniYjpgYrmiLIgIiAuLiB1aWQudGV4dAogICAgICAgIHVpZC5mb250U2l6ZSA9IDc2LjAKICAgIGVuZAogICAgbG9jYWwgYmV0YSA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVUlSb290L0Fib3ZlRGlhbG9nL0JldGFIaW50RGlhbG9nKENsb25lKSIpOkdldENvbXBvbmVudCgiVGV4dCIpCmVuZAoKdmVyc2lvbl90ZXh0KCk="));
                    break;
                default:
                    //var data3 = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmcgU2VydmVyIikgdGhlbgogICAgICAgIHVpZC50ZXh0ID0gIkRhbmhlbmcgU2VydmVyIGlzIGEgc2VtaSBvcGVuLXNvdXJjZSBzZXJ2ZXIgc29mdHdhcmUuXG5FZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    connection.SendPacket(new PacketClientDownloadDataScNotify("OvEDCFEQ18u5tAYa5gNsb2NhbCBmdW5jdGlvbiB2ZXJzaW9uX3RleHQoKQogICAgbG9jYWwgdWlkID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJWZXJzaW9uVGV4dCIpOkdldENvbXBvbmVudCgiVGV4dCIpCiAgICBpZiBub3Qgc3RyaW5nLm1hdGNoKHVpZC50ZXh0LCAiRGFuaGVuZyBTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZyBTZXJ2ZXIgaXMgYSBzZW1pIG9wZW4tc291cmNlIHNlcnZlciBzb2Z0d2FyZS5cbkVkdWNhdGlvbmFsIHB1cnBvc2Ugb25seSwgcGxlYXNlIHN1cHBvcnQgdGhlIGdlbnVpbmUgZ2FtZS4gIiAuLiB1aWQudGV4dAogICAgICAgIHVpZC5mb250U2l6ZSA9IDc2LjAKICAgIGVuZAogICAgbG9jYWwgYmV0YSA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVUlSb290L0Fib3ZlRGlhbG9nL0JldGFIaW50RGlhbG9nKENsb25lKSIpOkdldENvbXBvbmVudCgiVGV4dCIpCmVuZAoKdmVyc2lvbl90ZXh0KCk="));
                    break;
            }
        }
    }
}
