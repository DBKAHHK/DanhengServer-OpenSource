using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbyCreateScRsp : BasePacket
{
    public PacketLobbyCreateScRsp(MatchThreeRoomInstance instance) : base(CmdIds.LobbyCreateScRsp)
    {
        var proto = new LobbyCreateScRsp
        {
            RoomId = (uint)instance.RoomId,
            MemberInfo = { instance.Members.Select(x => x.ToProto()) },
            FightGameMode = instance.GameMode
        };

        SetData(proto);
    }
}