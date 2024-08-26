using System.Text;
using System.Text.Json;
using games;

namespace client
{
    class Program
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly string BaseUrl = "http://localhost:5000/api/game";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Select a game: 1 - Snake, 2 - Tetris, 3 - Race");
            var gameChoice = Console.ReadLine();
            var gameId = gameChoice switch
            {
                "1" => 1,
                "2" => 2,
                "3" => 3,
                _ => -1
            };

            if (gameId == -1)
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            await StartGameAsync(gameId);

            while (true)
            {
                var gameState = await GetGameStateAsync();
                Console.Clear();
                DisplayGame(gameState);

                var key = Console.ReadKey(true).Key;
                var action = MapKeyToAction(key);
                await PostActionAsync(action);
            }
        }

        static async Task StartGameAsync(int gameId)
        {
            await HttpClient.PostAsync($"{BaseUrl}/start", new StringContent(gameId.ToString(), Encoding.UTF8, "application/json"));
        }

        static async Task PostActionAsync(BrickGame.UserAction action)
        {
            var content = new StringContent(JsonSerializer.Serialize(action), Encoding.UTF8, "application/json");
            await HttpClient.PostAsync($"{BaseUrl}/actions", content);
        }

        static async Task<BrickGame.GameInfo> GetGameStateAsync()
        {
            var response = await HttpClient.GetStringAsync($"{BaseUrl}/state");
            return JsonSerializer.Deserialize<BrickGame.GameInfo>(response);
        }

        static void DisplayGame(BrickGame.GameInfo gameState)
        {
            // Implement game display logic
        }

        static BrickGame.UserAction MapKeyToAction(ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.LeftArrow => BrickGame.UserAction.Left,
                ConsoleKey.RightArrow => BrickGame.UserAction.Right,
                ConsoleKey.UpArrow => BrickGame.UserAction.Up,
                ConsoleKey.DownArrow => BrickGame.UserAction.Down,
                ConsoleKey.Spacebar => BrickGame.UserAction.Action,
                ConsoleKey.Enter => BrickGame.UserAction.Start,
                ConsoleKey.Q => BrickGame.UserAction.Terminate,
                _ => BrickGame.UserAction.Nothing,
            };
        }
    }
}
