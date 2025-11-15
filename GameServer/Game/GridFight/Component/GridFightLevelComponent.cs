using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.GridFight;
using EggLink.DanhengServer.GameServer.Game.GridFight.PendingAction;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightLevelComponent : BaseGridFightComponent
{
    #region Properties & Fields  // TODO : to proto field

    private uint _curChapterId = 1;
    private uint _curSectionId = 1;
    public Dictionary<uint, List<GridFightGameSectionInfo>> Sections { get; } = [];
    public GridFightGameSectionInfo CurrentSection => Sections[_curChapterId][(int)(_curSectionId - 1)];
    public List<GridFightRoleDamageSttInfo> RoleDamageSttInfos { get; } = [];
    public List<GridFightPortalBuffInfo> PortalBuffs { get; } = [];

    #endregion

    #region Constructors

    public GridFightLevelComponent(GridFightInstance inst) : base(inst)
    {
        // TODO: randomly select a base route id
        List<uint> chapterIds = [1100];
        List<GridFightCampExcel> campPool = GameData.GridFightCampData.Values.Where(x => x.BossBattleArea != 0).ToList();
        foreach (var chapterId in Enumerable.Range(1, 3))
        {
            var chapters = chapterIds.Count >= chapterId
                ? [GameData.GridFightStageRouteData[chapterIds[chapterId - 1]]]
                : GameData.GridFightStageRouteData.Values.Where(x => x.Any(j => j.Value.ChapterID == chapterId))
                    .ToList();
            if (chapters.Count == 0)
                continue;

            var select = chapters.RandomElement();

            var camp = campPool.RandomElement();  // cannot the same
            campPool.Remove(camp);

            // create section infos
            Sections[(uint)chapterId] = [.. select.Values.Select(x => new GridFightGameSectionInfo(x, camp))];
        }
    }

    #endregion

    #region Stt

    public async ValueTask<(Retcode, GridFightRoleDamageSttInfo?)> AddRoleDamageStt(uint roleId, double damage, bool sendPacket = true)
    {
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();

        var role = roleComp.Data.Roles.OrderBy(x => x.Pos).FirstOrDefault(x => x.RoleId == roleId);
        if (role == null)
            return (Retcode.RetGridFightRoleNotExist, null);

        var info = RoleDamageSttInfos.FirstOrDefault(x => x.RoleId == roleId && x.Tier == role.Tier);
        GridFightRoleDamageSttInfo res;
        if (info == null)
        {
            res = info = new GridFightRoleDamageSttInfo
            {
                RoleId = roleId,
                Tier = role.Tier,
                TotalDamage = damage,
                IsTrialAvatar = false,
                IsUpgrade = false
            };

            RoleDamageSttInfos.Add(info);
        }
        else
        {
            res = new GridFightRoleDamageSttInfo
            {
                RoleId = info.RoleId,
                IsTrialAvatar = info.IsTrialAvatar,
                IsUpgrade = info.IsUpgrade,
                Tier = info.Tier,
                TotalDamage = damage
            };

            info.TotalDamage += damage;
        }

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(new GridFightRoleDamageSttSyncData(GridFightSrc.KGridFightSrcBattleEnd, this)));
        }

        return (Retcode.RetSucc, res);
    }

    #endregion

    #region Actions

    public async ValueTask<List<BaseGridFightSyncData>> EnterNextSection(bool sendPacket = true, GridFightSrc src = GridFightSrc.KGridFightSrcBattleEnd)
    {
        // if last section of chapter
        if (_curSectionId >= Sections[_curChapterId].Count)
        {
            if (_curChapterId >= Sections.Count)
            {
                // end of game
                return [];
            }

            _curChapterId++;
            _curSectionId = 1;
        }
        else
        {
            _curSectionId++;
        }

        List<BaseGridFightSyncData> syncs = [new GridFightLevelSyncData(src, this)];

        syncs.AddRange(await Inst.CreatePendingAction<GridFightElitePendingAction>(sendPacket: false));
        if (CurrentSection.Excel.IsAugment == 1)
        {
            // create augment action
            await Inst.CreatePendingAction<GridFightAugmentPendingAction>(sendPacket: false);
        }

        if (CurrentSection.Excel.NodeType == GridFightNodeTypeEnum.Supply)
        {
            // create supply action
            await Inst.CreatePendingAction<GridFightSupplyPendingAction>(sendPacket: false);
            await Inst.CreatePendingAction<GridFightElitePendingAction>(sendPacket: false);
        }
        else if (CurrentSection.Excel.NodeType == GridFightNodeTypeEnum.EliteBranch)
        {
            await Inst.CreatePendingAction<GridFightEliteBranchPendingAction>(sendPacket: false);
        }

        await Inst.CreatePendingAction<GridFightEnterNodePendingAction>(sendPacket: false);

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

    public async ValueTask<List<BaseGridFightSyncData>> AddPortalBuff(uint portalBuffId, bool sendPacket = true, GridFightSrc src = GridFightSrc.KGridFightSrcSelectPortalBuff)
    {
        var info = new GridFightPortalBuffInfo
        {
            PortalBuffId = portalBuffId
        };

        PortalBuffs.Add(info);

        var syncData = new GridFightAddPortalBuffSyncData(src, info);
        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncData));
        }

        return [syncData];
    }

    #endregion

    #region Information

    public List<GridFightMonsterInfo> GetBossMonsters()
    {
        // get every chapter last section camp
        List<GridFightMonsterInfo> bosses = [];
        foreach (var chapter in Sections.Values)
        {
            var lastSection = chapter.Last();
            var bossMonsters = lastSection.MonsterCamp.Monsters.Where(x => x.MonsterTier == 5).ToList();
            if (bossMonsters.Count == 0)
                continue;

            var boss = bossMonsters.RandomElement();
            bosses.Add(new GridFightMonsterInfo
            {
                MonsterId = boss.MonsterID,
                Tier = boss.MonsterTier,
                MonsterCampId = lastSection.MonsterCamp.ID
            });
        }

        return bosses;
    }

    #endregion

    #region Serialization

    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridLevelInfo = new GridFightLevelInfo
            {
                ChapterId = CurrentSection.ChapterId,
                SectionId = CurrentSection.SectionId,
                RouteId = CurrentSection.Excel.ID,
                GridFightLayerInfo = new GridFightLayerInfo
                {
                    RouteInfo = CurrentSection.ToRouteInfo(),
                    RouteIsPending = CurrentSection.Excel.IsAugment == 1
                },
                BossInfo = new GridFightBossInfo
                {
                    BossMonsters = { GetBossMonsters() }
                },
                GridFightCampList =
                {
                    Sections.Values.SelectMany(x => x).Select(h => h.MonsterCamp.ID).ToHashSet().Select(s =>
                        new GridFightGameCampInfo
                        {
                            MonsterCampId = s,
                        })
                },
                GridChapterInfo = new GridFightChapterInfo
                {
                    SectionInfo =
                    {
                        Sections.Values.SelectMany(x => x).Select(s => s.ToProto())
                    }
                },
                LevelSttInfo = new GridFightLevelSttInfo
                {
                    GridFightDamageSttInfo = ToDamageSttInfo()
                },
                GridFightPortalBuffList = { PortalBuffs.Select(x => x.ToProto()) }
            }
        };
    }

    public GridFightDamageSttInfo ToDamageSttInfo()
    {
        return new GridFightDamageSttInfo
        {
            RoleDamageSttList = { RoleDamageSttInfos.Select(x => x.ToProto()) }
        };
    }

    #endregion
}

public class GridFightRoleDamageSttInfo
{
    public uint RoleId { get; set; }
    public uint Tier { get; set; }
    public double TotalDamage { get; set; }
    public bool IsTrialAvatar { get; set; }
    public bool IsUpgrade { get; set; }

    public GridFightRoleDamageStt ToProto()
    {
        return new GridFightRoleDamageStt
        {
            RoleBasicId = RoleId,
            Tier = Tier,
            IsTrialAvatar = IsTrialAvatar,
            IsUpgrade = IsUpgrade,
            TotalDamage = TotalDamage
        };
    }
}

public class GridFightPortalBuffInfo
{
    public uint PortalBuffId { get; set; }
    public Dictionary<string, uint> SavedValue { get; set; } = [];

    public GridFightGamePortalBuffInfo ToProto()
    {
        return new GridFightGamePortalBuffInfo
        {
            PortalBuffId = PortalBuffId,
            GameSavedValueMap = { SavedValue }
        };
    }

    public BattleGridFightPortalBuffInfo ToBattleInfo()
    {
        return new BattleGridFightPortalBuffInfo
        {
            PortalBuffId = PortalBuffId,
            GameSavedValueMap = { SavedValue }
        };
    }

    public GridFightPortalBuffSyncInfo ToSyncInfo()
    {
        return new GridFightPortalBuffSyncInfo
        {
            PortalBuffId = PortalBuffId,
            GameSavedValueMap = { SavedValue }
        };
    }
}

public class GridFightGameSectionInfo
{
    public GridFightStageRouteExcel Excel { get; }
    public uint ChapterId { get; }
    public uint SectionId { get; }
    public uint BranchId { get; set; } = 1;
    public GridFightCampExcel MonsterCamp { get; set; }
    public List<GridFightGameEncounterInfo> Encounters { get; } = [];

    public GridFightGameSectionInfo(GridFightStageRouteExcel excel, GridFightCampExcel camp)
    {
        Excel = excel;
        ChapterId = excel.ChapterID;
        SectionId = excel.SectionID;

        MonsterCamp = camp;

        if (Excel.NodeType is not GridFightNodeTypeEnum.Monster and not GridFightNodeTypeEnum.CampMonster
                and not GridFightNodeTypeEnum.Boss and not GridFightNodeTypeEnum.EliteBranch) return;

        if (Excel.NodeType is GridFightNodeTypeEnum.EliteBranch)
        {
            List<uint> difficulties = [1, 2, 3];
            BranchId = 0;

            foreach (var diff in difficulties.OrderBy(_ => Guid.NewGuid()).Take(2))
            {
                Encounters.Add(new GridFightGameEncounterInfo(diff, diff, this));
            }
        }
        else
        {
            Encounters.Add(new GridFightGameEncounterInfo(1, 1, this));
        }
    }

    public GridFightRouteInfo ToRouteInfo()
    {
        return new GridFightRouteInfo
        {
            FightCampId = MonsterCamp.ID,
            EliteBranchId = BranchId,
            RouteEncounterList = { Encounters.Select(x => x.ToProto()) }
        };
    }

    public GridFightSectionInfo ToProto()
    {
        return new GridFightSectionInfo
        {
            ChapterId = ChapterId,
            SectionId = SectionId
        };
    }
}

public class GridFightGameEncounterInfo
{
    public GridFightGameEncounterInfo(uint index, uint difficulty, GridFightGameSectionInfo section)
    {
        EncounterIndex = index;
        EncounterDifficulty = difficulty;
        ParentSection = section;

        var waves = GridFightEncounterGenerateHelper.GenerateMonsterWaves(section);
        MonsterWaves.AddRange(waves);
    }

    public uint EncounterIndex { get; set; }
    public uint EncounterDifficulty { get; set; }
    public GridFightGameSectionInfo ParentSection { get; }
    public List<GridFightGameMonsterWaveInfo> MonsterWaves { get; } = [];

    public GridFightEncounterInfo ToProto()
    {
        return new GridFightEncounterInfo
        {
            EncounterIndex = EncounterIndex,
            EncounterExtraDifficultyLevel = EncounterDifficulty,
            EncounterDropInfo = new GridFightDropInfo(),
            MonsterWaveList = { MonsterWaves.Select(x => x.ToProto()) }
        };
    }
}

public class GridFightGameMonsterWaveInfo(uint wave, List<GridFightMonsterExcel> monsters, uint campId)
{
    public uint Wave { get; set; } = wave;

    public List<GridFightGameMonsterInfo> Monsters { get; } = monsters
        .Select(x => new GridFightGameMonsterInfo(x, campId, (uint)Random.Shared.Next(1, (int)(x.MonsterTier + 1)))).ToList();

    public GridEncounterMonsterWave ToProto()
    {
        return new GridEncounterMonsterWave
        {
            EncounterWave = Wave,
            FightMonsterList =
            {
                Monsters.Select(x => x.ToProto())
            }
        };
    }
}

public class GridFightGameMonsterInfo(GridFightMonsterExcel monsters, uint campId, uint tier)
{
    public uint CampId { get; set; } = campId;
    public GridFightMonsterExcel Monster { get; } = monsters;
    public uint Tier { get; } = tier;

    public GridFightMonsterInfo ToProto()
    {
        return new GridFightMonsterInfo
        {
            MonsterId = Monster.MonsterID,
            MonsterCampId = CampId,
            Tier = Tier
        };
    }
}

public static class GridFightEncounterGenerateHelper
{
    private static readonly List<List<List<uint>>> RandomWaveRule =
    [
        [[3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]],
        [[3, 2, 2, 2, 2], [3, 3, 2, 2, 2]],
        [[3, 3, 3, 2, 2, 2, 2, 2]],
        [[2, 2, 2, 2, 3, 3]]
    ];

    public static List<GridFightGameMonsterWaveInfo> GenerateMonsterWaves(GridFightGameSectionInfo section)
    {
        switch (section.Excel.NodeType)
        {
            case GridFightNodeTypeEnum.Monster:
                return GenerateMonsterType(section);

            case GridFightNodeTypeEnum.CampMonster:
            case GridFightNodeTypeEnum.EliteBranch:
                return GenerateCampMonsterType(section);

            case GridFightNodeTypeEnum.Boss:
                return GenerateBossType(section);
            default:
                break;
        }

        return [];
    }

    public static List<GridFightGameMonsterWaveInfo> GenerateMonsterType(GridFightGameSectionInfo section)
    {
        List<GridFightGameMonsterWaveInfo> waves = [];

        var monsters = section.MonsterCamp.Monsters
            .Where(x => x.MonsterTier <= 2).ToList();

        List<GridFightMonsterExcel> targets = [];

        for (var i = 0; i < 5; i++)
        {
            targets.Add(monsters.RandomElement());
        }

        waves.Add(new GridFightGameMonsterWaveInfo(1, targets, section.MonsterCamp.ID));

        return waves;
    }

    public static List<GridFightGameMonsterWaveInfo> GenerateCampMonsterType(GridFightGameSectionInfo section)
    {
        List<GridFightGameMonsterWaveInfo> waves = [];

        var rules = RandomWaveRule.RandomElement();

        foreach (var rule in rules)
        {
            List<GridFightMonsterExcel> excels = [];

            foreach (var tier in rule)
            {
                var targets = section.MonsterCamp.Monsters.Where(x => x.MonsterTier == tier).ToList();
                if (targets.Count == 0)
                    continue;

                var selected = targets.RandomElement();
                excels.Add(selected);
            }

            // random order
            excels = excels.OrderBy(_ => Guid.NewGuid()).ToList();
            waves.Add(new GridFightGameMonsterWaveInfo((uint)(waves.Count + 1), excels, section.MonsterCamp.ID));
        }

        return waves;
    }

    public static List<GridFightGameMonsterWaveInfo> GenerateBossType(GridFightGameSectionInfo section)
    {
        List<GridFightGameMonsterWaveInfo> waves = [];

        var waveNum = section.ChapterId == 3 ? 2 : 1;

        for (var i = 0; i < waveNum; i++)
        {
            if (i == waveNum - 1)
            {
                // boss wave
                var bossMonsters = section.MonsterCamp.Monsters
                    .Where(x => x.MonsterTier == (section.ChapterId == 3 ? 6 : 5))
                    .ToList();

                if (bossMonsters.Count == 0)
                    continue;

                waves.Add(new GridFightGameMonsterWaveInfo((uint)(waves.Count + 1), bossMonsters, section.MonsterCamp.ID));
            }
            else
            {
                // normal wave
                var monsters = section.MonsterCamp.Monsters
                    .Where(x => x.MonsterTier <= 2).ToList();

                List<GridFightMonsterExcel> targets = [];

                for (var j = 0; j < 5; j++)
                {
                    targets.Add(monsters.RandomElement());
                }

                waves.Add(new GridFightGameMonsterWaveInfo((uint)(waves.Count + 1), targets, section.MonsterCamp.ID));
            }
        }

        return waves;
    }
}