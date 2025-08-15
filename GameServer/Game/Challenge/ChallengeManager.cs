using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Challenge;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.GameServer.Game.Challenge.Definitions;
using EggLink.DanhengServer.GameServer.Game.Challenge.Instances;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Challenge;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;
using Google.Protobuf;
using static EggLink.DanhengServer.GameServer.Plugin.Event.PluginEvent;

namespace EggLink.DanhengServer.GameServer.Game.Challenge;

public class ChallengeManager(PlayerInstance player) : BasePlayerManager(player)
{
    #region Properties

    public BaseChallengeInstance? ChallengeInstance { get; set; }

    public ChallengeData ChallengeData { get; } =
        DatabaseHelper.Instance!.GetInstanceOrCreateNew<ChallengeData>(player.Uid);

    #endregion

    #region Management

    public async ValueTask StartChallenge(int challengeId, ChallengeStoryBuffInfo? storyBuffs,
        ChallengeBossBuffInfo? bossBuffs)
    {
        // Get challenge excel
        if (!GameData.ChallengeConfigData.TryGetValue(challengeId, out var value))
        {
            await Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeNotExist));
            return;
        }

        var excel = value;

        // Sanity check lineups
        if (excel.StageNum > 0)
        {
            // Get lineup
            var lineup = Player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge)!;

            // Make sure this lineup has avatars set
            if (lineup.AvatarData!.FormalAvatars.Count == 0)
            {
                await Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeLineupEmpty));
                return;
            }

            // Reset hp/sp
            foreach (var avatar in lineup.AvatarData!.FormalAvatars)
            {
                avatar.SetCurHp(10000, true);
                avatar.SetCurSp(5000, true);
            }

            // Set technique points to full
            lineup.Mp = 5; // Max Mp
        }

        if (excel.StageNum >= 2)
        {
            // Get lineup
            var lineup = Player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge2)!;

            // Make sure this lineup has avatars set
            if (lineup.AvatarData!.FormalAvatars.Count == 0)
            {
                await Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeLineupEmpty));
                return;
            }

            // Reset hp/sp
            foreach (var avatar in lineup.AvatarData!.FormalAvatars)
            {
                avatar.SetCurHp(10000, true);
                avatar.SetCurSp(5000, true);
            }

            // Set technique points to full
            lineup.Mp = 5; // Max Mp
        }

        // Set challenge data for player
        var data = new ChallengeDataPb();
        BaseLegacyChallengeInstance instance;

        // Set challenge type
        if (excel.IsBoss())
        {
            data.Boss = new ChallengeBossDataPb
            {
                ChallengeMazeId = (uint)excel.ID,
                CurStatus = 1,
                CurrentStage = 1,
                CurrentExtraLineup = ChallengeLineupTypePb.Challenge1
            };

            instance = new ChallengeBossInstance(Player, data);
        }
        else if (excel.IsStory())
        {
            data.Story = new ChallengeStoryDataPb
            {
                ChallengeMazeId = (uint)excel.ID,
                CurStatus = 1,
                CurrentStage = 1,
                CurrentExtraLineup = ChallengeLineupTypePb.Challenge1
            };

            instance = new ChallengeStoryInstance(Player, data);
        }
        else
        {
            data.Memory = new ChallengeMemoryDataPb
            {
                ChallengeMazeId = (uint)excel.ID,
                CurStatus = 1,
                CurrentStage = 1,
                CurrentExtraLineup = ChallengeLineupTypePb.Challenge1,
                RoundsLeft = (uint)excel.ChallengeCountDown
            };

            instance = new ChallengeMemoryInstance(Player, data);
        }

        ChallengeInstance = instance;

        // Set first lineup before we enter scenes
        await Player.LineupManager!.SetCurLineup(instance.GetCurrentExtraLineupType() + 10);

        // Enter scene
        try
        {
            await Player.EnterScene(excel.MapEntranceID, 0, false);
        }
        catch
        {
            // Reset lineup/instance if entering scene failed
            ChallengeInstance = null;

            // Send error packet
            await Player.SendPacket(new PacketStartChallengeScRsp((uint)Retcode.RetChallengeNotExist));
            return;
        }

        // Save start positions
        instance.SetStartPos(Player.Data.Pos!);
        instance.SetStartRot(Player.Data.Rot!);
        instance.SetSavedMp(Player.LineupManager.GetCurLineup()!.Mp);

        if (excel.IsStory() && storyBuffs != null)
        {
            instance.Data.Story.Buffs.Add(storyBuffs.BuffOne);
            instance.Data.Story.Buffs.Add(storyBuffs.BuffTwo);
        }

        if (bossBuffs != null)
        {
            instance.Data.Boss.Buffs.Add(bossBuffs.BuffOne);
            instance.Data.Boss.Buffs.Add(bossBuffs.BuffTwo);
        }

        InvokeOnPlayerEnterChallenge(Player, instance);

        // Send packet
        await Player.SendPacket(new PacketStartChallengeScRsp(Player));

        // Save instance
        SaveInstance(instance);
    }

    public void AddHistory(int challengeId, int stars, int score)
    {
        if (stars <= 0) return;

        if (!ChallengeData.History.ContainsKey(challengeId))
            ChallengeData.History[challengeId] = new ChallengeHistoryData(Player.Uid, challengeId);
        var info = ChallengeData.History[challengeId];

        // Set
        info.SetStars(stars);
        info.Score = score;
    }

    public async ValueTask<List<TakenChallengeRewardInfo>?> TakeRewards(int groupId)
    {
        // Get excels
        if (!GameData.ChallengeGroupData.ContainsKey(groupId)) return null;
        var challengeGroup = GameData.ChallengeGroupData[groupId];

        if (!GameData.ChallengeRewardData.ContainsKey(challengeGroup.RewardLineGroupID)) return null;
        var challengeRewardLine = GameData.ChallengeRewardData[challengeGroup.RewardLineGroupID];

        // Get total stars
        var totalStars = 0;
        foreach (var ch in ChallengeData.History.Values)
        {
            // Legacy compatibility
            if (ch.GroupId == 0)
            {
                if (!GameData.ChallengeConfigData.ContainsKey(ch.ChallengeId)) continue;
                var challengeExcel = GameData.ChallengeConfigData[ch.ChallengeId];

                ch.GroupId = challengeExcel.GroupID;
            }

            // Add total stars
            if (ch.GroupId == groupId) totalStars += ch.GetTotalStars();
        }

        // Rewards
        var rewardInfos = new List<TakenChallengeRewardInfo>();
        var data = new List<ItemData>();

        // Get challenge rewards
        foreach (var challengeReward in challengeRewardLine)
        {
            // Check if we have enough stars to take this reward
            if (totalStars < challengeReward.StarCount) continue;

            // Get reward info
            if (!ChallengeData.TakenRewards.ContainsKey(groupId))
                ChallengeData.TakenRewards[groupId] = new ChallengeGroupReward(Player.Uid, groupId);
            var reward = ChallengeData.TakenRewards[groupId];

            // Check if reward has been taken
            if (reward.HasTakenReward(challengeReward.StarCount)) continue;

            // Set reward as taken
            reward.SetTakenReward(challengeReward.StarCount);

            // Get reward excel
            if (!GameData.RewardDataData.ContainsKey(challengeReward.RewardID)) continue;
            var rewardExcel = GameData.RewardDataData[challengeReward.RewardID];

            // Add rewards
            var proto = new TakenChallengeRewardInfo
            {
                StarCount = (uint)challengeReward.StarCount,
                Reward = new ItemList()
            };

            foreach (var item in rewardExcel.GetItems())
            {
                var itemData = new ItemData
                {
                    ItemId = item.Item1,
                    Count = item.Item2
                };

                proto.Reward.ItemList_.Add(itemData.ToProto());
                data.Add(itemData);
            }

            rewardInfos.Add(proto);
        }

        // Add items to inventory
        await Player.InventoryManager!.AddItems(data);
        return rewardInfos;
    }

    public void SaveInstance(BaseChallengeInstance instance)
    {
        ChallengeData.ChallengeInstance = Convert.ToBase64String(instance.Data.ToByteArray());
    }

    public void ClearInstance()
    {
        ChallengeData.ChallengeInstance = null;
        ChallengeInstance = null;
    }

    public void ResurrectInstance()
    {
        if (ChallengeData.ChallengeInstance == null) return;
        var protoByte = Convert.FromBase64String(ChallengeData.ChallengeInstance);
        var proto = ChallengeDataPb.Parser.ParseFrom(protoByte);

        if (proto != null)
        {
            ChallengeInstance = proto.ChallengeTypeCase switch
            {
                ChallengeDataPb.ChallengeTypeOneofCase.Memory => new ChallengeMemoryInstance(Player, proto),
                ChallengeDataPb.ChallengeTypeOneofCase.Peak => new ChallengePeakInstance(Player, proto),
                ChallengeDataPb.ChallengeTypeOneofCase.Story => new ChallengeStoryInstance(Player, proto),
                ChallengeDataPb.ChallengeTypeOneofCase.Boss => new ChallengeBossInstance(Player, proto),
                _ => null
            };
        }
        else
        {
            ChallengeData.ChallengeInstance = null;
        }
    }

    #endregion
}

// WatchAndyTW was here