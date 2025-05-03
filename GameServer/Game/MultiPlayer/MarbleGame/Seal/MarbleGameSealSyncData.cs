using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;

public class MarbleGameSealSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType)
{
    public MarbleGameSealInstance Instance { get; set; } = inst.Clone();

    public MarbleGameSyncData ToProto()
    {
        return new MarbleGameSyncData
        {
            Attack = Instance.Attack,
            Id = (uint)Instance.Id,
            Hp = Instance.CurHp,
            MaxHp = Instance.MaxHp,
            SealPosition = Instance.Position,
            SealRotation = Instance.Rotation,
            SealOnStage = Instance.OnStage,
            SealSize = Instance.Size,
            FrameType = frameType
        };
    }
}