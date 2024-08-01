using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Battle;

[Opcode(CmdIds.PVEBattleResultCsReq)]
public class HandlerPVEBattleResultCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = PVEBattleResultCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        player.BattleManager?.EndBattle(req);
        if (player.ActivityManager!.TrialActivityInstance != null && req.EndStatus == BattleEndStatus.BattleEndWin)
            await player.ActivityManager.TrialActivityInstance.EndActivity(TrialActivityStatus.Finish);
    }
}