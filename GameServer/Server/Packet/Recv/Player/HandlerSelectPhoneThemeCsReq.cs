using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.SelectPhoneThemeCsReq)]
public class HandlerSelectPhoneThemeCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SelectPhoneThemeCsReq.Parser.ParseFrom(data);

        connection.Player!.Data.PhoneTheme = (int)req.ThemeId;
        DatabaseHelper.Instance!.UpdateInstance(connection.Player!.Data);

        await connection.SendPacket(new PacketSelectPhoneThemeScRsp(req.ThemeId));
    }
}