using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using SqlSugar;
using LineupInfo = EggLink.DanhengServer.Database.Lineup.LineupInfo;

namespace EggLink.DanhengServer.Database.Avatar;

[SugarTable("Avatar")]
public class AvatarData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true)] public List<AvatarInfo> Avatars { get; set; } = [];
}

public class AvatarInfo
{
    public int CurAvatarId { get; set; }
    public int BaseAvatarId { get; set; }
    [JsonIgnore] public int SpecialBaseAvatarId { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Promotion { get; set; }
    public int Rewards { get; set; }
    public long Timestamp { get; set; }
    public int CurrentHp { get; set; } = 10000;
    public int CurrentSp { get; set; }
    public int ExtraLineupHp { get; set; } = 10000;
    public int ExtraLineupSp { get; set; }
    public bool IsMarked { get; set; } = false;
    public Dictionary<int, MultiPathData> PathInfo { get; set; } = [];
    [JsonIgnore] public int InternalEntityId { get; set; }

    public AvatarInfo()
    {
        // For db only
    }

    public AvatarInfo(int baseAvatarId, int? curAvatarId = null)
    {
        BaseAvatarId = baseAvatarId;
        CurAvatarId = curAvatarId ?? baseAvatarId;
        PathInfo.Add(CurAvatarId, new()); // Security add cur pathinfo
    }

    public void SetEntityId(int uid, int entityId, int worldLevel = 0)
    {
        if (SpecialBaseAvatarId > 0)
        {
            // set in SpecialAvatarExcel
            GameData.SpecialAvatarData.TryGetValue(SpecialBaseAvatarId * 10 + worldLevel,
                out var specialAvatar);
            if (specialAvatar != null)
            {
                specialAvatar.EntityId[uid] = entityId;
            }
        }

        InternalEntityId = entityId;
    }

    public bool HasTakenReward(int promotion)
    {
        return (Rewards & (1 << promotion)) != 0;
    }

    public void TakeReward(int promotion)
    {
        Rewards |= 1 << promotion;
    }

    public int GetCurHp(bool isExtraLineup)
    {
        return isExtraLineup ? ExtraLineupHp : CurrentHp;
    }

    public int GetCurSp(bool isExtraLineup)
    {
        return isExtraLineup ? ExtraLineupSp : CurrentSp;
    }

    public int GetSpecialAvatarId()
    {
        return SpecialBaseAvatarId > 0 ? SpecialBaseAvatarId : CurAvatarId;
    }

    public int GetUniqueAvatarId()
    {
        return SpecialBaseAvatarId > 0 ? SpecialBaseAvatarId : BaseAvatarId;
    }

    public MultiPathData GetCurAvatarInfo()
    {
        return GetPathInfo(CurAvatarId)!;
    }

    public MultiPathData? GetPathInfo(int avatarId)
    {
        return PathInfo.TryGetValue(avatarId, out var pathInfo) ? pathInfo : null;
    }

    public AvatarConfigExcel GetCurAvatarConfig()
    {
        return GetAvatarConfig(CurAvatarId);
    }

    public AvatarConfigExcel GetAvatarConfig(int avatarId)
    {
        return GameData.AvatarConfigData.Values.FirstOrDefault(x =>
            x.AvatarID == avatarId)!;
    }

    public void SetCurHp(int value, bool isExtraLineup)
    {
        if (isExtraLineup)
            ExtraLineupHp = value;
        else
            CurrentHp = value;
    }

    public void SetCurSp(int value, bool isExtraLineup)
    {
        if (isExtraLineup)
            ExtraLineupSp = value;
        else
            CurrentSp = value;
    }

    public Proto.Avatar ToProto()
    {
        var proto = new Proto.Avatar
        {
            BaseAvatarId = (uint)GetUniqueAvatarId(),
            Level = (uint)Level,
            Exp = (uint)Exp,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurAvatarInfo().Rank,
            DressedSkinId = (uint)GetCurAvatarInfo().Skin,
            FirstMetTimeStamp = (ulong)Timestamp,
            IsMarked = IsMarked
        };

        foreach (var item in GetCurAvatarInfo().Relic)
            proto.EquipRelicList.Add(new EquipRelic
            {
                RelicUniqueId = (uint)item.Value,
                Type = (uint)item.Key
            });

        if (GetCurAvatarInfo().EquipId != 0) proto.EquipmentUniqueId = (uint)GetCurAvatarInfo().EquipId;

        foreach (var skill in GetCurAvatarInfo().SkillTree)
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        for (var i = 0; i < Promotion; i++)
            if (HasTakenReward(i))
                proto.HasTakenPromotionRewardList.Add((uint)i);

        return proto;
    }

    public SceneEntityInfo ToSceneEntityInfo(Position? pos, Position? rot, AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        return new SceneEntityInfo
        {
            EntityId = (uint)InternalEntityId,
            Motion = new MotionInfo
            {
                Pos = pos?.ToProto() ?? new Vector(),
                Rot = rot?.ToProto() ?? new Vector()
            },
            Actor = new SceneActorInfo
            {
                BaseAvatarId = (uint)BaseAvatarId,
                AvatarType = avatarType
            }
        };
    }

    public LineupAvatar ToLineupInfo(int slot, LineupInfo info, AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        return new LineupAvatar
        {
            Id = (uint)GetUniqueAvatarId(),
            Slot = (uint)slot,
            AvatarType = avatarType,
            Hp = info.IsExtraLineup() ? (uint)ExtraLineupHp : (uint)CurrentHp,
            SpBar = new SpBarInfo
            {
                CurSp = info.IsExtraLineup() ? (uint)ExtraLineupSp : (uint)CurrentSp,
                MaxSp = 10000
            }
        };
    }

    public BattleAvatar ToBattleProto(int worldLevel, LineupInfo lineup,
        InventoryData inventory, AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        var proto = new BattleAvatar
        {
            Id = (uint)GetSpecialAvatarId(),
            AvatarType = avatarType,
            Level = (uint)Level,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurAvatarInfo().Rank,
            Index = (uint)lineup.GetSlot(BaseAvatarId),
            Hp = (uint)GetCurHp(lineup.LineupType != 0),
            SpBar = new SpBarInfo
            {
                CurSp = (uint)GetCurSp(lineup.LineupType != 0),
                MaxSp = 10000
            },
            WorldLevel = (uint)worldLevel
        };

        foreach (var skill in GetCurAvatarInfo().SkillTree)
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        foreach (var relic in GetCurAvatarInfo().Relic)
        {
            var item = inventory.RelicItems?.Find(item => item.UniqueId == relic.Value);
            if (item != null)
            {
                var protoRelic = new BattleRelic
                {
                    Id = (uint)item.ItemId,
                    UniqueId = (uint)item.UniqueId,
                    Level = (uint)item.Level,
                    MainAffixId = (uint)item.MainAffix
                };

                if (item.SubAffixes.Count >= 1)
                    foreach (var subAffix in item.SubAffixes)
                        protoRelic.SubAffixList.Add(subAffix.ToProto());

                proto.RelicList.Add(protoRelic);
            }
        }

        if (GetCurAvatarInfo().EquipId != 0)
        {
            var item = inventory.EquipmentItems?.Find(item => item.UniqueId == GetCurAvatarInfo().EquipId);
            if (item != null)
                proto.EquipmentList.Add(new BattleEquipment
                {
                    Id = (uint)item.ItemId,
                    Level = (uint)item.Level,
                    Promotion = (uint)item.Promotion,
                    Rank = (uint)item.Rank
                });
        }
        else if (GetCurAvatarInfo().EquipData != null)
        {
            proto.EquipmentList.Add(new BattleEquipment
            {
                Id = (uint)GetCurAvatarInfo().EquipData!.ItemId,
                Level = (uint)GetCurAvatarInfo().EquipData!.Level,
                Promotion = (uint)GetCurAvatarInfo().EquipData!.Promotion,
                Rank = (uint)GetCurAvatarInfo().EquipData!.Rank
            });
        }

        return proto;
    }

    public List<MultiPathAvatarInfo> ToAvatarPathProto()
    {
        var res = new List<MultiPathAvatarInfo>();

        foreach (var path in PathInfo)
        {
            if (path.Key > 8000 && path.Key % 2 != CurAvatarId % 2) continue;

            var proto = new MultiPathAvatarInfo
            {
                AvatarId = (MultiPathAvatarType)path.Key,
                Rank = (uint)path.Value.Rank,
                PathEquipmentId = (uint)path.Value.EquipId,
                DressedSkinId = (uint)path.Value.Skin
            };

            foreach (var skillTree in path.Value.SkillTree)
                proto.MultiPathSkillTree.Add(new AvatarSkillTree
                {
                    PointId = (uint)skillTree.Key,
                    Level = (uint)skillTree.Value
                });

            foreach (var relic in path.Value.Relic)
                proto.EquipRelicList.Add(new EquipRelic
                {
                    Type = (uint)relic.Key,
                    RelicUniqueId = (uint)relic.Value
                });

            res.Add(proto);
        }

        return res;
    }

    public DisplayAvatarDetailInfo ToDetailProto(int playerUid, int pos)
    {
        var proto = new DisplayAvatarDetailInfo
        {
            AvatarId = (uint)CurAvatarId,
            Level = (uint)Level,
            Exp = (uint)Exp,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurAvatarInfo().Rank,
            Pos = (uint)pos
        };

        var inventory = DatabaseHelper.Instance!.GetInstance<InventoryData>(playerUid)!;
        foreach (var item in GetCurAvatarInfo().Relic)
        {
            var relic = inventory.RelicItems.Find(x => x.UniqueId == item.Value)!;
            proto.RelicList.Add(relic.ToDisplayRelicProto());
        }

        if (GetCurAvatarInfo().EquipId != 0)
        {
            var equip = inventory.EquipmentItems.Find(x => x.UniqueId == GetCurAvatarInfo().EquipId)!;
            proto.Equipment = equip.ToDisplayEquipmentProto();
        }

        foreach (var skillTree in GetCurAvatarInfo().SkillTree)
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skillTree.Key,
                Level = (uint)skillTree.Value
            });

        return proto;
    }
}

public class MultiPathData
{
    public int Rank { get; set; } = 0;
    public int EquipId { get; set; } = 0;
    public int Skin { get; set; } = 0;
    public Dictionary<int, int> SkillTree { get; set; } = [];
    public Dictionary<int, int> Relic { get; set; } = [];
    public ItemData? EquipData { get; set; } // for special avatar
}