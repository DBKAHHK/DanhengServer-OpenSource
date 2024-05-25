using EggLink.DanhengServer.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Command.Cmd
{
    [CommandInfo("heal", "Heal all avatars on current team", "heal")]
    public class CommandHeal : ICommand
    {
        [CommandDefault]
        public void Help(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("Player not found");
                return;
            }

            var player = arg.Target.Player!;
            foreach (var avatar in player.LineupManager!.GetCurLineup()!.AvatarData!.Avatars)
            {
                avatar.CurrentHp = 10000;
            }
            player.SceneInstance!.SyncLineup();
            arg.SendMsg("Successfully healed all avatars on current team");
        }
    }
}
