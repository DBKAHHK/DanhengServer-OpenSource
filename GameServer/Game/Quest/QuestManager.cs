using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Database.Quests;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.Enums.Quest;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Quest;

public class QuestManager(PlayerInstance player) : BasePlayerManager(player)
{
    public UnlockHandler UnlockHandler { get; } = new(player);
    public QuestData QuestData { get; } = DatabaseHelper.Instance!.GetInstanceOrCreateNew<QuestData>(player.Uid);

    #region Actions

    public async ValueTask AcceptQuestByCondition()
    {
        foreach (var quest in GameData.QuestDataData.Values)
        {
            if (QuestData.Quests.ContainsKey(quest.QuestID)) continue; // Already accepted
            switch (quest.UnlockType)
            {
                case QuestUnlockTypeEnum.AutoUnlock:
                    await AcceptQuest(quest.QuestID);
                    break;
                case QuestUnlockTypeEnum.FinishMission:
                    var accept = true;

                    foreach (var missionId in quest.UnlockParamList)
                        if (Player.MissionManager!.GetMainMissionStatus(missionId) != MissionPhaseEnum.Finish)
                        {
                            accept = false;
                            break;
                        }

                    if (accept) await AcceptQuest(quest.QuestID);
                    break;
                case QuestUnlockTypeEnum.FinishQuest:
                    var accept2 = true;

                    foreach (var questId in quest.UnlockParamList)
                        if (GetQuestStatus(questId) != QuestStatus.QuestFinish &&
                            GetQuestStatus(questId) != QuestStatus.QuestClose)
                        {
                            accept2 = false;
                            break;
                        }

                    if (accept2) await AcceptQuest(quest.QuestID);
                    break;
                case QuestUnlockTypeEnum.ManualUnlock: // idk what this is
                    break;
                case QuestUnlockTypeEnum.BattlePassWeekly:
                case QuestUnlockTypeEnum.Unknown:
                default:
                    break;
            }
        }
    }

    public async ValueTask AcceptQuest(int questId)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return;

        if (QuestData.Quests.ContainsKey(questId)) return;

        var questInfo = new QuestInfo
        {
            QuestId = questId,
            QuestStatus = QuestStatus.QuestDoing,
            Progress = 0
        };

        QuestData.Quests.Add(questId, questInfo);

        await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));
    }

    public async ValueTask FinishQuest(int questId)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return;
        GameData.FinishWayData.TryGetValue(questExcel.FinishWayID, out var finishWayExcel);
        if (finishWayExcel == null) return;

        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return;
        if (questInfo.QuestStatus != QuestStatus.QuestDoing) return;

        questInfo.QuestStatus = QuestStatus.QuestFinish;
        questInfo.Progress = finishWayExcel.Progress;
        await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));

        // accept next quest
        await AcceptQuestByCondition();
    }

    public async ValueTask<Retcode> FinishQuestByClient(int questId)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return Retcode.RetFail;

        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return Retcode.RetQuestNotAccept;
        if (questInfo.QuestStatus != QuestStatus.QuestDoing) return Retcode.RetQuestStatusError;

        questInfo.QuestStatus = QuestStatus.QuestFinish;
        await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));

        // accept next quest
        await AcceptQuestByCondition();

        return Retcode.RetSucc;
    }

    public async ValueTask<(Retcode, List<ItemData>?)> TakeQuestReward(int questId)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return (Retcode.RetFail, null);

        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return (Retcode.RetQuestNotAccept, null);
        if (questInfo.QuestStatus != QuestStatus.QuestFinish) return (Retcode.RetQuestNotFinish, null);

        questInfo.QuestStatus = QuestStatus.QuestClose; // Close the quest after taking the reward

        // handle reward
        var items = await Player.InventoryManager!.HandleReward(questExcel.RewardID);

        await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));

        return (Retcode.RetSucc, items);
    }

    public async ValueTask AddQuestProgress(int questId, int progress)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return;
        GameData.FinishWayData.TryGetValue(questExcel.FinishWayID, out var finishWayExcel);
        if (finishWayExcel == null) return;

        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return;
        if (questInfo.QuestStatus != QuestStatus.QuestDoing) return;

        questInfo.Progress += progress;
        if (questInfo.Progress >= finishWayExcel.Progress)
            await FinishQuest(questId);
        else
            await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));
    }

    public async ValueTask UpdateQuestProgress(int questId, int progress)
    {
        GameData.QuestDataData.TryGetValue(questId, out var questExcel);
        if (questExcel == null) return;
        GameData.FinishWayData.TryGetValue(questExcel.FinishWayID, out var finishWayExcel);
        if (finishWayExcel == null) return;

        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return;
        if (questInfo.QuestStatus != QuestStatus.QuestDoing) return;

        if (progress < questInfo.Progress) return; // prevent rollback
        if (progress == questInfo.Progress) return;
        questInfo.Progress = progress;
        if (questInfo.Progress >= finishWayExcel.Progress)
            await FinishQuest(questId);
        else
            await Player.SendPacket(new PacketPlayerSyncScNotify(questInfo));
    }

    #endregion

    #region Information

    public QuestStatus GetQuestStatus(int questId)
    {
        if (!QuestData.Quests.TryGetValue(questId, out var questInfo)) return QuestStatus.QuestNone;
        return questInfo.QuestStatus;
    }

    public List<QuestInfo> GetRunningQuest()
    {
        return QuestData.Quests.Values.Where(x => x.QuestStatus == QuestStatus.QuestDoing).ToList();
    }

    #endregion
}