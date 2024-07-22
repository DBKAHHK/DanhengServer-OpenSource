using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.PlayBackGroundMusicCsReq)]
public class HandlerPlayBackGroundMusicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = PlayBackGroundMusicCsReq.Parser.ParseFrom(data);

        connection.Player!.Data.CurrentBgm = (int)req.PlayMusicId;

        await connection.SendPacket(new PacketPlayBackGroundMusicScRsp(req.PlayMusicId));
    }
}