using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Battle;

public class PacketSceneEnterStageScRsp : BasePacket
{
    public PacketSceneEnterStageScRsp() : base(CmdIds.SceneEnterStageScRsp)
    {
        var proto = new SceneEnterStageScRsp
        {
            Retcode = 1
        };

        SetData(proto);
    }

    public PacketSceneEnterStageScRsp(BattleInstance battleInstance) : base(CmdIds.SceneEnterStageScRsp)
    {
        var proto = new SceneEnterStageScRsp
        {
            BattleInfo = battleInstance.ToProto()
        };

        SetData(proto);
    }
}