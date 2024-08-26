using games;

namespace controller
{
    public class GameController
    {
        private readonly BrickGame _game;

        public GameController(BrickGame game)
        {
            _game = game;
        }

        public void HandleUserInput(BrickGame.UserAction action, bool hold)
        {
            _game.UserInput(action, hold);
        }

        public BrickGame.GameInfo? GetGameState()
        {
            return _game.UpdateCurrentState();
        }
    }
}
