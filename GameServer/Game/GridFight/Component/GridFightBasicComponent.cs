using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightBasicComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridBasicInfo = new GridFightGameBasicInfo
            {
                GridFightCurLevel = 1,
                GridFightCurLevelExp = 1,
                GridFightLevelCost = 1,
                GridFightMaxAvatarCount = 2,
                GridFightOffFieldMaxCount = 9,
                GridFightMaxFieldCount = 13,
                GridFightLineupHp = 100,
                GridFightCurGold = 200,
                GridFightMaxGold = 2000,
                OCMGMEHECBB = new OPIBBPCHFII
                {
                    IJDIAOMINLB = new BHJALAPDBLH()
                },
                CALCJMHAKPF = new OLEIDBLBILD()
            }
        };
    }
}