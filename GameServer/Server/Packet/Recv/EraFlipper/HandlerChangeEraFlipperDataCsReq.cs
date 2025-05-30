using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Mission;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Task;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipper;

[Opcode(CmdIds.ChangeEraFlipperDataCsReq)]
public class HandlerChangeEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ChangeEraFlipperDataCsReq.Parser.ParseFrom(data);

        var floorId = connection.Player!.SceneInstance!.FloorId;

        if (connection.Player.SceneInstance.FloorInfo?.FloorSavedValue.Find(x => x.Name == "FSV_FlashBackCount") != null)
        {
            // should save
            var plane = connection.Player.SceneInstance.PlaneId;
            var floor = connection.Player.SceneInstance.FloorId;
            connection.Player.SceneData!.FloorSavedData.TryGetValue(floor, out var value);
            if (value == null)
            {
                value = [];
                connection.Player.SceneData.FloorSavedData[floor] = value;
            }

            value["FSV_FlashBackCount"] = 0;
            value["FSV_FlashBackCount"] = value.GetValueOrDefault("FSV_FlashBackCount", 0) + 1; // ParamString[2] is the key
            await connection.SendPacket(new PacketUpdateFloorSavedValueNotify("FSV_FlashBackCount", value["FSV_FlashBackCount"], connection.Player));

            connection.Player.TaskManager?.SceneTaskTrigger.TriggerFloor(plane, floor);
            connection.Player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.FloorSavedValue);
        }

        await connection.SendPacket(new PacketChangeEraFlipperDataScRsp(req));
        //await connection.SendPacket(new PacketEraFlipperDataChangeScNotify(req, floorId));
    }
}