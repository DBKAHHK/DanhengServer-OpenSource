using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Friend;

namespace EggLink.DanhengServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.SearchPlayerCsReq)]
public class HandlerSearchPlayerCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SearchPlayerCsReq.Parser.ParseFrom(data);
        var playerList = new List<PlayerData>();

        foreach (var uid in req.UidList)
        {
            var player = PlayerData.GetPlayerByUid(uid);
            if (player != null) playerList.Add(player);
        }

        if (playerList.Count == 0)
            await connection.SendPacket(new PacketSearchPlayerScRsp());
        else
            await connection.SendPacket(new PacketSearchPlayerScRsp(playerList));
    }
}