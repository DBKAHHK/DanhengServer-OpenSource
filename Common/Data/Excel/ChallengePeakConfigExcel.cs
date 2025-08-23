using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakConfig.json")]
public class ChallengePeakConfigExcel : ExcelResource
{
    public int ID { get; set; }
    public int MazeGroupID { get; set; }
    public int MapEntranceID { get; set; }
    public List<int> TagList { get; set; } = [];
    public List<int> HPProgressValueList { get; set; } = [];
    public List<int> ProgressValueList { get; set; } = [];
    public List<int> ConfigIDList { get; set; } = [];
    public List<int> EventIDList { get; set; } = [];
    public List<int> NpcMonsterIDList { get; set; } = [];
    public List<int> NormalTargetList { get; set; } = [];

    [JsonIgnore]
    public Dictionary<int, List<ChallengeConfigExcel.ChallengeMonsterInfo>> ChallengeMonsters { get; } = [];

    [JsonIgnore] public ChallengePeakBossConfigExcel? BossExcel { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.ChallengePeakConfigData.TryAdd(ID, this);

        ChallengeMonsters.Add(MazeGroupID, []);
        for (var i = 0; i < ConfigIDList.Count; i++)
            ChallengeMonsters[MazeGroupID]
                .Add(new ChallengeConfigExcel.ChallengeMonsterInfo(ConfigIDList[i], NpcMonsterIDList[i],
                    EventIDList[i]));
    }
}