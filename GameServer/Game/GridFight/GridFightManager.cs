using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight;

public class GridFightManager(PlayerInstance player) : BasePlayerManager(player)
{
    public GridFightQueryInfo ToProto()
    {
        return new GridFightQueryInfo
        {
            GridFightRewardInfo = ToRewardInfo(),
            GridFightStaticGameInfo = ToGameInfo()
        };
    }

    public GridFightCurrentInfo ToCurrentInfo()
    {
        return new GridFightCurrentInfo
        {
            GridFightGameData = new GridFightGameData(),
            Season = 1,
            DivisionId = 1,
            UniqueId = 1,
            PendingAction = new GridFightPendingAction()
        };
    }

    public GridFightRewardInfo ToRewardInfo()
    {
        var time = RogueManager.GetCurrentRogueTime();

        return new GridFightRewardInfo
        {
            GridFightTalentInfo = new GridFightTalentInfo
            {
                JMOJEOALCLO = { 1011 }
            },
            //GridFightWeeklyReward = new GridFightTakeWeeklyRewardInfo
            //{
            //    EndTime = time.Item2,
            //}
        };
    }

    public GridFightStaticGameInfo ToGameInfo()
    {
        return new GridFightStaticGameInfo
        {
            GridFightTalentInfo = new GridFightTalentInfo
            {
                JMOJEOALCLO = { }
            },
            DivisionId = 10920,
            GridFightGameValueInfo = ToFightGameValueInfo(),
            Exp = new ()
            {
                BNCBPJIBHGI = 1,
                GDDHBECJECP = 1
            },
            MGGGAJJBAMN = 1,
            ECMBJBBGHGG = 1,
            IFEHBIMEMEC = 10
        };
    }

    public GridFightGameValueInfo ToFightGameValueInfo()
    {
        return new GridFightGameValueInfo
        {
            GridFightAvatarInfo = new GridFightAvatarInfo
            {
                GridFightAvatarList = { GameData.GridFightRoleBasicInfoData.Keys }
            },
            GridFightItemInfo = new GridFightItemInfo
            {
                GridFightItemList = { GameData.GridFightItemsData.Keys }
            },
            IADIEGJKNHA = new(),
            OCKCODILEOP = new(),
            PJBEPNCFGDJ = new()
        };
    }
}