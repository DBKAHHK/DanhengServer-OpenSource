using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Command.Cmd;

[CommandInfo("kick", "Game.Command.Kick.Desc", "Game.Command.Kick.Usage", permission: "egglink.manage")]
public class CommandKick : ICommand
{
    [CommandDefault]
    public async ValueTask Kick(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        await arg.Target.SendPacket(new PacketPlayerKickOutScNotify());
        await arg.SendMsg(I18nManager.Translate("Game.Command.Kick.PlayerKicked", arg.Target.Player!.Data.Name!));
        arg.Target.Stop();
    }
}