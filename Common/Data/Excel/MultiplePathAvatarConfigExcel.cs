namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MultiplePathAvatarConfig.json")]
public class MultiplePathAvatarConfigExcel : ExcelResource
{
    public int AvatarID { get; set; }
    public int BaseAvatarID { get; set; }

    public override int GetId()
    {
        return AvatarID;
    }

    public override void Loaded()
    {
        GameData.MultiplePathAvatarConfigData.Add(AvatarID, this);
    }
}