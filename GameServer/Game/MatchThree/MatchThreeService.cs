using EggLink.DanhengServer.GameServer.Game.MatchThree.Member;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Proto;
using System.Collections.Concurrent;

namespace EggLink.DanhengServer.GameServer.Game.MatchThree;

public static class MatchThreeService
{
    public static ConcurrentDictionary<int, MatchThreeRoomInstance> Rooms { get; } = [];
    public static List<MatchThreeGameInstance> GameInstances { get; } = [];
    public static int CurRoomId;
    public static Thread MatchingThread { get; set; } = new(MatchTask);

    /// <summary>
    /// Find a room and join it, or create a new room and join it.
    /// </summary>
    public static async ValueTask<MatchThreeRoomInstance> CreateRoom(PlayerInstance player, int birdId)
    {
        var room = new MatchThreeRoomInstance(++CurRoomId, FightGameMode.Match3);
        Rooms[room.RoomId] = room;

        await room.AddMember(player, birdId, LobbyCharacterType.LobbyCharacterLeader);

        player.MatchThreeManager!.RoomInstance = room;
        return room;
    }

    public static async ValueTask QuitRoom(PlayerInstance player)
    {
        var room = player.MatchThreeManager!.RoomInstance;
        if (room == null) return;

        var member = room.GetMemberByUid(player.Uid);
        if (member == null) return;

        await room.RemoveMember(member);

        if (room.Members.Count == 0)
        {
            Rooms.Remove(room.RoomId, out _);
        }
    }

    public static async void MatchTask()
    {
        // WARNING: DO NOT PERFORM IT IN MAIN THREAD!
        while (true)
        {
            List<MatchThreeMemberInstance> members = [];
            List<MatchThreeRoomInstance> rooms = [];

            foreach (var room in Rooms.Values.Where(x => x.InMatch))
            {
                if (room.Members.Count + members.Count <= 6)
                {
                    members.AddRange(room.Members);

                    rooms.Add(room);
                }

                if (members.Count == 6)
                    break;  // finish match
            }

            if (members.Count != 1) continue;
            // matched
            var inst = new MatchThreeGameInstance(FightGameMode.Match3)
            {
                Members = members
            };
            GameInstances.Add(inst);

            foreach (var room in rooms)
            {
                await room.DestroyRoom(LobbyModifyType.FightStart);
            }

            await inst.SyncMatchedResult();
        }
    }
}