using EggLink.DanhengServer.GameServer.Server.Packet.Send.Others;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Tutorial;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Tutorial;

[Opcode(CmdIds.GetTutorialCsReq)]
public class HandlerGetTutorialCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await SendPlayerData(connection);
        if (ConfigManager.Config.ServerOption.EnableMission) // If missions are enabled
            await connection.SendPacket(new PacketGetTutorialScRsp(connection.Player!));
    }

    private async Task SendPlayerData(Connection connection)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "Lua", "welcome.lua");
        if (File.Exists(filePath))
        {
            var fileBytes = File.ReadAllBytes(filePath);
            await connection.SendPacket(new PacketClientDownloadDataScNotify(fileBytes));
        }

        // DO NOT REMOVE THIS CODE
        // This code is responsible for sending the client data to the player
        switch (ConfigManager.Config.ServerOption.Language)
        {
            case "CHS":
                //var data = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9zci5taWhveW8uY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5qyi6L+O5L2/55SoRGFuaGVuZ1NlcnZlcu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuacrOacjeWKoeWZqOWujOWFqOWFjei0ue+8jOWmguaenOaCqOaYr+i0reS5sOW+l+WIsOeahO+8jOmCo+S5iOaCqOW3sue7j+iiq+mql+S6hu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuatpOacjeWKoeerr+S7heeUqOS9nOWtpuS5oOS6pOa1ge+8jOivt+aUr+aMgeato+eJiFxuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                await connection.SendPacket(new BasePacket(5)
                {
                    Data = Convert.FromBase64String(
                        "OtYDCFEQzsq5tAYaywNsb2NhbCBmdW5jdGlvbiBvbkRpYWxvZ0Nsb3NlZCgpCiAgICBDUy5Vbml0eUVuZ2luZS5BcHBsaWNhdGlvbi5PcGVuVVJMKCJodHRwczovL3NyLm1paG95by5jb20vIikKZW5kCgpsb2NhbCBmdW5jdGlvbiBzaG93X2hpbnQoKQogICAgbG9jYWwgdGV4dCA9ICLmrKLov47kvb/nlKhEYW5oZW5nU2VydmVy77yBXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5pys5pyN5Yqh5Zmo5a6M5YWo5YWN6LS577yM5aaC5p6c5oKo5piv6LSt5Lmw5b6X5Yiw55qE77yM6YKj5LmI5oKo5bey57uP6KKr6aqX5LqG77yBXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAi5q2k5pyN5Yqh56uv5LuF55So5L2c5a2m5Lmg5Lqk5rWB77yM6K+35pSv5oyB5q2j54mIXG4iCiAgICBDUy5SUEcuQ2xpZW50LkNvbmZpcm1EaWFsb2dVdGlsLlNob3dDdXN0b21Pa0NhbmNlbEhpbnQodGV4dCwgb25EaWFsb2dDbG9zZWQpCmVuZAoKc2hvd19oaW50KCk=")
                });
                break;
            case "CHT":
                //var data2 = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIuatoei/juS9v+eUqERhbmhlbmdTZXJ2ZXLvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmnKzkvLrmnI3lmajlrozlhajlhY3osrvvvIzlpoLmnpzmgqjmmK/os7zosrflvpfliLDnmoTjgILpgqPpurzmgqjlt7LntpPooqvpqJnkuobvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmraTkvLrmnI3lmajou5/pq5Tlg4Xkvpvlrbjnv5LkuqTmtYHkvb/nlKjvvIzoq4vmlK/mjIHmraPniYhcbiIKICAgIENTLlJQRy5DbGllbnQuQ29uZmlybURpYWxvZ1V0aWwuU2hvd0N1c3RvbU9rQ2FuY2VsSGludCh0ZXh0LCBvbkRpYWxvZ0Nsb3NlZCkKZW5kCgpzaG93X2hpbnQoKQ=="));
                await connection.SendPacket(new BasePacket(5)
                {
                    Data = Convert.FromBase64String(
                        "OuMDCFEQzsq5tAYa2ANsb2NhbCBmdW5jdGlvbiBvbkRpYWxvZ0Nsb3NlZCgpCiAgICBDUy5Vbml0eUVuZ2luZS5BcHBsaWNhdGlvbi5PcGVuVVJMKCJodHRwczovL2hzci5ob3lvdmVyc2UuY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5q2h6L+O5L2/55SoRGFuaGVuZ1NlcnZlcu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuacrOS8uuacjeWZqOWujOWFqOWFjeiyu++8jOWmguaenOaCqOaYr+izvOiyt+W+l+WIsOeahOOAgumCo+m6vOaCqOW3sue2k+iiq+momeS6hu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuatpOS8uuacjeWZqOi7n+mrlOWDheS+m+WtuOe/kuS6pOa1geS9v+eUqO+8jOiri+aUr+aMgeato+eJiFxuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp")
                });
                break;
            default:
                //var data3 = new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIldlbGNvbWUgdG8gRGFuaGVuZ1NlcnZlciFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICJUaGlzIHNlcnZlciBzb2Z0d2FyZSBpcyB0b3RhbGx5IGZyZWUuXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAiSWYgeW91IHBheSBmb3IgaXQsIHlvdSBoYXZlIGJlZW4gc2NhbW1lZC5cbiIKICAgIHRleHQgPSB0ZXh0IC4uICJFZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                await connection.SendPacket(new BasePacket(5)
                {
                    Data = Convert.FromBase64String(
                        "Ou4DCFEQzsq5tAYa4wNsb2NhbCBmdW5jdGlvbiBvbkRpYWxvZ0Nsb3NlZCgpCiAgICBDUy5Vbml0eUVuZ2luZS5BcHBsaWNhdGlvbi5PcGVuVVJMKCJodHRwczovL2hzci5ob3lvdmVyc2UuY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAiV2VsY29tZSB0byBEYW5oZW5nU2VydmVyIVxuIgogICAgdGV4dCA9IHRleHQgLi4gIlRoaXMgc2VydmVyIHNvZnR3YXJlIGlzIHRvdGFsbHkgZnJlZS5cbiIKICAgIHRleHQgPSB0ZXh0IC4uICJJZiB5b3UgcGF5IGZvciBpdCwgeW91IGhhdmUgYmVlbiBzY2FtbWVkLlxuIgogICAgdGV4dCA9IHRleHQgLi4gIkVkdWNhdGlvbmFsIHB1cnBvc2Ugb25seSwgcGxlYXNlIHN1cHBvcnQgdGhlIGdlbnVpbmUgZ2FtZS4iCiAgICBDUy5SUEcuQ2xpZW50LkNvbmZpcm1EaWFsb2dVdGlsLlNob3dDdXN0b21Pa0NhbmNlbEhpbnQodGV4dCwgb25EaWFsb2dDbG9zZWQpCmVuZAoKc2hvd19oaW50KCk=")
                });
                break;
        }
    }
}