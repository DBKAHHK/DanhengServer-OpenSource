using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.GridFight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightAvatarComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public List<GridFightAvatarInfo> Avatars { get; set; } = [];
    private uint _curUniqueId;

    public async ValueTask AddAvatar(uint roleId, uint tier = 1)
    {
        var info = new GridFightAvatarInfo(roleId, ++_curUniqueId, tier);
        Avatars.Add(info);

        // TODO sync
        await ValueTask.CompletedTask;
    }

    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridAvatarGameInfo = new GridFightGameAvatarInfo
            {
                GridGameAvatarList = { Avatars.Select(x => x.ToProto()) }
            }
        };
    }
}

public class GridFightAvatarInfo(uint roleId, uint uniqueId, uint tier = 1)
{
    public GridFightRoleBasicInfoExcel RoleInfo { get; set; } = GameData.GridFightRoleBasicInfoData[roleId];
    public GridFightRolePositionEnum Pos { get; set; } = GridFightRolePositionEnum.Foreground;
    public uint Tier { get; set; } = tier;
    public uint UniqueId { get; set; } = uniqueId;

    public GridGameAvatarInfo ToProto()
    {
        return new GridGameAvatarInfo
        {
            Id = RoleInfo.ID,
            Pos = (uint)Pos,
            Tier = Tier,
            UniqueId = UniqueId
        };
    }
}