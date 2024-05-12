using System.Collections.Generic;
using System.Text;

namespace EggLink.DanhengServer.Command.Cmd
{
    [CommandInfo("mission", "获取正在进行的任务或完成任务", "/mission <finish [submissionId]>/<running>/<reaccept [missionId]>")]
    public class CommandMission : ICommand
    {
        [CommandMethod("0 pass")]
        public void PassRunningMission(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("未找到玩家。");
                return;
            }
            var mission = arg.Target!.Player!.MissionManager!;
            mission.GetRunningSubMissionIdList().ForEach(mission.FinishSubMission);
            arg.SendMsg("已完成所有正在进行的任务");
        }

        [CommandMethod("0 finish")]
        public void FinishRunningMission(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("未找到玩家。");
                return;
            }

            if (arg.BasicArgs.Count < 1)
            {
                arg.SendMsg("请输入任务ID");
                return;
            }

            if (!int.TryParse(arg.BasicArgs[0], out var missionId))
            {
                arg.SendMsg("无效的任务ID");
                return;
            }

            var mission = arg.Target!.Player!.MissionManager!;
            mission.FinishSubMission(missionId);
            arg.SendMsg("任务已完成");
        }

        [CommandMethod("0 running")]
        public void ListRunningMission(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("未找到玩家");
                return;
            }

            var mission = arg.Target!.Player!.MissionManager!;
            var runningMissions = mission.GetRunningSubMissionList();
            if (runningMissions.Count == 0)
            {
                arg.SendMsg("没有正在进行的任务");
                return;
            }

            arg.SendMsg("正在进行的任务：");
            Dictionary<int, List<int>> missionMap = [];

            foreach (var m in runningMissions)
            {
                if (!missionMap.TryGetValue(m.MainMissionID, out List<int>? value))
                {
                    value = [];
                    missionMap[m.MainMissionID] = value;
                }

                value.Add(m.ID);
            }

            var possibleStuckIds = new List<int>();
            var morePossibleStuckIds = new List<int>();

            foreach (var list in missionMap)
            {
                arg.SendMsg($"主任务 {list.Key}：");
                var sb = new StringBuilder();
                foreach (var id in list.Value)
                {
                    sb.Append($"{id}、");

                    if (id.ToString().StartsWith("10"))
                    {
                        possibleStuckIds.Add(id);

                        var info = mission.GetSubMissionInfo(id);
                        if (info != null && info.FinishType == Enums.MissionFinishTypeEnum.PropState)
                        {
                            morePossibleStuckIds.Add(id);
                        }
                    }
                }

                sb.Remove(sb.Length - 1, 1);

                arg.SendMsg(sb.ToString());
            }

            if (morePossibleStuckIds.Count > 0)
            {
                arg.SendMsg("可能被卡住的任务ID如下：");

                var sb = new StringBuilder();
                foreach (var id in morePossibleStuckIds)
                {
                    sb.Append($"{id}、");
                }

                sb.Remove(sb.Length - 1, 1);

                arg.SendMsg(sb.ToString());
            }
            else if (possibleStuckIds.Count > 0)
            {
                arg.SendMsg("可能被卡住的任务ID如下：");

                var sb = new StringBuilder();
                foreach (var id in possibleStuckIds)
                {
                    sb.Append($"{id}、");
                }

                sb.Remove(sb.Length - 1, 1);

                arg.SendMsg(sb.ToString());
            }
        }

        [CommandMethod("0 reaccept")]
        public void ReAcceptMission(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("玩家不存在");
                return;
            }

            if (arg.BasicArgs.Count < 1)
            {
                arg.SendMsg("请输入主任务ID");
                return;
            }

            if (!int.TryParse(arg.BasicArgs[0], out var missionId))
            {
                arg.SendMsg("无效的任务ID");
                return;
            }

            var mission = arg.Target!.Player!.MissionManager!;
            mission.ReAcceptMainMission(missionId);
            arg.SendMsg($"已重新接取ID为 {missionId} 的任务");
        }
    }
}
