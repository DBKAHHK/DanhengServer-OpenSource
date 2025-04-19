using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Avatar;

public class AvatarManager : BasePlayerManager
{
    public AvatarManager(PlayerInstance player) : base(player)
    {
        AvatarData = DatabaseHelper.Instance!.GetInstanceOrCreateNew<AvatarData>(player.Uid);
        foreach (var avatar in AvatarData.Avatars)
        {
            avatar.PlayerData = player.Data;
            avatar.Excel = GameData.AvatarConfigData[avatar.AvatarId];
        }
    }

    public AvatarData AvatarData { get; }

    public async ValueTask<AvatarConfigExcel?> AddAvatar(int avatarId, bool sync = true, bool notify = true,
        bool isGacha = false)
    {
        GameData.AvatarConfigData.TryGetValue(avatarId, out var avatarExcel);
        if (avatarExcel == null) return null;

        GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var multiPathAvatar);
        if (multiPathAvatar != null && multiPathAvatar.BaseAvatarID != avatarId)
        {
            // Is path
            foreach (var avatarData in AvatarData.Avatars)
                if (avatarData.AvatarId == multiPathAvatar.BaseAvatarID)
                {
                    // Add path for the character
                    avatarData.PathInfoes.Add(avatarId, new PathInfo(avatarId));
                    break;
                }

            return null;
        }

        var avatar = new AvatarInfo(avatarExcel)
        {
            AvatarId = avatarId >= 8001 ? 8001 : avatarId,
            Level = 1,
            Timestamp = Extensions.GetUnixSec(),
            CurrentHp = 10000,
            CurrentSp = 0
        };

        if (avatarId >= 8001)
        {
            if (GetHero() != null) return null; // Only one hero
            avatar.PathId = avatarId;
        }

        avatar.PlayerData = Player.Data;
        AvatarData.Avatars.Add(avatar);

        if (sync)
            await Player.SendPacket(new PacketPlayerSyncScNotify(avatar));

        if (notify) await Player.SendPacket(new PacketAddAvatarScNotify(avatar.GetBaseAvatarId(), isGacha));

        return avatarExcel;
    }

    public AvatarInfo? GetAvatar(int avatarId)
    {
        if (avatarId > 8000) avatarId = 8001;
        if (GameData.MultiplePathAvatarConfigData.ContainsKey(avatarId))
            avatarId = GameData.MultiplePathAvatarConfigData[avatarId].BaseAvatarID;
        return AvatarData.Avatars.Find(avatar => avatar.AvatarId == avatarId);
    }

    public AvatarInfo? GetHero()
    {
        return AvatarData.Avatars.Find(avatar => avatar.AvatarId == 8001);
    }

    public async ValueTask ReforgeRelic(int uniqueId)
    {
        var relic = Player.InventoryManager!.Data.RelicItems.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (relic == null) return;
        await Player.InventoryManager!.RemoveItem(238, 1);

        var subAffixesClone = relic.SubAffixes.Select(x => x.Clone()).ToList();

        var levelUpCnt = 0;
        foreach (var subAffix in relic.SubAffixes)
        {
            levelUpCnt += subAffix.Count - 1;
            subAffix.Count = 1;
            subAffix.Step = 0;
        }
        relic.IncreaseRandomRelicSubAffix(levelUpCnt);
        relic.ReforgeSubAffixes = relic.SubAffixes;
        relic.SubAffixes = subAffixesClone;

        await Player.SendPacket(new PacketPlayerSyncScNotify(relic));
    }

    public async ValueTask ConfirmReforgeRelic(int uniqueId, bool isCancel)
    {
        var relic = Player.InventoryManager!.Data.RelicItems.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (relic == null) return;
        if (relic.ReforgeSubAffixes.Count == 0) return;

        if (!isCancel)
            relic.SubAffixes = relic.ReforgeSubAffixes;
        relic.ReforgeSubAffixes = [];

        await Player.SendPacket(new PacketPlayerSyncScNotify(relic));
    }

    public List<RelicSmartWearPlan> GetRelicPlan(int avatarId)
    {
        var planList = new List<RelicSmartWearPlan>();
        foreach (var plan in Player.InventoryManager!.Data.RelicPlans)
        {
            if (plan.Value.EquipAvatar != avatarId) continue;
            if (plan.Value.InsideRelic.Count == 0 && plan.Value.OutsideRelic.Count == 0) continue;

            planList.Add(new RelicSmartWearPlan
            {
                AvatarId = (uint)avatarId,
                UniqueId = (uint)plan.Key,
                OutsideRelicList = { plan.Value.OutsideRelic.Select(x => (uint)x) },
                InsideRelicList = { plan.Value.InsideRelic.Select(x => (uint)x) },
            });
        }
        return planList;
    }

    public RelicSmartWearPlan AddRelicPlan(RelicSmartWearPlan plan)
    {
        var curUnique = Player.InventoryManager!.Data.RelicPlans.Keys;
        var uniqueId = curUnique.Count > 0 ? curUnique.Max() + 1 : 1;
        Player.InventoryManager!.Data.RelicPlans[uniqueId] = new RelicPlanData
        {
            EquipAvatar = (int)plan.AvatarId,
            InsideRelic = [.. plan.InsideRelicList.Select(x => (int)x)],
            OutsideRelic = [.. plan.OutsideRelicList.Select(x => (int)x)]
        };

        plan.UniqueId = (uint)uniqueId;
        return plan;
    }

    public void UpdateRelicPlan(RelicSmartWearPlan plan)
    {
        var avatar = GetAvatar((int)plan.AvatarId)!.GetCurAvatarInfo();
        Player.InventoryManager!.Data.RelicPlans[(int)plan.UniqueId] = new RelicPlanData
        {
            EquipAvatar = (int)plan.AvatarId,
            InsideRelic = [.. plan.InsideRelicList.Select(x => (int)x)],
            OutsideRelic = [.. plan.OutsideRelicList.Select(x => (int)x)]
        };
    }

    public void DeleteRelicPlan(int uniqueId)
    {
        Player.InventoryManager!.Data.RelicPlans[uniqueId] = new();
    }
}