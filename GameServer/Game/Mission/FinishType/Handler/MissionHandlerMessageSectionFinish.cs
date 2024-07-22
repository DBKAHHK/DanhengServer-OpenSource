using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.MessageSectionFinish)]
public class MissionHandlerMessageSectionFinish : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        var data = player.MessageManager!.GetMessageSectionData(info.ParamInt1);
        if (data == null) return;

        if (data.Status == MessageSectionStatus.MessageSectionFinish)
            await player.MissionManager!.FinishSubMission(info.ID);
    }
}