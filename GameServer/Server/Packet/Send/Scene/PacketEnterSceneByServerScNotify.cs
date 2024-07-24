using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketEnterSceneByServerScNotify : BasePacket
{
    public PacketEnterSceneByServerScNotify(SceneInstance scene,
        ChangeStoryLineAction storyLineAction = ChangeStoryLineAction.None) : base(CmdIds.EnterSceneByServerScNotify)
    {
        var sceneInfo = scene.ToProto();
        var notify = new EnterSceneByServerScNotify
        {
            Scene = sceneInfo,
            Lineup = scene.Player.LineupManager!.GetCurLineup()!.ToProto()
        };

        //if (scene.Player.StoryLineManager?.StoryLineData.CurStoryLineId != 0)
        //{
        //    notify.Scene.BONACBOIIBE = (uint)storyLineAction;
        //}

        SetData(notify);
    }
}