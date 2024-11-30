using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util.Security;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Fight;

public class PacketFightEnterScRsp : BasePacket
{
    public PacketFightEnterScRsp(Connection connection) : base(CmdIds.FightEnterScRsp)
    {
        var proto = new FightEnterScRsp
        {
            ServerTimestampMs = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            LJMFOHLOBCI = true,
            KMANPJCMAOB = 1
        };

        if (ConfigManager.Config.GameServer.UsePacketEncryption)
        {
            var tempRandom = new MT19937((ulong)DateTimeOffset.Now.ToUnixTimeSeconds());
            proto.SecretKeySeed = connection.ClientSecretKeySeed = tempRandom.NextUInt64();
        }

        SetData(proto);
    }
}