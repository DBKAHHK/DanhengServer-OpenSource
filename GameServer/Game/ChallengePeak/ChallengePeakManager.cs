using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Challenge;
using EggLink.DanhengServer.Database.Lineup;
using EggLink.DanhengServer.GameServer.Game.Challenge.Definitions;
using EggLink.DanhengServer.GameServer.Game.Challenge.Instances;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Challenge;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.ChallengePeak;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;
using System.Drawing.Drawing2D;
using EggLink.DanhengServer.Util;
using ChallengePeakInfo = EggLink.DanhengServer.Proto.ChallengePeakInfo;
using ChallengePeakLevelInfo = EggLink.DanhengServer.Proto.ChallengePeakLevelInfo;

namespace EggLink.DanhengServer.GameServer.Game.ChallengePeak;

/// <summary>
/// this class is used to manage the challenge peak for a player
/// but the challenge instance shouldnt be stored here ( see ChallengeManager )
/// </summary>
/// <see cref="EggLink.DanhengServer.GameServer.Game.Challenge.ChallengeManager"/>
public class ChallengePeakManager(PlayerInstance player) : BasePlayerManager(player)
{
    public ChallengePeakInfo GetChallengePeakInfo(int groupId)
    {
        var proto = new ChallengePeakInfo
        {
            CurPeakGroupId = (uint)groupId,
        };

        var data = GameData.ChallengePeakGroupConfigData.GetValueOrDefault(groupId);
        if (data == null) return proto;

        foreach (var levelId in data.PreLevelIDList)
        {
            var levelData = GameData.ChallengePeakConfigData.GetValueOrDefault(levelId);
            if (levelData == null) continue;

            var levelProto = new ChallengePeakLevelInfo
            {
                PeakLevelId = (uint)levelId,
                IsRead = true
            };

            if (Player.ChallengeManager!.ChallengeData.PeakLevelDatas.TryGetValue(levelId, out var levelPbData))
            {
                levelProto.PeakStar = levelPbData.PeakStar;
                levelProto.PeakLevelLineup.AddRange(levelPbData.BaseAvatarList);
                foreach (var avatarId in levelPbData.BaseAvatarList)
                {
                    var avatar = Player.AvatarManager!.GetFormalAvatar((int)avatarId);
                    if (avatar == null) continue;

                    levelProto.PeakAvatarInfoList.Add(avatar.ToPeakAvatarProto());
                    proto.PeakAvatarInfoList.Add(avatar.ToPeakAvatarProto());
                }
            }

            proto.PeakLevelInfoList.Add(levelProto);
        }

        // boss
        var bossLevelId = data.BossLevelID;
        if (bossLevelId <= 0) return proto;
        
        var bossLevelData = GameData.ChallengePeakBossConfigData.GetValueOrDefault(bossLevelId);
        if (bossLevelData == null) return proto;

        var bossProto = new ChallengePeakLevelInfo
        {
            PeakLevelId = (uint)bossLevelId,
            IsRead = true
        };

        if (Player.ChallengeManager!.ChallengeData.PeakLevelDatas.TryGetValue(bossLevelId, out var bossPbData))
        {
            bossProto.PeakStar = bossPbData.PeakStar;
            bossProto.PeakLevelLineup.AddRange(bossPbData.BaseAvatarList);
            foreach (var avatarId in bossPbData.BaseAvatarList)
            {
                var avatar = Player.AvatarManager!.GetFormalAvatar((int)avatarId);
                if (avatar == null) continue;
                bossProto.PeakAvatarInfoList.Add(avatar.ToPeakAvatarProto());
                proto.PeakAvatarInfoList.Add(avatar.ToPeakAvatarProto());
            }
        }

        proto.PeakLevelInfoList.Add(bossProto);

        return proto;
    }

    public async ValueTask SetLineupAvatars(int groupId, List<ChallengePeakLineup> lineups)
    {
        var datas = Player.ChallengeManager!.ChallengeData.PeakLevelDatas;
        foreach (var lineup in lineups)
        {
            if (!datas.TryGetValue((int)lineup.PeakLevelId,
                    out var data))
            {
                datas[(int)lineup.PeakLevelId] = new ChallengePeakLevelData
                {
                    LevelId = (int)lineup.PeakLevelId,
                    BaseAvatarList = lineup.PeakLevelLineup.ToList()
                };
            }
            else
            {
                data.BaseAvatarList = lineup.PeakLevelLineup.ToList();
            }
        }

        await Player.SendPacket(new PacketChallengePeakGroupDataUpdateScNotify(GetChallengePeakInfo(groupId)));
    }

    public async ValueTask StartChallenge(int levelId, uint buffId, List<int> avatarIdList)
    {
        // Get challenge excel
        if (!GameData.ChallengePeakConfigData.TryGetValue(levelId, out var excel))
        {
            await Player.SendPacket(new PacketStartChallengePeakScRsp(Retcode.RetChallengeNotExist));
            return;
        }

        // Format to base avatar id
        List<int> avatarIds = [];
        foreach (var avatarId in avatarIdList)
        {
            var avatar = Player.AvatarManager!.GetFormalAvatar(avatarId);
            if (avatar != null)
                avatarIds.Add(avatar.BaseAvatarId);
        }

        // Get lineup
        var lineup = Player.LineupManager!.GetExtraLineup(ExtraLineupType.LineupChallenge)!;
        if (avatarIds.Count > 0)
        {
            lineup.BaseAvatars = avatarIds.Select(x => new LineupAvatarInfo
            {
                BaseAvatarId = x
            }).ToList();
        }
        else
        {
            lineup.BaseAvatars = Player.ChallengeManager!.ChallengeData.PeakLevelDatas.GetValueOrDefault(levelId)
                ?.BaseAvatarList
                .Select(x => new LineupAvatarInfo
                {
                    BaseAvatarId = (int)x
                }).ToList() ?? [];
        }

        // Set technique points to full
        lineup.Mp = 5; // Max Mp

        // Make sure this lineup has avatars set
        if (Player.AvatarManager!.AvatarData!.FormalAvatars.Count == 0)
        {
            await Player.SendPacket(new PacketStartChallengePeakScRsp(Retcode.RetChallengeLineupEmpty));
            return;
        }

        // Reset hp/sp
        foreach (var avatar in Player.AvatarManager!.AvatarData.FormalAvatars)
        {
            avatar.SetCurHp(10000, true);
            avatar.SetCurSp(5000, true);
        }

        // Set challenge data for player
        var data = new ChallengeDataPb
        {
            Peak = new ChallengePeakDataPb
            {
                CurrentPeakLevelId = (uint)levelId,
                CurrentExtraLineup = ChallengeLineupTypePb.Challenge1,
                CurStatus = 1
            }
        };

        if (buffId > 0)
        {
            data.Peak.Buffs.Add(buffId);
        }

        var instance = new ChallengePeakInstance(Player, data);

        Player.ChallengeManager!.ChallengeInstance = instance;

        // Set first lineup before we enter scenes
        await Player.LineupManager!.SetCurLineup((int)instance.Data.Peak.CurrentExtraLineup + 10);

        // Enter scene
        try
        {
            await Player.EnterScene(excel.MapEntranceID, 0, true);
        }
        catch
        {
            // Reset lineup/instance if entering scene failed
            Player.ChallengeManager!.ChallengeInstance = null;

            // Send error packet
            await Player.SendPacket(new PacketStartChallengePeakScRsp(Retcode.RetChallengeNotExist));
            return;
        }

        // Save start positions
        data.Peak.StartPos = Player.Data.Pos!.ToVector3Pb();
        data.Peak.StartPos = Player.Data.Rot!.ToVector3Pb();
        data.Peak.SavedMp = (uint)Player.LineupManager.GetCurLineup()!.Mp;

        // Send packet
        await Player.SendPacket(new PacketStartChallengePeakScRsp(Retcode.RetSucc));

        // Save instance
        Player.ChallengeManager!.SaveInstance(instance);
    }
}