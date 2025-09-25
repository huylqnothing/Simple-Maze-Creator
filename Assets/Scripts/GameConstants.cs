public class GameConstants
{
    public class TileKey
    {
        public static string Empty = "0";
        public static string Wall = "1";
        public static string Npc = "S";
        public static string Destination = "G";
    }

    public class ObserverKey
    {
        public static string OnSolveNotify = "OnSolveNotify";
        public static string OnReplayNotify = "OnReplayNotify";
        public static string OnMapModified = "OnMapModified";
        public static string OnWidthHeightModified = "OnWidthHeightModified";
    }
}