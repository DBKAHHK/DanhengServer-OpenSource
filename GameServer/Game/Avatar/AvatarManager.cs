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

public class AvatarManager(PlayerInstance player) : BasePlayerManager(player)
{
    public AvatarData AvatarData { get; } = DatabaseHelper.Instance!.GetInstanceOrCreateNew<AvatarData>(player.Uid);

    public async ValueTask<AvatarConfigExcel?> AddAvatar(int avatarId, bool sync = true, bool notify = true,
        bool isGacha = false)
    {
        if (avatarId > 8000 && avatarId % 2 != (int)Player.Data.CurrentGender % 2) return null;
        GameData.AvatarConfigData.TryGetValue(avatarId, out var avatarExcel);
        if (avatarExcel == null) return null;

        GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var multiPathAvatar);
        var baseAvatarId = multiPathAvatar?.BaseAvatarID ?? avatarId;
        var avatarData = GetAvatar(baseAvatarId);

        var pathInfo = new MultiPathData();
        foreach (var skillTree in avatarExcel.DefaultSkillTree)
            pathInfo.SkillTree.Add(skillTree.PointID, 1);

        // Check if base avatar exist
        if (avatarData != null)
        {
            if (multiPathAvatar == null) return null;
            if (avatarData.PathInfo.TryAdd(avatarId, pathInfo))
                return avatarExcel;
            return null;
        }

        var avatar = new AvatarInfo(baseAvatarId, avatarId)
        {
            Level = 1,
            Timestamp = Extensions.GetUnixSec(),
            CurrentHp = 10000,
            CurrentSp = 0
        };
        avatar.PathInfo[avatarId] = pathInfo; // Add curAvatarId's pathinfo
        AvatarData.Avatars.Add(avatar);

        if (sync) await Player.SendPacket(new PacketPlayerSyncScNotify(avatar));
        if (notify) await Player.SendPacket(new PacketAddAvatarScNotify(avatar.BaseAvatarId, isGacha));

        return avatarExcel;
    }

    public AvatarInfo? GetAvatar(int avatarId)
    {
        if (GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var pathConfig))
            avatarId = pathConfig.BaseAvatarID;

        return AvatarData.Avatars.Find(avatar => avatar.BaseAvatarId == avatarId);
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