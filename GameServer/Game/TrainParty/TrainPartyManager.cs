using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.TrainParty;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Proto;
using System.Numerics;

namespace EggLink.DanhengServer.GameServer.Game.TrainParty;

public class TrainPartyManager : BasePlayerManager
{
    public GameTrainPartyData TrainPartyData { get; }

    public TrainPartyManager(PlayerInstance player) : base(player)
    {
        TrainPartyData =
            DatabaseHelper.Instance!.GetInstanceOrCreateNew<GameTrainPartyData>(player.Uid);

        foreach (var excel in GameData.TrainPartyAreaConfigData.Where(excel => !TrainPartyData.Areas.ContainsKey(excel.Key)))
        {
            TrainPartyData.Areas[excel.Key] = new GameTrainPartyAreaInfo
            {
                AreaId = excel.Key,
                StepList = [excel.Value.FirstStep]
            };
        }
    }

    public TrainPartyData ToProto()
    {
        var proto = new TrainPartyData
        {
            TrainPartyInfo = ToPartyInfo(),
            PassengerInfo = ToPassengerInfo()
        };

        return proto;
    }

    public TrainPartyInfo ToPartyInfo()
    {
        var proto = new TrainPartyInfo
        {
            AreaList = { TrainPartyData.Areas.Values.Select(x => x.ToProto()) },
            CoinCost = (uint)TrainPartyData.Areas.Values.Sum(x => x.GetCoinCost()),
            DynamicIdList = { GameData.TrainPartyDynamicConfigData.Select(x => (uint)x.Key) }
        };

        return proto;
    }

    public TrainPartyPassengerInfo ToPassengerInfo()
    {
        return new TrainPartyPassengerInfo
        {
            PassengerInfoList =
            {
                GameData.TrainPartyPassengerConfigData.Select(x => new TrainPartyPassenger
                {
                    PassengerId = (uint)x.Key
                })
            }
        };
    }
}