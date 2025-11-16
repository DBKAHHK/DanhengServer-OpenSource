using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;
using System.Data.OscarClient;
using System.Threading;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightItemsComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public GridFightItemsInfoPb Data { get; set; } = new();

    public async ValueTask<(GridFightEquipmentItemPb?, List<BaseGridFightSyncData>)> AddEquipment(uint equipmentId, GridFightSrc src = GridFightSrc.KGridFightSrcNone, bool sendPacket = true, uint groupId = 0, params uint[] param)
    {
        if (!GameData.GridFightEquipmentData.ContainsKey(equipmentId))
            return (null, []);

        var roleComp = Inst.GetComponent<GridFightRoleComponent>();
        var info = new GridFightEquipmentItemPb
        {
            ItemId = equipmentId,
            UniqueId = ++roleComp.Data.CurUniqueId
        };

        Data.EquipmentItems.Add(info);
        var syncData = new GridFightAddGameItemSyncData(src, [info], [], groupId, param);

        if (sendPacket)
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncData));

        return (info, [syncData]);
    }

    public async ValueTask<List<BaseGridFightSyncData>> UpdateConsumable(uint consumableId, int count, GridFightSrc src = GridFightSrc.KGridFightSrcNone, bool sendPacket = true, params uint[] param)
    {
        if (!GameData.GridFightConsumablesData.ContainsKey(consumableId) || count == 0)
            return [];

        var existingItem = Data.ConsumableItems.FirstOrDefault(x => x.ItemId == consumableId);

        var isRemove = false;

        if (existingItem != null)
        {
            if (count < 0)
            {
                count = (int)-Math.Min(existingItem.Count, -count);
                existingItem.Count -= (uint)-count;
                if (existingItem.Count == 0)
                {
                    Data.ConsumableItems.Remove(existingItem);
                }


                isRemove = true;
            }
            else
            {
                existingItem.Count += (uint)count;
            }
        }
        else
        {
            if (count < 0) return [];

            var info = new GridFightConsumableItemPb
            {
                ItemId = consumableId,
                Count = (uint)count  // safe
            };

            Data.ConsumableItems.Add(info);
            existingItem = info;
        }

        BaseGridFightSyncData syncData = isRemove
            ? new GridFightRemoveGameItemSyncData(src, [], [existingItem.ToUpdateInfo(count)], 0, param)
            : new GridFightAddGameItemSyncData(src, [], [existingItem.ToUpdateInfo(count)], 0, param);

        if (sendPacket)
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncData));

        return [syncData];
    }

    public async ValueTask<List<BaseGridFightSyncData>> RemoveEquipment(uint uniqueId, GridFightSrc src = GridFightSrc.KGridFightSrcNone, bool sendPacket = true, params uint[] param)
    {
        var existingItem = Data.EquipmentItems.FirstOrDefault(x => x.UniqueId == uniqueId);

        if (existingItem == null)
            return [];

        Data.EquipmentItems.Remove(existingItem);

        List<BaseGridFightSyncData> syncDatas =
        [
            new GridFightRemoveGameItemSyncData(src, [existingItem], [], 0, param)
        ];

        // if equipped
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();
        foreach (var role in roleComp.Data.Roles)
        {
            if (!role.EquipmentIds.Contains(existingItem.ItemId)) continue;

            role.EquipmentIds.Remove(existingItem.ItemId);
            syncDatas.Add(new GridFightRoleUpdateSyncData(src, role.Clone(), 0, param));
        }

        if (sendPacket)
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncDatas));

        return syncDatas;
    }

    public async ValueTask<List<BaseGridFightSyncData>> CraftEquipment(uint targetEquipId, List<uint> materials)
    {
        List<BaseGridFightSyncData> syncDatas = [];

        // remove materials
        foreach (var matId in materials)
        {
            syncDatas.AddRange(await RemoveEquipment(matId, GridFightSrc.KGridFightSrcCraftEquip, false));
        }

        // add crafted equipment
        var addEquipDatas = await AddEquipment(targetEquipId, GridFightSrc.KGridFightSrcCraftEquip, false);
        syncDatas.AddRange(addEquipDatas.Item2);

        // if auto equip to a role
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();
        var roleUnique =
            (syncDatas.FirstOrDefault(x => x is GridFightRoleUpdateSyncData) as GridFightRoleUpdateSyncData)?.Role
            .UniqueId ?? 0;

        if (roleUnique != 0 && addEquipDatas.Item1 != null)
        {
            syncDatas.AddRange(await roleComp.DressRole(roleUnique, addEquipDatas.Item1.UniqueId, GridFightSrc.KGridFightSrcCraftEquip, false));
        }

        // sync
        if (syncDatas.Count > 0)
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncDatas));

        return syncDatas;
    }

    public async ValueTask<List<BaseGridFightSyncData>> TakeDrop(List<GridFightDropItemInfo> drops, bool sendPacket = false, GridFightSrc src = GridFightSrc.KGridFightSrcNone, uint groupId = 0, params uint[] param)
    {
        var syncs = new List<BaseGridFightSyncData>();
        var basicComp = Inst.GetComponent<GridFightBasicComponent>();
        var shopComp = Inst.GetComponent<GridFightShopComponent>();
        var roleComp = Inst.GetComponent<GridFightRoleComponent>();
        var orbComp = Inst.GetComponent<GridFightOrbComponent>();

        foreach (var item in drops)
        {
            // take drop
            switch (item.DropType)
            {
                case GridFightDropType.Coin:
                {
                    await basicComp.UpdateGoldNum((int)item.Num, false);
                    syncs.Add(new GridFightGoldSyncData(src, basicComp.Data, groupId, param));
                    break;
                }
                case GridFightDropType.Exp:
                {
                    await basicComp.AddLevelExp(item.Num, false);
                    syncs.Add(new GridFightPlayerLevelSyncData(src, basicComp.Data, groupId, param));
                    break;
                }
                case GridFightDropType.Refresh:
                {
                    shopComp.Data.FreeRefreshCount += item.Num;
                    syncs.Add(new GridFightShopSyncData(src, shopComp.Data, basicComp.Data.CurLevel, groupId, param));
                    break;
                }
                case GridFightDropType.Role:
                {
                    syncs.AddRange(await roleComp.AddAvatar(item.DropItemId, item.DisplayValue.Tier, false, true,
                        src, groupId, 0, param));
                    break;
                }
                case GridFightDropType.Item:
                {
                    // consumable or equipment
                    if (GameData.GridFightConsumablesData.ContainsKey(item.DropItemId))
                    {
                        syncs.AddRange(await UpdateConsumable(item.DropItemId, (int)item.Num, src, false, param));
                    }
                    else if (GameData.GridFightEquipmentData.ContainsKey(item.DropItemId))
                    {
                        for (uint i = 0; i < item.Num; i++)
                        {
                            syncs.AddRange((await AddEquipment(item.DropItemId, src, false, groupId, param)).Item2);
                        }
                    }

                    break;
                }
                case GridFightDropType.Orb:
                {
                    // add orbs
                    syncs.AddRange(await orbComp.AddOrb(item.DropItemId, src, false, groupId, param));
                    break;
                }
            }
        }

        if (sendPacket && syncs.Count > 0)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
        }

        return syncs;
    }

    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridItemsInfo = new GridFightGameItemsInfo
            {
                GridFightEquipmentList = { Data.EquipmentItems.Select(x => x.ToProto()) },
                GridFightConsumableList = { Data.ConsumableItems.Select(x => x.ToProto()) }
            }
        };
    }
}

public static class GridFightItemsComponentExtensions
{
    public static GridFightEquipmentInfo ToProto(this GridFightEquipmentItemPb equipment)
    {
        return new GridFightEquipmentInfo
        {
            GridFightEquipmentId = equipment.ItemId,
            UniqueId = equipment.UniqueId
        };
    }

    public static GridFightConsumableInfo ToProto(this GridFightConsumableItemPb consumable)
    {
        return new GridFightConsumableInfo
        {
            ItemId = consumable.ItemId,
            Num = consumable.Count
        };
    }

    public static GridFightConsumableUpdateInfo ToUpdateInfo(this GridFightConsumableItemPb consumable, int updateCount)
    {
        return new GridFightConsumableUpdateInfo
        {
            ItemId = consumable.ItemId,
            Num = consumable.Count,
            ItemStackCount = updateCount
        };
    }

    public static BattleGridFightEquipmentInfo ToBattleInfo(this GridFightEquipmentItemPb equipment)
    {
        return new BattleGridFightEquipmentInfo
        {
            GridFightEquipmentId = equipment.ItemId,
            UniqueId = equipment.UniqueId
        };
    }
}