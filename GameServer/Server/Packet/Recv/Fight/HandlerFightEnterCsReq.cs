using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Fight;

[Opcode(CmdIds.FightEnterCsReq)]
public class HandlerFightEnterCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FightEnterCsReq.Parser.ParseFrom(data);
        var room = MatchThreeService.GameInstances.Find(x => x.Members.FindIndex(j => j.Player.Uid == req.Uid) != -1);  // find room by player uid
        if (room == null) return;

        var member = room.Members.Find(x => x.Player.Uid == req.Uid)!;
        connection.GameInstance = room;
        connection.Uid = member.Player.Uid;

        await connection.SendPacket(new PacketFightEnterScRsp(connection));
        connection.State = SessionStateEnum.ACTIVE;

        if (ConfigManager.Config.GameServer.UsePacketEncryption)
        {
            connection.XorKey = Crypto.GenerateXorKey(connection.ClientSecretKeySeed);
        }
    }
}