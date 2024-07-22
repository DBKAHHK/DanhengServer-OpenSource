using System.Buffers;
using System.Net;
using System.Reflection;
using EggLink.DanhengServer.Common.Enums;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Server.Packet;
using EggLink.DanhengServer.Util;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using KcpSharp;

namespace EggLink.DanhengServer.Server;

public class Connection
{
    public static readonly List<int> BANNED_PACKETS = [];
    private static readonly Logger Logger = new("GameServer");
    public static readonly Dictionary<string, string> LogMap = [];

    public static readonly List<int> IgnoreLog =
    [
        CmdIds.PlayerHeartBeatCsReq, CmdIds.PlayerHeartBeatScRsp, CmdIds.SceneEntityMoveCsReq,
        CmdIds.SceneEntityMoveScRsp, CmdIds.GetShopListCsReq, CmdIds.GetShopListScRsp
    ];

    private readonly CancellationTokenSource CancelToken;
    private readonly KcpConversation Conversation;
    public readonly IPEndPoint RemoteEndPoint;

    public string DebugFile = "";
    public bool IsOnline = true;
    public StreamWriter? writer;

    public Connection(KcpConversation conversation, IPEndPoint remote)
    {
        Conversation = conversation;
        RemoteEndPoint = remote;
        CancelToken = new CancellationTokenSource();
        Start();
    }

    public long? ConversationID => Conversation.ConversationId;
    public SessionStateEnum State { get; set; } = SessionStateEnum.INACTIVE;
    public PlayerInstance? Player { get; set; }

    private async void Start()
    {
        Logger.Info($"New connection from {RemoteEndPoint}.");
        State = SessionStateEnum.WAITING_FOR_TOKEN;
        await ReceiveLoop();
    }

    public void Stop()
    {
        Player?.OnLogoutAsync();
        Listener.UnregisterConnection(this);
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
#pragma warning disable CS8600
            var typ = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == "DanhengCommon")!.GetTypes()
                .First(t => t.Name == $"{LogMap[opcode.ToString()]}"); //get the type using the packet name
            var descriptor =
                (MessageDescriptor)typ.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)!.GetValue(
                    null, null); // get the static property Descriptor
            var packet = descriptor!.Parser.ParseFrom(payload);
#pragma warning restore CS8600
            var formatter = JsonFormatter.Default;
            var asJson = formatter.Format(packet);
            var output = $"{sendOrRecv}: {LogMap[opcode.ToString()]}({opcode})\r\n{asJson}";
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
        catch
        {
            var output = $"{sendOrRecv}: {LogMap[opcode.ToString()]}({opcode})";
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

        writer ??= new StreamWriter(DebugFile, true);
        return writer;
    }

    private async Task ReceiveLoop()
    {
        while (!CancelToken.IsCancellationRequested)
        {
            // WaitToReceiveAsync call completes when there is at least one message is received or the transport is closed.
            var result = await Conversation.WaitToReceiveAsync(CancelToken.Token);
            if (result.TransportClosed)
            {
                Logger.Debug("Connection was closed");
                break;
            }

            if (result.BytesReceived > Listener.MAX_MSG_SIZE)
            {
                // The message is too large.
                Logger.Error("Packet too large");
                Conversation.SetTransportClosed();
                break;
            }

            var buffer = ArrayPool<byte>.Shared.Rent(result.BytesReceived);
            try
            {
                // TryReceive should not return false here, unless the transport is closed.
                // So we don't need to check for result.TransportClosed.
                if (!Conversation.TryReceive(buffer, out result))
                {
                    Logger.Error("Failed to receive packet");
                    break;
                }

                await ProcessMessageAsync(buffer.AsMemory(0, result.BytesReceived));
            }
            catch (Exception ex)
            {
                Logger.Error("Packet parse error", ex);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        Stop();
    }

    // DO THE PROCESSING OF THE GAME PACKET
    private async Task ProcessMessageAsync(Memory<byte> data)
    {
        var gamePacket = data.ToArray();

        await using MemoryStream? ms = new(gamePacket);
        using BinaryReader? br = new(ms);

        // Handle
        try
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                // Length
                if (br.BaseStream.Length - br.BaseStream.Position < 12) return;
                // Packet sanity check
                var Magic1 = br.ReadUInt32BE();
                if (Magic1 != 0x9D74C714)
                {
                    Logger.Error($"Bad Data Package Received: got 0x{Magic1:X}, expect 0x9D74C714");
                    return; // Bad packet
                }

                // Data
                var opcode = br.ReadUInt16BE();
                var headerLength = br.ReadUInt16BE();
                var payloadLength = br.ReadUInt32BE();
                var header = br.ReadBytes(headerLength);
                var payload = br.ReadBytes((int)payloadLength);
                LogPacket("Recv", opcode, payload);
                await HandlePacket(opcode, header, payload);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message, e);
        }
        finally
        {
            await ms.DisposeAsync();
        }
    }

    private async Task<bool> HandlePacket(ushort opcode, byte[] header, byte[] payload)
    {
        // Find the Handler for this opcode
        var handler = HandlerManager.GetHandler(opcode);
        if (handler != null)
        {
            // Handle
            // Make sure session is ready for packets
            var state = State;
            switch (opcode)
            {
                case CmdIds.PlayerGetTokenCsReq:
                {
                    if (state != SessionStateEnum.WAITING_FOR_TOKEN) return true;
                    goto default;
                }
                case CmdIds.PlayerLoginCsReq:
                {
                    if (state != SessionStateEnum.WAITING_FOR_LOGIN) return true;
                    goto default;
                }
                default:
                    break;
            }

            await handler.OnHandle(this, header, payload);
            return true;
        }

        return false;
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
        if (BANNED_PACKETS.Contains(packet.CmdId)) return;
        LogPacket("Send", packet.CmdId, packet.Data);
        // Header
        var packetBytes = packet.BuildPacket();

        try
        {
            _ = await Conversation.SendAsync(packetBytes, CancelToken.Token);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SendPacket(int cmdId)
    {
        await SendPacket(new BasePacket((ushort)cmdId));
    }
}