using EggLink.DanhengServer.Proto;
using Microsoft.Xna.Framework;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Seal;

public class MarbleGameSealSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType)
{
    public MarbleGameSealInstance Instance { get; set; } = inst.Clone();

    public virtual MarbleGameSyncData ToProto()
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

public class MarbleGameSealActionSyncData(MarbleGameSealInstance inst, MarbleFrameType frameType, float time = 0) : MarbleGameSealSyncData(inst, frameType)
{
    public override MarbleGameSyncData ToProto()
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
            SealVelocity = Instance.Velocity,
            FrameType = frameType,
            Time = time
        };
    }
}

public class MarbleGameSealCollisionSyncData(MarbleGameSealInstance inst, int collideOwnerId, int collideTargetId, float time, Vector2 collidePos) : MarbleGameSealSyncData(inst, MarbleFrameType.Collide)
{
    public override MarbleGameSyncData ToProto()
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
            FrameType = MarbleFrameType.Collide,
            CollideType = collideTargetId == 1 ? MarbleFactionType.Field : collideTargetId / 100 == collideOwnerId / 100 ? MarbleFactionType.Ally : MarbleFactionType.Enemy,
            CollideOwnerId = (uint)collideOwnerId,
            CollideTargetId = (uint)collideTargetId,
            CollisionPosition = new MarbleSealVector
            {
                X = collidePos.X,
                Y = collidePos.Y
            },
            CollisionTargetVelocity = new MarbleSealVector(),
            SealVelocity = Instance.Velocity,
            Time = time
        };
    }
}