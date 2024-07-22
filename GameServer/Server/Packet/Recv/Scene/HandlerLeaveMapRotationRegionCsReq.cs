namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.LeaveMapRotationRegionCsReq)]
public class HandlerLeaveMapRotationRegionCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.LeaveMapRotationRegionScRsp);
    }
}