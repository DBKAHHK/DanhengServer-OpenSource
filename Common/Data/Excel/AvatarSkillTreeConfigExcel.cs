namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("AvatarSkillTreeConfig.json,AvatarSkillTreeConfigLD.json", true)]
public class AvatarSkillTreeConfigExcel : ExcelResource
{
    public int PointID { get; set; }
    public int Level { get; set; }
    public int AvatarID { get; set; }
    public int EnhancedID { get; set; }
    public bool DefaultUnlock { get; set; }
    public int MaxLevel { get; set; }

    public override int GetId()
    {
        return PointID * 100 + Level;
    }

    public override void AfterAllDone()
    {
        if (EnhancedID == 1) return;
        GameData.AvatarConfigData.TryGetValue(AvatarID, out var excel);
        if (excel != null && DefaultUnlock && excel.DefaultSkillTree.All(x => x.PointID != PointID))
            excel.DefaultSkillTree.Add(this);
        if (excel != null && excel.SkillTree.All(x => x.PointID != PointID)) excel.SkillTree.Add(this);
        GameData.AvatarSkillTreeConfigData.TryAdd(GetId(), this);
    }
}