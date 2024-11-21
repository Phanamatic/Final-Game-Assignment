// GameState.cs
public static class GameState
{
    public static bool IsReturningFromGame = false;
    public static bool IsResettingLevel = false;
    public static string LevelToLoad = "";

    // New fields for rejoin functionality
    public static bool WasInGameRoom = false;
    public static string LastRoomName = "";
}
