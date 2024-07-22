using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Battle;

[Opcode(CmdIds.PVEBattleResultCsReq)]
public class HandlerPVEBattleResultCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = PVEBattleResultCsReq.Parser.ParseFrom(data);
        await connection.Player!.BattleManager!.EndBattle(req);
    }
}