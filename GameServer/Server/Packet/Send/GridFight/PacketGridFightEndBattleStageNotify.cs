using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.GridFight;
using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;

public class PacketGridFightEndBattleStageNotify : BasePacket
{
    public PacketGridFightEndBattleStageNotify(GridFightInstance inst, uint expAddNum, GridFightBasicInfoPb prev,
        GridFightBasicInfoPb cur, List<GridFightRoleDamageSttInfo> stt, bool win, uint baseCoin, uint interestCoin,
        uint comboCoin) : base(CmdIds.GridFightEndBattleStageNotify)
    {
        var levelComp = inst.GetComponent<GridFightLevelComponent>();
        var curSec = levelComp.CurrentSection;

        var proto = new GridFightEndBattleStageNotify
        {
            SectionId = curSec.SectionId,
            RouteId = curSec.Excel.ID,
            ChapterId = curSec.ChapterId,
            GridFightDamageSttInfo = new GridFightDamageSttInfo
            {
                RoleDamageSttList = { stt.Select(x => x.ToProto()) }
            },
            GridFightLevelUpdateInfo = new GridFightLevelUpdateInfo
            {
                PrevLevelInfo = new GridFightLevelDisplayInfo
                {
                    Level = prev.CurLevel,
                    MaxExp = GameData.GridFightPlayerLevelData[prev.CurLevel].LevelUpExp,
                    Exp = prev.LevelExp
                },
                CurLevelInfo = new GridFightLevelDisplayInfo
                {
                    Level = cur.CurLevel,
                    MaxExp = GameData.GridFightPlayerLevelData[cur.CurLevel].LevelUpExp,
                    Exp = cur.LevelExp
                },
                AddExpNum = expAddNum
            },
            AddExpNum = expAddNum,
            GridFightCurComboNum = cur.ComboNum,
            GridFightChallengeWin = win,
            GridFightCoinBaseNum = baseCoin,
            GridFightCoinInterestNum = interestCoin,
            GridFightCoinComboNum = comboCoin,
            GridFightCurLineupHp = cur.CurHp,
            GridFightMaxLineupHp = GridFightBasicComponent.MaxHp
        };

        SetData(proto);
    }
}