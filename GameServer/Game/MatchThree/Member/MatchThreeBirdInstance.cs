using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MatchThree.Member;

public class MatchThreeBirdInstance
{
    public int BirdId { get; set; }
    public int Hp { get; set; } = 20;

    public GameBirdInfo ToProto()
    {
        return new GameBirdInfo
        {
            BirdId = (uint)BirdId
        };
    }
}