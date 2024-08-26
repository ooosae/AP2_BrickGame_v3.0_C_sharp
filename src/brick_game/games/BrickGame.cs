namespace games;

public abstract class BrickGame
{
    public enum UserAction { Start, Pause, Terminate, Left, Right, Up, Down, Action, Nothing }

    public class GameInfo
    {
        public int[,]? Field { get; set; }
        public int[,]? Next { get; set; }
        public int Score { get; set; }
        public int HighScore { get; set; }
        public int Level { get; set; }
        public int Speed { get; set; }
        public int Pause { get; set; }
    }

    protected GameInfo info;

    public abstract void UserInput(UserAction action, bool hold);
    public abstract GameInfo UpdateCurrentState();
}