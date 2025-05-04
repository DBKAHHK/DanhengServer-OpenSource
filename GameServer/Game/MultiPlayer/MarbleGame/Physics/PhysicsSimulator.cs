
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Microsoft.Xna.Framework;

namespace EggLink.DanhengServer.GameServer.Game.MultiPlayer.MarbleGame.Physics;

public class PhysicsSimulator
{
    public World World { get; }
    public List<CollisionRecord> CollisionRecords { get; } = [];

    public float CurrentTime;

    public PhysicsSimulator(Vector2 gravity, float leftBound, float rightBound, float topBound, float bottomBound)
    {
        World = new World(gravity);
        CreateWalls(leftBound, rightBound, topBound, bottomBound);

        World.ContactManager.OnBroadphaseCollision += OnCollisionDetected;
    }

    private void CreateWalls(float left, float right, float top, float bottom)
    {
        // 创建四个静态墙体
        BodyFactory.CreateEdge(World, new Vector2(left, bottom), new Vector2(left, top));  // 左墙
        BodyFactory.CreateEdge(World, new Vector2(right, bottom), new Vector2(right, top)); // 右墙
        BodyFactory.CreateEdge(World, new Vector2(left, top), new Vector2(right, top));     // 上墙
        BodyFactory.CreateEdge(World, new Vector2(left, bottom), new Vector2(right, bottom));// 下墙
    }

    private void OnCollisionDetected(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
    {
        Fixture fixtureA = proxyA.Fixture;
        Fixture fixtureB = proxyB.Fixture;

        // 过滤墙体碰撞记录（可选）
        if (fixtureA.Body.BodyType == BodyType.Static ||
            fixtureB.Body.BodyType == BodyType.Static)
            return;

        if (fixtureA.Body.UserData is int idA && fixtureB.Body.UserData is int idB)
        {
            CollisionRecords.Add(new CollisionRecord
            {
                Time = CurrentTime,
                ObjectAId = idA,
                ObjectBId = idB,
                Position = (fixtureA.Body.Position + fixtureB.Body.Position) / 2, // 使用实际接触点
                VelocityA = fixtureA.Body.LinearVelocity,
                VelocityB = fixtureB.Body.LinearVelocity
            });
        }
    }

    public void AddBall(int id, Vector2 position, float mass, float radius, Vector2? velocity = null)
    {
        float density = mass / (MathF.PI * radius * radius);

        Body body = BodyFactory.CreateCircle(World, radius, density, position);
        body.BodyType = BodyType.Dynamic;
        body.Restitution = 0.8f;  // 统一恢复系数
        body.Friction = 0.3f;
        body.LinearDamping = 1.5f;  // 使用阻尼代替手动衰减
        body.UserData = id;
        body.IsBullet = true;  // 启用CCD

        if (velocity.HasValue)
            body.LinearVelocity = velocity.Value;
    }



    public void Simulate(float maxDuration, float timeStep = 0.001f)
    {
        World.Step(0);
        while (CurrentTime < maxDuration && !AllObjectsStopped())
        {
            World.Step(timeStep);
            CurrentTime += timeStep;
        }
    }

    public IEnumerable<string> GetFinalPositions()
    {
        return World.BodyList.Select(body =>
            $"ID {(int)(body?.UserData ?? -1)}: ({body.Position.X:F2}, {body.Position.Y:F2})");
    }

    private bool AllObjectsStopped() =>
        World.BodyList.All(b => b.LinearVelocity.Length() < 0.1f);
}

public class CollisionRecord
{
    public float Time { get; set; }
    public int ObjectAId { get; set; }
    public int ObjectBId { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 VelocityA { get; set; }
    public Vector2 VelocityB { get; set; }
}