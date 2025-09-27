namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("GridFightEquipment.json")]
public class GridFightEquipmentExcel : ExcelResource
{
    public uint ID { get; set; }
    public string EquipCategory { get; set; } = ""; // TODO use enum

    public override int GetId()
    {
        return (int)ID;
    }

    public override void Loaded()
    {
        GameData.GridFightEquipmentData.TryAdd(ID, this);
    }
}