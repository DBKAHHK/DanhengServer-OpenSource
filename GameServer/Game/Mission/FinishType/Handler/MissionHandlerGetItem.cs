using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.GetItem)]
public class MissionHandlerGetItem : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (arg != null && arg is ItemData item)
            if (item.ItemId == info.ParamInt1)
                await player.MissionManager!.FinishSubMission(info.ID);
    }
}