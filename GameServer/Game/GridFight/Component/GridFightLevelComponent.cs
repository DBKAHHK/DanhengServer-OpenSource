using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.GridFight;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightLevelComponent : BaseGridFightComponent
{
    private uint _curChapterId = 1;
    private uint _curSectionId = 1;
    public Dictionary<uint, List<GridFightGameSectionInfo>> Sections { get; } = [];
    public GridFightGameSectionInfo CurrentSection => Sections[_curChapterId][(int)(_curSectionId - 1)];
    public List<GridFightRoleDamageSttInfo> RoleDamageSttInfos { get; } = [];

    public GridFightLevelComponent(GridFightInstance inst) : base(inst)
    {
        // TODO: randomly select a base route id
        List<uint> chapterIds = [1100];
        List<GridFightCampExcel> campPool = GameData.GridFightCampData.Values.ToList();
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
            Sections[(uint)chapterId] = [..select.Values.Select(x => new GridFightGameSectionInfo(x, camp))];
        }
    }

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

    public async ValueTask<List<BaseGridFightSyncData>> EnterNextSection(bool sendPacket = true)
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

        List<BaseGridFightSyncData> syncs = [new GridFightLevelSyncData(GridFightSrc.KGridFightSrcBattleEnd, this)];
        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

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
                    RouteInfo = CurrentSection.ToRouteInfo()
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
                }
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

public class GridFightGameSectionInfo
{
    public GridFightStageRouteExcel Excel { get; }
    public uint ChapterId { get; }
    public uint SectionId { get; }
    public GridFightCampExcel MonsterCamp { get; set; }
    public List<GridFightGameEncounterInfo> Encounters { get; } = [];

    public GridFightGameSectionInfo(GridFightStageRouteExcel excel, GridFightCampExcel camp)
    {
        Excel = excel;
        ChapterId = excel.ChapterID;
        SectionId = excel.SectionID;

        MonsterCamp = camp;

        Encounters.Add(new GridFightGameEncounterInfo(1, 1, this));
    }

    public GridFightRouteInfo ToRouteInfo()
    {
        return new GridFightRouteInfo
        {
            FightCampId = MonsterCamp.ID,
            EliteBranchId = 0,
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

        if (ParentSection.Excel.NodeType != GridFightNodeTypeEnum.Monster) return;

        var monsterPool = ParentSection.MonsterCamp.Monsters.Where(x => x.MonsterTier <= 3).OrderBy(_ => Guid.NewGuid()).ToList();

        List<GridFightMonsterExcel> monsters = [];

        foreach (var _ in Enumerable.Range(0, Random.Shared.Next(1, 5)))
        {
            monsters.Add(monsterPool.RandomElement());
        }

        MonsterWaves.Add(new GridFightGameMonsterWaveInfo(1, monsters, ParentSection.MonsterCamp.ID));
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
    public uint CampId { get; set; } = campId;
    public List<GridFightMonsterExcel> Monsters { get; } = monsters;

    public GridEncounterMonsterWave ToProto()
    {
        return new GridEncounterMonsterWave
        {
            EncounterWave = Wave,
            FightMonsterList =
            {
                Monsters.Select(x => new GridFightMonsterInfo
                {
                    MonsterId = x.MonsterID,
                    MonsterCampId = CampId,
                    Tier = x.MonsterTier
                })
            }
        };
    }
}