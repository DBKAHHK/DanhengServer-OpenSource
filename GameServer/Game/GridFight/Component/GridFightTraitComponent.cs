using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightTraitComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public GridFightTraitInfoPb Data { get; set; } = new();

    public void CheckTrait()
    {
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();

        Dictionary<uint, uint> traitCount = [];

        foreach (var traitId in GameData.GridFightTraitBasicInfoData.Keys)
        {
            traitCount[traitId] = 0;  // initialize
        }

        foreach (var role in roleComp.Data.Roles.Where(x => x.Pos <= GridFightRoleComponent.PrepareAreaPos))
        {
            if (!GameData.GridFightRoleBasicInfoData.TryGetValue(role.RoleId, out var excel)) continue;

            foreach (var traitId in excel.TraitList)
            {
                traitCount[traitId]++;  // increase count
            }
        }

        foreach (var (traitId, count) in traitCount)
        {
            var traitExcel = GameData.GridFightTraitBasicInfoData.GetValueOrDefault(traitId);
            if (traitExcel == null) continue;

            var traitLayers = GameData.GridFightTraitLayerData.GetValueOrDefault(traitId);
            if (traitLayers == null) continue;

            var layers = traitLayers.Where(x => x.Key <= count).ToList();
            var layer = layers.Count > 0 ? layers.Max(x => x.Value.Layer) : 0;

            var existingTrait = Data.Traits.FirstOrDefault(x => x.TraitId == traitId);

            if (existingTrait != null)
            {
                existingTrait.TraitLayer = layer;
            }
            else
            {
                if (layer == 0) continue;  // do not add if no layer

                Data.Traits.Add(new GridFightGameTraitPb
                {
                    TraitId = traitId,
                    TraitLayer = layer,
                    Effects =
                    {
                        traitExcel.TraitEffectList.Select(x => new GridFightGameTraitEffectPb
                        {
                            EffectId = x
                        })
                    }
                });
            }
        }
    }

    public override GridFightGameInfo ToProto()
    {
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();

        return new GridFightGameInfo
        {
            GridTraitGameInfo = new GridFightGameTraitInfo
            {
                GridFightTraitInfo = { Data.Traits.Select(x => x.ToProto(roleComp)) }
            }
        };
    }
}

public static class GridFightTraitInfoPbExtensions
{
    public static GridGameTraitInfo ToProto(this GridFightGameTraitPb info, GridFightRoleComponent roleComp)
    {
        var traitRoles = roleComp.Data.Roles.Where(x =>
            GameData.GridFightRoleBasicInfoData.GetValueOrDefault(x.RoleId)?.TraitList.Contains(info.TraitId) == true).ToList();

        return new GridGameTraitInfo
        {
            TraitId = info.TraitId,
            TraitEffectLayer = info.TraitLayer,
            GridFightTraitMemberUniqueIdList = { traitRoles.Select(x => x.UniqueId) },
            TraitEffectList = { info.Effects.Select(x => x.ToProto())}
        };
    }

    public static BattleGridFightTraitInfo ToBattleInfo(this GridFightGameTraitPb info, GridFightRoleComponent roleComp)
    {
        var traitRoles = roleComp.Data.Roles.Where(x =>
            x.Pos <= GridFightRoleComponent.PrepareAreaPos && GameData.GridFightRoleBasicInfoData
                .GetValueOrDefault(x.RoleId)?.TraitList.Contains(info.TraitId) == true).ToList();

        var phainonRole = roleComp.Data.Roles.FirstOrDefault(x =>
            x.Pos <= GridFightRoleComponent.PrepareAreaPos && x.RoleId == 1408);  // hardcode

        var res = new BattleGridFightTraitInfo
        {
            TraitId = info.TraitId,
            TraitEffectLayer = info.TraitLayer,
            MemberList = { traitRoles.Select(x => new GridFightTraitMember
            {
                GridUpdateSrc = GridFightTraitSrc.KGridFightTraitSrcRole,
                MemberRoleId = x.RoleId,
                MemberRoleUniqueId = x.UniqueId,
                MemberType = GridFightTraitMemberType.KGridFightTraitMemberRole
            }) },
            TraitEffectList = { info.Effects.Select(x => x.ToBattleInfo())}
        };

        if (phainonRole != null && traitRoles.All(x => x.UniqueId != phainonRole.UniqueId))
        {
            res.MemberList.Add(new GridFightTraitMember
            {
                GridUpdateSrc = GridFightTraitSrc.KGridFightTraitSrcDummy,
                MemberRoleId = phainonRole.RoleId,
                MemberRoleUniqueId = phainonRole.UniqueId,
                MemberType = GridFightTraitMemberType.KGridFightTraitMemberRole
            });
        }

        return res;
    }

    public static GridFightTraitEffectInfo ToProto(this GridFightGameTraitEffectPb info)
    {
        return new GridFightTraitEffectInfo
        {
            EffectId = info.EffectId,
            EffectDnaNum = info.Param
        };
    }

    public static BattleGridFightTraitEffectInfo ToBattleInfo(this GridFightGameTraitEffectPb info)
    {
        return new BattleGridFightTraitEffectInfo
        {
            EffectId = info.EffectId
        };
    }
}