using Microsoft.AspNetCore.Mvc;
using games;
using race;

namespace server
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private static BrickGame? _currentGame;
        private static controller.GameController? _gameController;

        public class StartGameRequest
        {
            public int GameId { get; set; }
        }

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] int gameId)
        {
            Console.WriteLine($"Received StartGame request with ID: {gameId}");
    
            _currentGame = gameId switch
            {
                1 => new SnakeGame(10, 20),
                2 => new TetrisGame(),
                3 => new RaceGame(),
                _ => null
            };

            if (_currentGame == null)
            {
                Console.WriteLine("Invalid game ID");
                return BadRequest("Invalid game ID");
            }

            _gameController = new controller.GameController(_currentGame);

            Console.WriteLine($"Game started: {_currentGame.GetType().Name}");

            return Ok();
        }

        [HttpPost("actions")]
        public IActionResult PostAction([FromBody] BrickGame.UserAction action)
        {
            if (_currentGame == null || _gameController == null)
                return BadRequest("No game is running");

            _gameController.HandleUserInput(action, false);
            return NoContent();
        }

        [HttpGet("state")]
        public IActionResult GetState()
        {
            if (_currentGame == null || _gameController == null)
                return BadRequest("No game is running");

            var gameState = _gameController.GetGameState();
            return Ok(gameState);
        }
    }
}