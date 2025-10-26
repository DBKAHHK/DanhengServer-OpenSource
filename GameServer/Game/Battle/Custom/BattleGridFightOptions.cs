using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.GameServer.Game.GridFight;
using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Battle.Custom;

public class BattleGridFightOptions(GridFightGameSectionInfo curSection, GridFightInstance inst, PlayerInstance player)
{
    public GridFightGameEncounterInfo Encounter { get; set; } = curSection.Encounters.RandomElement();
    public GridFightInstance Inst { get; set; } = inst;
    public GridFightAvatarComponent AvatarComponent { get; set; } = inst.GetComponent<GridFightAvatarComponent>();
    public GridFightBasicComponent BasicComponent { get; set; } = inst.GetComponent<GridFightBasicComponent>();
    public GridFightGameSectionInfo CurSection { get; set; } = curSection;
    public PlayerInstance Player { get; set; } = player;

    public void HandleProto(SceneBattleInfo proto, BattleInstance battle)
    {
        var avatars = AvatarComponent.GetForegroundAvatarInfos();

        var formatted = avatars.Select(x =>
            x.ToBattleProto(
                new PlayerDataCollection(Player.Data, Player.InventoryManager!.Data,
                    Player.LineupManager!.GetCurLineup()!), AvatarType.AvatarGridFightType)).ToList();

        proto.BattleAvatarList.Add(formatted);

        foreach (var wave in Encounter.MonsterWaves)
        {
            proto.MonsterWaveList.Add(new SceneMonsterWave
            {
                BattleStageId = proto.StageId,
                BattleWaveId = wave.Wave,
                MonsterParam = new SceneMonsterWaveParam
                {
                    Level = 89
                },
                MonsterList =
                {
                    wave.Monsters.Select(x => new SceneMonster
                    {
                        MonsterId = x.MonsterID
                    })
                }
            });
        }

        foreach (var role in AvatarComponent.Data.Roles)
        {
            if (!GameData.GridFightRoleStarData.TryGetValue(role.RoleId << 2 | role.Tier, out var roleConf)) continue;
            battle.BattleEvents.TryAdd((int)roleConf.BEID, new BattleEventInstance((int)roleConf.BEID, 5000));
        }

        proto.BattleGridFightInfo = new BattleGridFightInfo
        {
            GridGameAvatarList =
            {
                AvatarComponent.Data.Roles.Where(x => x.Pos <= 4).Select(x => x.ToBattleInfo())
            },
            BattleWaveId = 1,
            GridFightCurLevel = BasicComponent.Data.CurLevel,
            GridFightLineupHp = BasicComponent.Data.CurHp,
            GridFightAvatarList = { formatted },
            GridFightStageInfo = new BattleGridFightStageInfo
            {
                ChapterId = CurSection.ChapterId,
                RouteId = CurSection.Excel.ID,
                SectionId = CurSection.SectionId
            },
            IsOverlock = Inst.IsOverLock,
            Season = Inst.Season
        };
    }
}