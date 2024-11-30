using EggLink.DanhengServer.GameServer.Game.MatchThree.Member;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MatchThree;

public class MatchThreeRoomInstance(int roomId, FightGameMode mode)
{
    public List<MatchThreeMemberInstance> Members { get; set; } = [];
    public int RoomId { get; set; } = roomId;
    public FightGameMode GameMode { get; set; } = mode;
    public bool InMatch { get; set; }

    public async ValueTask AddMember(MatchThreeMemberInstance member)
    {
        Members.Add(member);

        if (Members.Count == 1) // reroll a leader
        {
            Members.First().CharacterType = LobbyCharacterType.LobbyCharacterLeader;
        }

        await SyncInfo(LobbyModifyType.JoinLobby);
    }

    public async ValueTask RemoveMember(MatchThreeMemberInstance member)
    {
        Members.Remove(member);

        if (member.CharacterType == LobbyCharacterType.LobbyCharacterLeader) // reroll a leader
        {
            if (Members.Count != 0)
            {
                Members.First().CharacterType = LobbyCharacterType.LobbyCharacterLeader;
            }
        }

        await SyncInfo(LobbyModifyType.QuitLobby);
    }

    public async ValueTask AddMember(PlayerInstance player, int birdId, LobbyCharacterType characterType)
    {
        await AddMember(new MatchThreeMemberInstance(player, birdId, characterType, this));
    }

    public MatchThreeMemberInstance? GetMemberByUid(int uid)
    {
        return Members.Find(member => member.Player.Uid == uid);
    }

    public async ValueTask ModifyPlayerInfo(PlayerInstance player, PlayerExtraInfo extraInfo, LobbyModifyType modifyType)
    {
        var member = GetMemberByUid(player.Uid);
        if (member == null) return;

        member.Bird.BirdId = (int)extraInfo.GameBirdInfo.BirdId;

        member.Status = modifyType == LobbyModifyType.Operating ? LobbyCharacterStatus.Operating : LobbyCharacterStatus.Idle;

        await SyncInfo(modifyType);
    }

    public async ValueTask StartMatch()
    {
        InMatch = true;

        foreach (var member in Members)
        {
            member.Status = LobbyCharacterStatus.Matching;
        }

        await SyncInfo(LobbyModifyType.Match);

        if (!MatchThreeService.MatchingThread.IsAlive)
            MatchThreeService.MatchingThread.Start();
    }

    public async ValueTask CancelMatch()
    {
        InMatch = false;

        foreach (var member in Members)
        {
            member.Status = LobbyCharacterStatus.Idle;
        }

        await SyncInfo(LobbyModifyType.CancelMatch);
    }

    public async ValueTask DestroyRoom(LobbyModifyType type)
    {
        InMatch = false;

        foreach (var member in Members)
        {
            member.Status = LobbyCharacterStatus.Fighting;
        }

        await SyncInfo(type);
    }

    public async ValueTask SyncInfo(LobbyModifyType modifyType = LobbyModifyType.None)
    {
        foreach (var member in Members)
        {
            await member.Player.SendPacket(new PacketLobbySyncInfoScNotify(this, member.Player.Uid, modifyType));
        }
    }
}