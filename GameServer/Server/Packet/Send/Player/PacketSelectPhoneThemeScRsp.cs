using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketSelectPhoneThemeScRsp : BasePacket
{
    public PacketSelectPhoneThemeScRsp(uint themeId) : base(CmdIds.SelectPhoneThemeScRsp)
    {
        var proto = new SelectPhoneThemeScRsp
        {
            CurPhoneTheme = themeId
        };

        SetData(proto);
    }
}