using EggLink.DanhengServer.GameServer.Game.GridFight;
using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;

public class PacketGridFightEndBattleStageNotify : BasePacket
{
    public PacketGridFightEndBattleStageNotify(GridFightInstance inst) : base(CmdIds.GridFightEndBattleStageNotify)
    {
        var levelComp = inst.GetComponent<GridFightLevelComponent>();
        var curSec = levelComp.CurrentSection;

        var proto = new GridFightEndBattleStageNotify
        {
            SectionId = curSec.SectionId,
            RouteId = curSec.Excel.ID,
            ChapterId = curSec.ChapterId,
            GridFightDamageSttInfo = new GridFightDamageSttInfo(),
            EMLLKALLOPL = new HEHHADKPDOC
            {
                CGECGAAJLJM = new(),
                NMMJMPNGIGD = new()
            }
        };

        SetData(proto);
    }
}