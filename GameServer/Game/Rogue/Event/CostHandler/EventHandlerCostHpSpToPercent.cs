using EggLink.DanhengServer.Enums.Rogue;
using EggLink.DanhengServer.Server.Packet.Send.Lineup;

namespace EggLink.DanhengServer.Game.Rogue.Event.CostHandler;

[RogueEvent(costType: DialogueEventCostTypeEnum.CostHpSpToPercent)]
public class EventHandlerCostHpSpToPercent : RogueEventCostHandler
{
    public override async ValueTask Handle(BaseRogueInstance rogue, RogueEventInstance? eventInstance,
        List<int> paramList)
    {
        if (rogue.CurLineup!.CostNowPercentHp(1 - paramList[0] / 100f))
            await rogue.Player!.SendPacket(new PacketSyncLineupNotify(rogue.CurLineup!));
    }
}