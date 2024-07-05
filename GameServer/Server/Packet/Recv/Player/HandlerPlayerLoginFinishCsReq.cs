using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Server.Packet.Send.Mission;
using EggLink.DanhengServer.Server.Packet.Send.Others;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player
{
    [Opcode(CmdIds.PlayerLoginFinishCsReq)]
    public class HandlerPlayerLoginFinishCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            connection.SendPacket(CmdIds.PlayerLoginFinishScRsp);
            //var list = connection.Player!.MissionManager!.GetRunningSubMissionIdList();
            //connection.SendPacket(new PacketMissionAcceptScNotify(list));

            // DO NOT REMOVE THIS CODE
            // This code is responsible for sending the client data to the player
            connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gdmVyc2lvbl90ZXh0KCkNCiAgICBsb2NhbCB1aWQgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlZlcnNpb25UZXh0Iik6R2V0Q29tcG9uZW50KCJUZXh0IikNCiAgICBpZiBub3Qgc3RyaW5nLm1hdGNoKHVpZC50ZXh0LCAi5q2k5pyN5Yqh56uvIikgdGhlbg0KICAgICAgICB1aWQudGV4dCA9ICLmraTmnI3liqHnq6/ku4XnlKjkvZzlrabkuaDkuqTmtYHvvIzor7fmlK/mjIHmraPniYjmuLjmiI9cbiIgLi4gdWlkLnRleHQNCiAgICAgICAgdWlkLmZvbnRTaXplID0gNzYuMA0KICAgIGVuZA0KICAgIGxvY2FsIGJldGEgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKTpHZXRDb21wb25lbnQoIlRleHQiKQ0KZW5kDQoNCnZlcnNpb25fdGV4dCgp")));
        }
    }
}
