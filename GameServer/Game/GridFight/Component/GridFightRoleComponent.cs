using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightRoleComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public GridFightAvatarInfoPb Data { get; set; } = new();

    public async ValueTask<List<BaseGridFightSyncData>> AddAvatar(uint roleId, uint tier = 1, bool sendPacket = true,
        bool checkMerge = true, GridFightSrc src = GridFightSrc.KGridFightSrcBuyGoods)
    {
        var pos = 1u;
        // get first empty pos
        var usedPos = Data.Roles.Select(x => x.Pos).ToHashSet();
        for (var i = 1u; i <= 20u; i++)
        {
            if (usedPos.Contains(i)) continue;
            pos = i;
            break;
        }

        var info = new GridFightRoleInfoPb
        {
            RoleId = roleId,
            UniqueId = ++Data.CurUniqueId,
            Tier = tier,
            Pos = pos
        };

        Data.Roles.Add(info);

        List<BaseGridFightSyncData> syncs = [new GridFightRoleAddSyncData(src, info)];

        if (checkMerge)
        {
            var mergeSyncs = await CheckIfMergeRole();
            syncs.AddRange(mergeSyncs);
        }

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

    public async ValueTask<List<BaseGridFightSyncData>> CheckIfMergeRole(bool sendPacket = false)
    {
        List<BaseGridFightSyncData> syncs = [];
        bool hasMerged;

        do
        {
            hasMerged = false;

            // group roles by RoleId and Tier, then filter groups with 3 or more roles
            var mergeCandidates = Data.Roles
                .GroupBy(r => new { r.RoleId, r.Tier })
                .Where(g => g.Count() >= 3)
                .Where(g =>
                {
                    // check if next tier exists
                    var nextTierKey = g.Key.RoleId << 4 | (g.Key.Tier + 1);
                    return GameData.GridFightRoleStarData.ContainsKey(nextTierKey);
                })
                .OrderBy(g => g.Key.RoleId)
                .ThenBy(g => g.Key.Tier)
                .FirstOrDefault(); // process one group at a time to handle continuous merging

            if (mergeCandidates != null)
            {
                var roleId = mergeCandidates.Key.RoleId;
                var currentTier = mergeCandidates.Key.Tier;
                var toMerge = mergeCandidates.Take(3).ToList();

                // remove merged roles
                foreach (var role in toMerge)
                {
                    Data.Roles.Remove(role);
                    syncs.Add(new GridFightRoleRemoveSyncData(GridFightSrc.KGridFightSrcMergeRole, role));
                }

                // add new merged role with tier + 1
                var addSyncs = await AddAvatar(roleId, currentTier + 1, false, false, GridFightSrc.KGridFightSrcMergeRole);
                syncs.AddRange(addSyncs);

                hasMerged = true;
            }
        } while (hasMerged); // continue until no more merges are possible

        if (sendPacket && syncs.Count > 0)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

    public async ValueTask<List<BaseGridFightSyncData>> SellAvatar(uint uniqueId, bool sendPacket = true)
    {
        var role = Data.Roles.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (role == null)
        {
            return [];
        }

        Data.Roles.Remove(role);

        var tier = role.Tier;
        var rarity = GameData.GridFightRoleBasicInfoData[role.RoleId].Rarity;

        var sellPrice = GameData.GridFightShopPriceData.GetValueOrDefault(rarity)
            ?.SellGoldList[(int)(tier - 1)] ?? 1;

        var basicComp = Inst.GetComponent<GridFightBasicComponent>();
        await basicComp.UpdateGoldNum((int)sellPrice, false, GridFightSrc.KGridFightSrcRecycleRole);

        List<BaseGridFightSyncData> syncs =
        [
            new GridFightRoleRemoveSyncData(GridFightSrc.KGridFightSrcRecycleRole, role),
            new GridFightGoldSyncData(GridFightSrc.KGridFightSrcRecycleRole, basicComp.Data)
        ];

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

    public List<BaseAvatarInfo> GetForegroundAvatarInfos(uint maxAvatarNum)
    {
        var foreground = Data.Roles.Where(x => x.Pos <= 4).OrderBy(x => x.Pos).ToList();
        List<BaseAvatarInfo> res = [];

        foreach (var role in foreground)
        {
            var excel = GameData.GridFightRoleBasicInfoData[role.RoleId];
            // get formal or special
            var formal = Inst.Player.AvatarManager!.GetFormalAvatar((int)excel.AvatarID);
            if (formal != null)
            {
                res.Add(formal);
            }
            else
            {
                var special = Inst.Player.AvatarManager.GetTrialAvatar((int)excel.SpecialAvatarID);
                if (special != null)
                {
                    res.Add(special);
                }
            }
        }

        return res;
    }

    public List<BaseAvatarInfo> GetBackgroundAvatarInfos(uint maxAvatarNum)
    {
        var foreground = Data.Roles.Where(x => x.Pos <= maxAvatarNum && x.Pos > 4).OrderBy(x => x.Pos).ToList();
        List<BaseAvatarInfo> res = [];

        foreach (var role in foreground)
        {
            var excel = GameData.GridFightRoleBasicInfoData[role.RoleId];
            // get formal or special
            var formal = Inst.Player.AvatarManager!.GetFormalAvatar((int)excel.AvatarID);
            if (formal != null)
            {
                res.Add(formal);
            }
            else
            {
                var special = Inst.Player.AvatarManager.GetTrialAvatar((int)excel.SpecialAvatarID);
                if (special != null)
                {
                    res.Add(special);
                }
            }
        }

        return res;
    }

    public async ValueTask UpdatePos(List<GridFightPosInfo> posList)
    {
        List<BaseGridFightSyncData> syncs = [];
        foreach (var pos in posList)
        {
            var role = Data.Roles.FirstOrDefault(x => x.UniqueId == pos.UniqueId);
            if (role != null)
            {
                role.Pos = pos.Pos;
                syncs.Add(new GridFightRoleUpdateSyncData(GridFightSrc.KGridFightSrcCopyRole, role));
            }
        }

        if (syncs.Count > 0)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }
    }

    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridAvatarGameInfo = new GridFightGameAvatarInfo
            {
                GridGameAvatarList = { Data.Roles.Select(x => x.ToProto()) }
            }
        };
    }
}

public static class GridFightRoleInfoPbExtensions
{
    public static GridGameAvatarInfo ToProto(this GridFightRoleInfoPb info)
    {
        return new GridGameAvatarInfo
        {
            Id = info.RoleId,
            UniqueId = info.UniqueId,
            Tier = info.Tier,
            Pos = info.Pos
        };
    }

    public static BattleGridFightRoleInfo ToBattleInfo(this GridFightRoleInfoPb info)
    {
        return new BattleGridFightRoleInfo
        {
            RoleBasicId = info.RoleId,
            UniqueId = info.UniqueId,
            Tier = info.Tier,
            Pos = info.Pos,
            AvatarId = GameData.GridFightRoleBasicInfoData[info.RoleId].AvatarID
        };
    }
}