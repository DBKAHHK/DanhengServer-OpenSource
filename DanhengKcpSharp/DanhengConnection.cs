using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using EggLink.DanhengServer.Kcp.KcpSharp;
using EggLink.DanhengServer.Util;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace EggLink.DanhengServer.Kcp;

public class DanhengConnection
{
    public const int MAX_MSG_SIZE = 16384;
    public const int HANDSHAKE_SIZE = 20;
    public static readonly ConcurrentBag<int> BannedPackets = [];
    private static readonly Logger Logger = new("GameServer");
    public static readonly ConcurrentDictionary<int, string> LogMap = [];

    public static readonly ConcurrentBag<int> IgnoreLog =
    [
        CmdIds.PlayerHeartBeatCsReq, CmdIds.PlayerHeartBeatScRsp, CmdIds.SceneEntityMoveCsReq,
        CmdIds.SceneEntityMoveScRsp, CmdIds.GetShopListCsReq, CmdIds.GetShopListScRsp
    ];

    protected readonly CancellationTokenSource CancelToken;
    protected readonly KcpConversation Conversation;
    public readonly IPEndPoint RemoteEndPoint;

    public string DebugFile = "";
    public bool IsOnline = true;
    public StreamWriter? Writer;

    public byte[]? XorKey { get; set; }
    public ulong ClientSecretKeySeed { get; set; }

    public DanhengConnection(KcpConversation conversation, IPEndPoint remote)
    {
        Conversation = conversation;
        RemoteEndPoint = remote;
        CancelToken = new CancellationTokenSource();
        if (ConfigManager.Config.GameServer.UsePacketEncryption)
        {
#pragma warning disable CS8602 // CS8602 - Dereference of a possibly null reference.
            XorKey = Crypto.ClientSecretKey.GetXorKey();
#pragma warning restore CS8602 // CS8602 - Dereference of a possibly null reference.
        }
        Start();
    }

    public long? ConversationId => Conversation.ConversationId;

    public SessionStateEnum State { get; set; } = SessionStateEnum.INACTIVE;
    //public PlayerInstance? Player { get; set; }

    public virtual void Start()
    {
        Logger.Info($"New connection from {RemoteEndPoint}.");
        State = SessionStateEnum.WAITING_FOR_TOKEN;
    }

    public virtual void Stop()
    {
        //Player?.OnLogoutAsync();
        //Listener.UnregisterConnection(this);
        Conversation.Dispose();
        try
        {
            CancelToken.Cancel();
            CancelToken.Dispose();
        }
        catch
        {
        }

        IsOnline = false;
    }

    public void LogPacket(string sendOrRecv, ushort opcode, byte[] payload)
    {
        try
        {
            //Logger.DebugWriteLine($"{sendOrRecv}: {Enum.GetName(typeof(OpCode), opcode)}({opcode})\r\n{Convert.ToHexString(payload)}");
            if (IgnoreLog.Contains(opcode)) return;
            var typ = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == "DanhengProto")!.GetTypes()
                .First(t => t.Name == $"{LogMap[opcode]}"); //get the type using the packet name
            var descriptor =
                typ.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(
                    null, null) as MessageDescriptor; // get the static property Descriptor
            var packet = descriptor?.Parser.ParseFrom(payload);
            var formatter = JsonFormatter.Default;
            var asJson = formatter.Format(packet);
            var output = $"{sendOrRecv}: {LogMap[opcode]}({opcode})\r\n{asJson}";
#if DEBUG
            Logger.Debug(output);
#endif
            if (DebugFile == "" || !ConfigManager.Config.ServerOption.SavePersonalDebugFile) return;
            var sw = GetWriter();
            sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
            sw.Flush();
        }
        catch
        {
            var output = $"{sendOrRecv}: {LogMap.GetValueOrDefault(opcode, "UnknownPacket")}({opcode})";
#if DEBUG
            Logger.Debug(output);
#endif
            if (DebugFile != "" && ConfigManager.Config.ServerOption.SavePersonalDebugFile)
            {
                var sw = GetWriter();
                sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [GameServer] [DEBUG] " + output);
                sw.Flush();
            }
        }
    }

    private StreamWriter GetWriter()
    {
        // Create the file if it doesn't exist
        var file = new FileInfo(DebugFile);
        if (!file.Exists)
        {
            Directory.CreateDirectory(file.DirectoryName!);
            File.Create(DebugFile).Dispose();
        }

        Writer ??= new StreamWriter(DebugFile, true);
        return Writer;
    }

    public async Task SendPacket(byte[] packet)
    {
        try
        {
            if (ConfigManager.Config.GameServer.UsePacketEncryption)
                Crypto.Xor(packet, XorKey);
            _ = await Conversation.SendAsync(packet, CancelToken.Token);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SendPacket(BasePacket packet)
    {
        // Test
        if (packet.CmdId <= 0)
        {
            Logger.Debug("Tried to send packet with missing cmd id!");
            return;
        }

        // DO NOT REMOVE (unless we find a way to validate code before sending to client which I don't think we can)
        if (BannedPackets.Contains(packet.CmdId)) return;
        LogPacket("Send", packet.CmdId, packet.Data);
        // Header
        var packetBytes = packet.BuildPacket();

        try
        {
            //_ = await Conversation.SendAsync(packetBytes, CancelToken.Token);
            await SendPacket(packetBytes);
        }
        catch
        {
            // ignore
        }

        if (packet.CmdId == CmdIds.SetClientPausedScRsp)
        {
            BasePacket lData;
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcuS4uuWNiuW8gOa6kOWFjei0ueacjeWKoeerr1xu5q2k5pyN5Yqh56uv5LuF55So5L2c5a2m5Lmg5Lqk5rWB77yM6K+35pSv5oyB5q2j54mI5ri45oiPICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    break;
                case "CHT":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmdTZXJ2ZXIiKSB0aGVuCiAgICAgICAgdWlkLnRleHQgPSAiRGFuaGVuZ1NlcnZlcueCuuWNiumWi+a6kOWFjeiyu+S8uuacjeWZqOi7n+mrlFxu5q2k5Ly65pyN5Zmo6Luf6auU5YOF55So5L2c5a2457+S5Lqk5rWB77yM6KuL5pSv5oyB5q2j54mI6YGK5oiyICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    break;
                default:
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkKICAgIGxvY2FsIHVpZCA9IENTLlVuaXR5RW5naW5lLkdhbWVPYmplY3QuRmluZCgiVmVyc2lvblRleHQiKTpHZXRDb21wb25lbnQoIlRleHQiKQogICAgaWYgbm90IHN0cmluZy5tYXRjaCh1aWQudGV4dCwgIkRhbmhlbmcgU2VydmVyIikgdGhlbgogICAgICAgIHVpZC50ZXh0ID0gIkRhbmhlbmcgU2VydmVyIGlzIGEgc2VtaSBvcGVuLXNvdXJjZSBzZXJ2ZXIgc29mdHdhcmUuXG5FZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuICIgLi4gdWlkLnRleHQKICAgICAgICB1aWQuZm9udFNpemUgPSA3Ni4wCiAgICBlbmQKICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQplbmQKCnZlcnNpb25fdGV4dCgp"));
                    break;
            }

            await SendPacket(lData.BuildPacket());
        }

        if (packet.CmdId == CmdIds.GetTutorialScRsp)
        {
            BasePacket lData;
            switch (ConfigManager.Config.ServerOption.Language)
            {
                case "CHS":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9zci5taWhveW8uY29tLyIpCmVuZAoKbG9jYWwgZnVuY3Rpb24gc2hvd19oaW50KCkKICAgIGxvY2FsIHRleHQgPSAi5qyi6L+O5L2/55SoRGFuaGVuZ1NlcnZlcu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuacrOacjeWKoeWZqOWujOWFqOWFjei0ue+8jOWmguaenOaCqOaYr+i0reS5sOW+l+WIsOeahO+8jOmCo+S5iOaCqOW3sue7j+iiq+mql+S6hu+8gVxuIgogICAgdGV4dCA9IHRleHQgLi4gIuatpOacjeWKoeerr+S7heeUqOS9nOWtpuS5oOS6pOa1ge+8jOivt+aUr+aMgeato+eJiFxuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                    break;
                case "CHT":
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIuatoei/juS9v+eUqERhbmhlbmdTZXJ2ZXLvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmnKzkvLrmnI3lmajlrozlhajlhY3osrvvvIzlpoLmnpzmgqjmmK/os7zosrflvpfliLDnmoTjgILpgqPpurzmgqjlt7LntpPooqvpqJnkuobvvIFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICLmraTkvLrmnI3lmajou5/pq5Tlg4Xkvpvlrbjnv5LkuqTmtYHkvb/nlKjvvIzoq4vmlK/mjIHmraPniYhcbiIKICAgIENTLlJQRy5DbGllbnQuQ29uZmlybURpYWxvZ1V0aWwuU2hvd0N1c3RvbU9rQ2FuY2VsSGludCh0ZXh0LCBvbkRpYWxvZ0Nsb3NlZCkKZW5kCgpzaG93X2hpbnQoKQ=="));
                    break;
                default:
                    lData = new HandshakePacket(Convert.FromBase64String(
                        "bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgQ1MuVW5pdHlFbmdpbmUuQXBwbGljYXRpb24uT3BlblVSTCgiaHR0cHM6Ly9oc3IuaG95b3ZlcnNlLmNvbS8iKQplbmQKCmxvY2FsIGZ1bmN0aW9uIHNob3dfaGludCgpCiAgICBsb2NhbCB0ZXh0ID0gIldlbGNvbWUgdG8gRGFuaGVuZ1NlcnZlciFcbiIKICAgIHRleHQgPSB0ZXh0IC4uICJUaGlzIHNlcnZlciBzb2Z0d2FyZSBpcyB0b3RhbGx5IGZyZWUuXG4iCiAgICB0ZXh0ID0gdGV4dCAuLiAiSWYgeW91IHBheSBmb3IgaXQsIHlvdSBoYXZlIGJlZW4gc2NhbW1lZC5cbiIKICAgIHRleHQgPSB0ZXh0IC4uICJFZHVjYXRpb25hbCBwdXJwb3NlIG9ubHksIHBsZWFzZSBzdXBwb3J0IHRoZSBnZW51aW5lIGdhbWUuIgogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KHRleHQsIG9uRGlhbG9nQ2xvc2VkKQplbmQKCnNob3dfaGludCgp"));
                    break;
            }

            await SendPacket(lData.BuildPacket());
        }
    }

    public async Task SendPacket(int cmdId)
    {
        await SendPacket(new BasePacket((ushort)cmdId));
    }
}