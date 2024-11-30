using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyCreateCsReq)]
public class HandlerLobbyCreateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LobbyCreateCsReq.Parser.ParseFrom(data);

        var player = connection.Player!;
        var room = await MatchThreeService.CreateRoom(player, (int)req.LobbyExtraInfo.GameBirdInfo.BirdId);

        await connection.SendPacket(new PacketLobbyCreateScRsp(room));
    }
}