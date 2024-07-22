using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("mail", "Game.Command.Mail.Desc", "Game.Command.Mail.Usage", permission: "")]
public class CommandMail : ICommand
{
    [CommandDefault]
    public async ValueTask Mail(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.Args.Count < 5)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var sender = arg.Args[0];
        var title = arg.Args[1];
        var content = arg.Args[2];
        var templateId = int.Parse(arg.Args[3]);
        var expiredDay = int.Parse(arg.Args[4]);

        await arg.Target.Player!.MailManager!.SendMail(sender, title, content, templateId, expiredDay);

        await arg.SendMsg(I18nManager.Translate("Game.Command.Mail.MailSent"));
    }
}