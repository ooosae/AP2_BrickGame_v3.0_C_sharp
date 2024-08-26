using games;
using race;

namespace tests
{
    public class RaceGameTests
    {
        [Fact]
        public void TestCheckCollisions_CollisionDetected()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();
            
            // Arrange
            game.CarPosition = 1;
            game.SetObstacles(new List<(int y, int lane, int type)>
            {
                (15, 1, 2)
            });

            // Act
            for (var i = 0; i < 10; i++)
            {
                game.GenerateObstacles();
                game.MoveObstacles();
            }

            game.CheckCollisions();
            
            // Assert
            Assert.Equal(RaceGame.GameState.Move, game.GetGameState());
        }
        
        [Fact]
        public void TestScore()
        {
            // Arrange
            var score = 100;
            var game = new RaceGame();
            game.UpdateCurrentState();
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();
            game.CheckCollisions();
            
            // Act
            game.SetHighScore(score);
            
            // Assert
            Assert.Equal(score, game.GetHighScore());
        }
        
        [Fact]
        public void TestGameStart()
        {
            // Arrange
            var game = new RaceGame();

            // Act
            game.UserInput(BrickGame.UserAction.Start, false);
            var info = game.UpdateCurrentState();

            // Assert
            Assert.Equal(RaceGame.GameState.Move, game.GetGameState());
            Assert.Equal(0, info.Score);
            Assert.Equal(1, info.Level);
            Assert.Equal(20 / 2 / 10, game.GetCarPosition());
            game.UserInput(BrickGame.UserAction.Up, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Down, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            Assert.Equal(RaceGame.GameState.Move, game.GetGameState());
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Pause, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Action, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            game.UserInput(BrickGame.UserAction.Nothing, false);
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
        }

        [Fact]
        public void TestSpawnWhenEmpty()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            var timerField = typeof(RaceGame).GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timerField.SetValue(game, DateTime.MaxValue);

            var stateField = typeof(RaceGame).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateField.SetValue(game, RaceGame.GameState.Move);

            game.SetObstacles(new List<(int y, int lane, int type)>());

            // Act
            game.UpdateCurrentState();

            // Assert
            Assert.True(game.GetObstacles().Count == 0);
        }

        [Fact]
        public void TestSpawnNot()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            var timerField = typeof(RaceGame).GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timerField.SetValue(game, DateTime.MaxValue);

            game.SetObstacles(new List<(int y, int lane, int type)> { (20, 2, 2) });
            game.CheckCollisions();
            game.UpdateCurrentState();
            game.CheckCollisions();
            
            // Act
            game.UpdateCurrentState();

            // Assert
            Assert.Single(game.GetObstacles());
        }

        [Fact]
        public void TestCheckAddsScore()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            game.SetObstacles(new List<(int y, int lane, int type)> { (20 - 5, 0, 2) });

            // Act
            game.UpdateCurrentState(); 
            var scoreBefore = game.UpdateCurrentState().Score;

            for (int i = 0; i < 10; i++)
            {
                game.UpdateCurrentState();
            }

            var scoreAfter = game.UpdateCurrentState().Score;

            // Assert
            Assert.Equal(scoreBefore, scoreAfter);
        }

        [Fact]
        public void TestCheckHit()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            game.SetObstacles(new List<(int y, int lane, int type)> { (20 - 5, 0, 2) });

            // Act
            game.UpdateCurrentState();

            // Assert
            Assert.Equal(RaceGame.GameState.Move, game.GetGameState());
        }

        [Fact]
        public void TestMoveLeft()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            // Act
            game.UserInput(BrickGame.UserAction.Left, false);
            var positionAfterMove = game.GetCarPosition();

            // Assert
            Assert.Equal(0, positionAfterMove);
        }

        [Fact]
        public void TestMoveRight()
        {
            // Arrange
            var game = new RaceGame();
            game.UserInput(BrickGame.UserAction.Start, false);
            game.UpdateCurrentState();

            // Act
            game.UserInput(BrickGame.UserAction.Right, false);
            game.UserInput(BrickGame.UserAction.Right, false);
            var positionAfterMove = game.GetCarPosition();

            // Assert
            Assert.Equal(2, positionAfterMove); 
        }
    }
}
