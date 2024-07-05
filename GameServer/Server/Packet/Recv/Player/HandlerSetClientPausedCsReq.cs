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

            // DO NOT REMOVE THIS CODE
            // This code is responsible for sending the client data to the player
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmcgU2VydmVyIikgdGhlbgogICAgICAgIHVpZC50ZXh0ID0gIkRhbmhlbmdTZXJ2ZXLkuLrljYrlvIDmupDlhY3otLnmnI3liqHnq69cbuatpOacjeWKoeerr+S7heeUqOS9nOWtpuS5oOS6pOa1ge+8jOivt+aUr+aMgeato+eJiOa4uOaIjyAiIC4uIHVpZC50ZXh0CiAgICAgICAgdWlkLmZvbnRTaXplID0gNzYuMAogICAgZW5kCiAgICBsb2NhbCBiZXRhID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJVSVJvb3QvQWJvdmVEaWFsb2cvQmV0YUhpbnREaWFsb2coQ2xvbmUpIik6R2V0Q29tcG9uZW50KCJUZXh0IikKZW5kCgp2ZXJzaW9uX3RleHQoKQ==")));
                    break;
                case "CHT":
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmcgU2VydmVyIikgdGhlbgogICAgICAgIHVpZC50ZXh0ID0gIkRhbmhlbmdTZXJ2ZXLngrrljYrplovmupDlhY3osrvkvLrmnI3lmajou5/pq5RcbuatpOS8uuacjeWZqOi7n+mrlOWDheeUqOS9nOWtuOe/kuS6pOa1ge+8jOiri+aUr+aMgeato+eJiOmBiuaIsiAiIC4uIHVpZC50ZXh0CiAgICAgICAgdWlkLmZvbnRTaXplID0gNzYuMAogICAgZW5kCiAgICBsb2NhbCBiZXRhID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJVSVJvb3QvQWJvdmVEaWFsb2cvQmV0YUhpbnREaWFsb2coQ2xvbmUpIik6R2V0Q29tcG9uZW50KCJUZXh0IikKZW5kCgp2ZXJzaW9uX3RleHQoKQ==")));
                    break;
                default:
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmcgU2VydmVyIikgdGhlbgogICAgICAgIHVpZC50ZXh0ID0gIkRhbmhlbmcgU2VydmVyIGlzIGEgc2VtaSBvcGVuLXNvdXJjZSBzZXJ2ZXIgc29mdHdhcmUuXG5FZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp")));
                    break;
            }
        }
    }
}
