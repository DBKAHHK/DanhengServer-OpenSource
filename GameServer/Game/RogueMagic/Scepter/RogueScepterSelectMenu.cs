using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.RogueMagic.Scepter;

public class RogueScepterSelectMenu(BaseRogueInstance rogue)
{
    public List<RogueMagicScepterExcel> Scepters { get; set; } = [];
    public int RollMaxCount { get; set; } = rogue.BaseRerollCount;
    public int RollCount { get; set; }
    public int RollFreeCount { get; set; } = rogue.BaseRerollFreeCount;
    public int RollCost { get; set; } = rogue.CurRerollCost;
    public int QueueAppend { get; set; } = 3;
    public List<RogueMagicScepterExcel> ScepterPool { get; set; } = [];

    public void RollScepter(List<RogueMagicScepterExcel> scepters, int count = 3)
    {
        ScepterPool.Clear();
        ScepterPool.AddRange(scepters);

        var list = new RandomList<RogueMagicScepterExcel>();

        foreach (var magicScepterExcel in scepters)
            list.Add(magicScepterExcel, 1);
        var result = new List<RogueMagicScepterExcel>();

        for (var i = 0; i < count; i++)
        {
            var scepterExcel = list.GetRandom();
            if (scepterExcel != null)
            {
                result.Add(scepterExcel);
                list.Remove(scepterExcel);
            }

            if (list.GetCount() == 0) break; // No more scepter to roll
        }

        Scepters = result;
    }

    public async ValueTask RerollScepter()
    {
        if (RollFreeCount > 0)
        {
            RollFreeCount--; // Free reroll
        }
        else
        {
            if (RollMaxCount - RollCount <= 0) return;
            RollCount++; // Paid reroll
            await rogue.CostMoney(RollCost);
        }

        RollScepter(ScepterPool.Clone().ToList());
    }

    public RogueActionInstance GetActionInstance()
    {
        rogue.CurActionQueuePosition += QueueAppend;
        return new RogueActionInstance
        {
            QueuePosition = rogue.CurActionQueuePosition,
            RogueScepterSelectMenu = this
        };
    }


    public RogueMagicScepterSelectInfo ToProto()
    {
        return new RogueMagicScepterSelectInfo
        {
            SelectScepters = { Scepters.Select(x => new RogueMagicScepter
            {
                ScepterId = (uint)x.ScepterID,
                Level = (uint)x.ScepterLevel
            }) }
        };
    }
}