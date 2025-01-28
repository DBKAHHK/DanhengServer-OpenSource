using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("gender", "Game.Command.Gender.Desc", "Game.Command.Gender.Usage")]
public class CommandGender : ICommand
{
    [CommandDefault]
    public async ValueTask ChangeGender(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 1)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var gender = (Gender)arg.GetInt(0);
        if (gender == Gender.None)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Gender.GenderNotSpecified"));
            return;
        }

        var player = arg.Target!.Player!;
        player.Data.CurrentGender = gender;
        await player.ChangeAvatarPathType(8001, gender == Gender.Man ?
            MultiPathAvatarType.BoyWarriorType : MultiPathAvatarType.GirlKnightType);
        await player.SendPacket(new PacketGetMultiPathAvatarInfoScRsp(player));

        await arg.SendMsg(I18NManager.Translate("Game.Command.Gender.GenderChanged"));
    }
}