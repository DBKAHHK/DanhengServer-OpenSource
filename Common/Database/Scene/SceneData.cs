using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.Proto;
using Google.Protobuf;
using SqlSugar;

namespace EggLink.DanhengServer.Database.Scene;

[SugarTable("Scene")]
public class SceneData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<int, List<ScenePropData>>> ScenePropData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<GroupId, ScenePropData>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, List<int>> UnlockSectionIdList { get; set; } = []; // Dictionary<FloorId, List<SectionId>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<int, string>> CustomSaveData { get; set; } =
        []; // Dictionary<EntryId, Dictionary<GroupId, SaveData>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<string, int>> FloorSavedData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<SaveDataKey, SaveDataValue>>

    [SugarColumn(IsJson = true)]
    public Dictionary<int, Dictionary<int, Dictionary<int, ScenePropTimelineData>>> PropTimelineData { get; set; } =
        []; // Dictionary<FloorId, Dictionary<GroupId, Dictionary<PropId, ScenePropTimelineData>>
}

public class ScenePropData
{
    public int PropId { get; set; }
    public PropStateEnum State { get; set; }
}

public class ScenePropTimelineData
{
    public uint UintValue { get; set; }
    public bool BoolValue { get; set; }
    public string ByteValue { get; set; } = "";  // Base64

    public PropTimelineInfo ToProto()
    {
        return new PropTimelineInfo
        {
            TimelineIntValue = UintValue,
            TimelineBoolValue = BoolValue,
            TimelineByteValue = ByteString.FromBase64(ByteValue)
        };
    }
}