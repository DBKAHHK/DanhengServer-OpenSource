using System.Collections.Generic;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("GridFightConsumables.json")]
public class GridFightConsumablesExcel : ExcelResource
{
    public uint ID { get; set; }

    public override int GetId()
    {
        return (int)ID;
    }

    public override void Loaded()
    {
        GameData.GridFightConsumablesData.TryAdd(ID, this);
    }
}