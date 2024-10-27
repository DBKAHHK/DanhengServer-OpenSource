using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using SqlSugar;

namespace EggLink.DanhengServer.Database.TrainParty;

[SugarTable("train_party_data")]
public class GameTrainPartyData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true, ColumnDataType = "TEXT")]
    public Dictionary<int, GameTrainPartyAreaInfo> Areas { get; set; } = [];
}

public class GameTrainPartyAreaInfo
{
    public int AreaId { get; set; }
    public List<int> StepList { get; set; } = [];

    public TrainPartyArea ToProto()
    {
        var info = new TrainPartyArea
        {
            AreaId = (uint)AreaId,
            StepIdList = { StepList.Select(x => (uint)x) },
            AreaStepInfo = new AreaStepInfo(),
        };

        foreach (var step in StepList)
        {
            GameData.TrainPartyStepConfigData.TryGetValue(step, out var stepExcel);
            if (stepExcel == null) continue;

            info.StaticPropIdList.AddRange(stepExcel.StaticPropIDList.Select(x => (uint)x));
        }

        return info;
    }

    public int GetCoinCost()
    {
        var cost = 0;

        foreach (var step in StepList)
        {
            GameData.TrainPartyStepConfigData.TryGetValue(step, out var stepExcel);
            if (stepExcel == null) continue;

            cost += stepExcel.CoinCost;
        }

        return cost;
    }
}