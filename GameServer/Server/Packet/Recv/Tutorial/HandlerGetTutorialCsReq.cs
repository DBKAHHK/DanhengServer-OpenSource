using EggLink.DanhengServer.Server.Packet.Send.Others;
using EggLink.DanhengServer.Server.Packet.Send.Tutorial;
using EggLink.DanhengServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Server.Packet.Recv.Tutorial
{
    [Opcode(CmdIds.GetTutorialCsReq)]
    public class HandlerGetTutorialCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            SendPlayerData(connection);
            if (ConfigManager.Config.ServerOption.EnableMission)  // If missions are enabled
                connection.SendPacket(new PacketGetTutorialScRsp(connection.Player!));
        }

        private void SendPlayerData(Connection connection)
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "Lua", "welcome.lua");
            if (File.Exists(filePath))
            {
                var fileBytes = File.ReadAllBytes(filePath);
                connection.SendPacket(new PacketClientDownloadDataScNotify(fileBytes));
            }

            // DO NOT REMOVE THIS CODE
            // This code is responsible for sending the client data to the player
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9zci5taWhveW8uY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5qyi6L+O5L2/55SoRGFuaGVuZ1NlcnZlcu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuacrOacjeWKoeWZqOWujOWFqOWFjei0ue+8jOWmguaenOaCqOaYr+i0reS5sOW+l+WIsOeahO+8jOmCo+S5iOaCqOW3sue7j+iiq+mql+S6hu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuatpOacjeWKoeerr+S7heeUqOS9nOWtpuS5oOS6pOa1ge+8jOivt+aUr+aMgeato+eJiFxuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp")));
                    break;
                case "CHT":
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIuatoei/juS9v+eUqERhbmhlbmdTZXJ2ZXLvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmnKzkvLrmnI3lmajlrozlhajlhY3osrvvvIzlpoLmnpzmgqjmmK/os7zosrflvpfliLDnmoTjgILpgqPpurzmgqjlt7LntpPooqvpqJnkuobvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmraTkvLrmnI3lmajou5/pq5Tlg4Xkvpvlrbjnv5LkuqTmtYHkvb/nlKjvvIzoq4vmlK/mjIHmraPniYhcbiIKICAgIENTLlJQRy5DbGllbnQuQ29uZmlybURpYWxvZ1V0aWwuU2hvd0N1c3RvbU9rQ2FuY2VsSGludCh0ZXh0LCBvbkRpYWxvZ0Nsb3NlZCkKZW5kCgpzaG93X2hpbnQoKQ==")));
                    break;
                default:
                    connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIldlbGNvbWUgdG8gRGFuaGVuZ1NlcnZlciFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICJUaGlzIHNlcnZlciBzb2Z0d2FyZSBpcyB0b3RhbGx5IGZyZWUuXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAiSWYgeW91IHBheSBmb3IgaXQsIHlvdSBoYXZlIGJlZW4gc2NhbW1lZC5cbiIKICAgIHRleHQgPSB0ZXh0IC4uICJFZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp")));
                    break;
            }
        }
    }
}
