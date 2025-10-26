using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight;

public class GridFightInstance(PlayerInstance player, uint season, uint divisionId, bool isOverLock, uint uniqueId)
{
    public uint Season { get; } = season;
    public uint DivisionId { get; } = divisionId;
    public bool IsOverLock { get; } = isOverLock;
    public uint UniqueId { get; } = uniqueId;
    public List<BaseGridFightComponent> Components { get; } = [];
    public PlayerInstance Player { get; } = player;

    public BattleInstance? StartBattle()
    {
        return Player.BattleManager!.StartGridFightBattle(this);
    }

    public async ValueTask EndBattle(BattleInstance battle)
    {
        if (battle.BattleEndStatus != BattleEndStatus.BattleEndWin) return;

        List<BaseGridFightSyncData> syncs = [];

        await Player.SendPacket(new PacketGridFightEndBattleStageNotify(this));

        var basicComp = GetComponent<GridFightBasicComponent>();
        await basicComp.UpdateGoldNum(5, false, GridFightSrc.KGridFightSrcNone);

        syncs.Add(new GridFightGoldSyncData(GridFightSrc.KGridFightSrcBattleEnd, basicComp.Data));
        syncs.AddRange(await GetComponent<GridFightLevelComponent>().EnterNextSection(false));

        await Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
    }

    public void InitializeComponents()
    {
        Components.Add(new GridFightBasicComponent(this));
        Components.Add(new GridFightShopComponent(this));
        Components.Add(new GridFightLevelComponent(this));
        Components.Add(new GridFightAvatarComponent(this));

        _ = GetComponent<GridFightAvatarComponent>().AddAvatar(1414, 3, false);  // TODO test
        _ = GetComponent<GridFightShopComponent>().RefreshShop(true, false);
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