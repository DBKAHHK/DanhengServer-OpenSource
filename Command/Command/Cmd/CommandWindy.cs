using EggLink.DanhengServer.GameServer.Server.Packet.Send.Others;
using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("windy", "Kinda windy today!", "/windy <lua>")]
public class CommandWindy : ICommand
{
    [CommandDefault]
    public async ValueTask Windy(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var filePath = Path.Combine(Environment.CurrentDirectory, "Lua", arg.Raw);
        if (File.Exists(filePath))
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            await arg.Target.SendPacket(new PacketClientDownloadDataScNotify(fileBytes));
            await arg.SendMsg("Read BYTECODE from Lua script: " + filePath.Replace("\\", "/"));
        }
        else
        {
            await arg.SendMsg("Error reading Lua script: " + arg.Raw.Replace("\\", "/"));
        }
    }
}