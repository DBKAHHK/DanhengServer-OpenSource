using EggLink.DanhengServer.Server.Packet.Send.Others;
using EggLink.DanhengServer.Server.Packet.Send.Tutorial;
using EggLink.DanhengServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Server.Packet.Recv.Tutorial
{
    [Opcode(CmdIds.GetTutorialCsReq)]
    public class HandlerGetTutorialCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            SendPlayerData(connection);
            if (ConfigManager.Config.ServerOption.EnableMission)  // If missions are enabled
                connection.SendPacket(new PacketGetTutorialScRsp(connection.Player!));
        }

        private void SendPlayerData(Connection connection)
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "Lua", "welcome.lua");
            if (File.Exists(filePath))
            {
                var fileBytes = File.ReadAllBytes(filePath);
                connection.SendPacket(new PacketClientDownloadDataScNotify(fileBytes));
            }

            connection.SendPacket(new PacketClientDownloadDataScNotify(Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gb25EaWFsb2dDbG9zZWQoKQogICAgLS0g5omT5byA5oyH5a6a55qEVVJMCiAgICBDUy5Vbml0eUVuZ2luZS5BcHBsaWNhdGlvbi5PcGVuVVJMKCJodHRwczovL3NyLm1paG95by5jb20vIikKZW5kCgpsb2NhbCBmdW5jdGlvbiBzaG93X2hpbnQoKQogICAgQ1MuUlBHLkNsaWVudC5Db25maXJtRGlhbG9nVXRpbC5TaG93Q3VzdG9tT2tDYW5jZWxIaW50KAogICAgICAgICLmrKLov47kvb/nlKhEYW5oZW5nIFNlcnZlciEgV2VsY29tZSEgXG7kuLnmgZLCt+W0qeWdjzrmmJ/nqbnpk4HpgZNcblRISVMgU0VSVkVSIElTIFRPVEFMTFkgRlJFRSEgSUYgWU9VIFBBWSBGT1IgSVQsIFlPVSBIQVZFIEJFRU4gU0NBTU1FRO+8gVxuVGhpcyBzZXJ2ZXIgaXMgb25seSBmb3IgY29tbXVuaWNhdGluZywgcGxlYXNlIGdvIGZvciBvcmlnaW5hbCBlZGl0aW9uXG7mnKzmnI3liqHlmajlrozlhajlhY3otLnvvIzlpoLmnpzmgqjmmK/otK3kubDlvpfliLDnmoQs6YKj5LmI5oKo5bey57uP6KKr6aqX5LqG77yBXG7mraTmnI3liqHnq6/ku4XnlKjkvZzlrabkuaDkuqTmtYHvvIzor7fmlK/mjIHmraPniYgiLAogICAgICAgIG9uRGlhbG9nQ2xvc2VkCiAgICApCmVuZAoKc2hvd19oaW50KCkK")));
        }
    }
}
