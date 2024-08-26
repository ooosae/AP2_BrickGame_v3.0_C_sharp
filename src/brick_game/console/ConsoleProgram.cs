using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using games;

namespace ConsoleApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiUrl = "http://localhost:5109/api/game/";

        private const char Char2 = '█';
        private const char Char1 = '░';
        private const char EmptyChar = '.';

        static async Task Main(string[] args)
        {
            Console.WriteLine("Select a game: 1 - Snake, 2 - Tetris, 3 - Race");
            var gameChoice = Console.ReadLine();

            int gameId = gameChoice switch
            {
                "1" => 1,
                "2" => 2,
                "3" => 3,
                _ => 0
            };

            if (gameId == 0)
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            await StartGame(gameId);

            while (true)
            {
                await UpdateGameState();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    string action = MapKeyToAction(key);
                    await PostAction(action);
                }

                await Task.Delay(100);
            }
        }

        static async Task StartGame(int gameId)
        {
            var gameName = gameId switch
            {
                1 => "Snake Game",
                2 => "Tetris Game",
                3 => "Race Game",
                _ => "Unknown Game"
            };

            try
            {
                var response = await client.PostAsync(apiUrl + "start",
                    new StringContent(JsonConvert.SerializeObject(gameId), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                Console.WriteLine($"Started {gameName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game: {ex.Message}");
            }
        }

        static async Task UpdateGameState()
        {
            try
            {
                var response = await client.GetAsync(apiUrl + "state");

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching game state: {response.StatusCode} {response.ReasonPhrase}. Response: {responseBody}");
                    return;
                }

                var gameStateJson = await response.Content.ReadAsStringAsync();
                var gameState = JsonConvert.DeserializeObject<BrickGame.GameInfo>(gameStateJson);

                DisplayGame(gameState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching game state: {ex.Message}");
            }
        }

        static async Task PostAction(string action)
        {
            try
            {
                var response = await client.PostAsync(apiUrl + "actions",
                    new StringContent(JsonConvert.SerializeObject(action), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error posting action: {response.StatusCode} {response.ReasonPhrase}. Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting action: {ex.Message}");
            }
        }

        static string MapKeyToAction(ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.LeftArrow => "Left",
                ConsoleKey.RightArrow => "Right",
                ConsoleKey.UpArrow => "Up",
                ConsoleKey.DownArrow => "Down",
                ConsoleKey.Spacebar => "Action",
                ConsoleKey.Enter => "Start",
                ConsoleKey.Escape => "Terminate",
                _ => "Nothing",
            };
        }

        static void DisplayGame(BrickGame.GameInfo? gameState)
        {
            Console.Clear();

            if (gameState?.Field != null)
            {
                for (int i = 0; i < gameState.Field.GetLength(0); i++)
                {
                    for (int j = 0; j < gameState.Field.GetLength(1); j++)
                    {
                        char displayChar = Char2;
                        if (gameState.Field[i, j] == 9)
                        {
                            displayChar = Char1;
                        }
                        else if (gameState.Field[i, j] == 0)
                        {
                            displayChar = EmptyChar;
                        }
                        Console.Write(displayChar + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine($"Score: {gameState.Score}");
                Console.WriteLine($"High Score: {gameState.HighScore}");
                Console.WriteLine($"Level: {gameState.Level}");
            }
            else
            {
                Console.WriteLine("Game Over");
                Console.WriteLine("Press Enter to restart.");
            }
        }
    }
}
