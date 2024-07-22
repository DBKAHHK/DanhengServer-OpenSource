using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Mission.FinishType;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.ItemNum)]
public class MissionHandlerItemNum : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        var count = 0;
        var item = player.InventoryManager?.GetItem(info.ParamInt1);
        if (item != null) count += item.Count;

        if (count == info.Progress)
        {
            await player.MissionManager!.FinishSubMission(info.ID);
        }
        else
        {
            if (player.MissionManager?.GetMissionProgress(info.ID) != count)
                await player.MissionManager!.SetMissionProgress(info.ID, count);
        }
    }
}