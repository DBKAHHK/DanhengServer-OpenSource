using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight;

public class GridFightInstance(uint season, uint divisionId, bool isOverLock, uint uniqueId)
{
    public uint Season { get; } = season;
    public uint DivisionId { get; } = divisionId;
    public bool IsOverLock { get; } = isOverLock;
    public uint UniqueId { get; } = uniqueId;
    public List<BaseGridFightComponent> Components { get; } = [];

    public void InitializeComponents()
    {
        Components.Add(new GridFightBasicComponent(this));
        Components.Add(new GridFightLevelComponent(this));
        Components.Add(new GridFightAvatarComponent(this));

        _ = GetComponent<GridFightAvatarComponent>().AddAvatar(1414, 3);  // TODO test
    }

    public T GetComponent<T>() where T : BaseGridFightComponent
    {
        return (T)Components.First(c => c is T);
    }

    public GridFightCurrentInfo ToProto()
    {
        return new GridFightCurrentInfo
        {
            DivisionId = DivisionId,
            Season = Season,
            IsOverlock = IsOverLock,
            UniqueId = UniqueId,
            PendingAction = new GridFightPendingAction(),
            GridFightGameData = ToGameDataInfo(),
            RogueCurrentGameInfo = { ToGameInfos() }
        };
    }

    public List<GridFightGameInfo> ToGameInfos()
    {
        return (from c in Components select c.ToProto()).ToList();
    }

    public GridFightGameData ToGameDataInfo()
    {
        return new GridFightGameData();
    }
}