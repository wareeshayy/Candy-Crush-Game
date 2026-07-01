using UnityEngine;

namespace CandyCrush.Core
{
    public enum CandyType
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5,
        Cyan = 6
    }

    public enum SpecialCandyType
    {
        None,
        StripedHorizontal,
        StripedVertical,
        Wrapped,
        ColorBomb
    }

    public enum Difficulty
    {
        Easy,
        Hard
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory
    }
}
