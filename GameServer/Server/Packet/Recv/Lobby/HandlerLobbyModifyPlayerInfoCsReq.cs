using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyModifyPlayerInfoCsReq)]
public class HandlerLobbyModifyPlayerInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = LobbyModifyPlayerInfoCsReq.Parser.ParseFrom(data);
        var room = connection.Player!.MatchThreeManager!.RoomInstance;

        if (room == null)
        {
            await connection.SendPacket(CmdIds.LobbyModifyPlayerInfoScRsp);
            return;
        }

        await room.ModifyPlayerInfo(connection.Player!, req.LobbyExtraInfo, req.Type);
        await connection.SendPacket(CmdIds.LobbyModifyPlayerInfoScRsp);
    }
}