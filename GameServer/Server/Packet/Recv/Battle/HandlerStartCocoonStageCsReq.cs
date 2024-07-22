using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Battle;

[Opcode(CmdIds.StartCocoonStageCsReq)]
public class HandlerStartCocoonStageCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = StartCocoonStageCsReq.Parser.ParseFrom(data);
        await connection.Player!.BattleManager!.StartCocoonStage((int)req.CocoonId, (int)req.Wave, (int)req.WorldLevel);
    }
}