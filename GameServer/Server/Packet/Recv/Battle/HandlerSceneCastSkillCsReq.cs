using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Game.Battle.Skill;
using EggLink.DanhengServer.Game.Battle.Skill.Action;
using EggLink.DanhengServer.Game.Scene;
using EggLink.DanhengServer.Game.Scene.Entity;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Server.Packet.Recv.Battle
{
    [Opcode(CmdIds.SceneCastSkillCsReq)]
    public class HandlerSceneCastSkillCsReq : Handler
    {
        public override void OnHandle(Connection connection, byte[] header, byte[] data)
        {
            var req = SceneCastSkillCsReq.Parser.ParseFrom(data);
            if (req != null)
            {
                var scene = connection.Player!.SceneInstance!;
                scene.AvatarInfo.TryGetValue((int)req.AttackedByEntityId, out var info);
                MazeSkill mazeSkill = new([]);

                bool triggerBattle = true;
                if (info != null)  // cast by player
                {
                    mazeSkill = MazeSkillManager.GetSkill(info.AvatarInfo.GetAvatarId(), (int)req.SkillIndex);
                } else
                {
                    // monster
                    foreach (var id in req.AssistMonsterEntityIdList)
                    {
                        if (scene.Entities.TryGetValue((int)id, out var entity))
                        {
                            if (entity is EntityMonster || entity is EntityProp)  // avoid monster hit monster
                            {
                                triggerBattle = false;
                                break;
                            } 
                        }
                    }
                }

                if (req.AssistMonsterEntityIdList.Count == 0)
                {
                    triggerBattle = false;
                }

                if (!triggerBattle)
                {
                    // didnt hit any target
                    if (info != null && req.SkillIndex > 0)
                    {
                        mazeSkill.OnCast(info);
                    }
                    connection.SendPacket(new PacketSceneCastSkillScRsp(req.CastEntityId));
                }
                else
                {
                    connection.Player!.BattleManager!.StartBattle(req, mazeSkill);
                }
            }
        }
    }
}
