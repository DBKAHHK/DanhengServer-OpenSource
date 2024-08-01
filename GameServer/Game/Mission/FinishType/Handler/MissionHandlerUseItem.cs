using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.UseItem)]
public class MissionHandlerUseItem : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (arg is ItemData item)
            if (info.ParamInt1 == item.ItemId)
                await player.MissionManager!.AddMissionProgress(info.ID, 1);

        if (player.MissionManager!.GetMissionProgress(info.ID) >= info.Progress)
            await player.MissionManager!.FinishSubMission(info.ID);
    }
}