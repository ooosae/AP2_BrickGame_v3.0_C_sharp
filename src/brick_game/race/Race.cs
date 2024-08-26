using System;
using System.Collections.Generic;
using System.IO;
using games;

namespace race
{
    public class RaceGame : BrickGame
    {
        private const int FieldWidth = 10;
        private const int FieldHeight = 20;
        private const int MaxLane = 2;
        public const int ObstacleGenerationInterval = 20;

        public static readonly int[,] CarTexture = {
            {0, 1, 0},
            {1, 1, 1},
            {0, 1, 0},
            {1, 1, 1},
            {0, 1, 0}
        };
        
        private static readonly TimeSpan[] SpeedTable =
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(90),
            TimeSpan.FromMilliseconds(80),
            TimeSpan.FromMilliseconds(70),
            TimeSpan.FromMilliseconds(60),
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(40),
            TimeSpan.FromMilliseconds(30),
            TimeSpan.FromMilliseconds(20),
            TimeSpan.FromMilliseconds(10)
        };

        public enum GameState
        {
            GameStart,
            GameEnd,
            Move
        }

        private GameState _state = GameState.GameStart;
        private DateTime _timer;
        public int CarPosition;
        private List<(int y, int lane, int type)> _obstacles;
        private bool _scoreIncreased = false;
        private int _obstacleGenerationCounter = 0;
        private TimeSpan _currentSpeed;
        
        public RaceGame()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            info = new GameInfo
            {
                Field = new int[FieldHeight, FieldWidth],
                Score = 0,
                HighScore = GetHighScore(),
                Level = 1,
                Speed = 0,
                Pause = 0
            };
            _state = GameState.GameStart;
            _timer = DateTime.Now;
            _currentSpeed = SpeedTable[0];
            _obstacles = new List<(int y, int lane, int type)>();
        }

        public override void UserInput(UserAction action, bool hold)
        {
            if (_state == GameState.GameStart && action == UserAction.Start)
            {
                StartGame();
            }
            else if (_state == GameState.Move)
            {
                ProcessMovement(action);
            }

            if (action == UserAction.Terminate)
            {
                Environment.Exit(0);
            }
            else if (_state == GameState.GameEnd && action == UserAction.Start)
            {
                StartGame();
            }
        }

        public override GameInfo UpdateCurrentState()
        {
            if (_state == GameState.Move && (DateTime.Now - _timer) > _currentSpeed)
            {
                MoveObstacles();
                CheckCollisions();
                _obstacleGenerationCounter++;

                if (_obstacleGenerationCounter >= ObstacleGenerationInterval)
                {
                    GenerateObstacles();
                    _obstacleGenerationCounter = 0;
                }

                _timer = DateTime.Now;
            }

            UpdateViewField();
            return info;
        }

        private void StartGame()
        {
            _state = GameState.Move;
            _timer = DateTime.Now;
            CarPosition = FieldWidth / 2 / 3;
            _obstacles.Clear();
            info.Score = 0;
            info.Level = 1;
            _obstacleGenerationCounter = 0;
            _currentSpeed = SpeedTable[0];
        }

        private void ProcessMovement(UserAction action)
        {
            if (info.Pause == 1)
            {
                _timer = DateTime.Now;
                if (action == UserAction.Pause) info.Pause = 0;
                return;
            }

            if (action == UserAction.Left && CarPosition > 0)
            {
                CarPosition--;
            }
            else if (action == UserAction.Right && CarPosition < MaxLane)
            {
                CarPosition++;
            }
        }

        public void MoveObstacles()
        {
            for (int i = 0; i < _obstacles.Count; i++)
            {
                var (y, lane, type) = _obstacles[i];
                _obstacles[i] = (y + 1, lane, type);
                if (_obstacles[i].y >= FieldHeight)
                {
                    _obstacles.RemoveAt(i);
                    i--;
                }
            }
        }

        public void GenerateObstacles()
        {
            var random = new Random();
            int lane;
            do
            {
                lane = random.Next(0, MaxLane + 1);
            } while (_obstacles.Exists(o => o.lane == lane && o.y < 3));

            _obstacles.Add((0, lane, 9));
        }

        public void CheckCollisions()
        {
            bool collisionDetected = false;

            foreach (var (y, lane, type) in _obstacles)
            {
                if (lane == CarPosition && y >= FieldHeight - 5)
                {
                    _state = GameState.GameEnd;
                    collisionDetected = true;
                    break;
                }
            }

            if (!collisionDetected && _obstacles.Count > 0 && _obstacles[0].y >= FieldHeight - 1)
            {
                if (!_scoreIncreased)
                {
                    info.Score++;
                    _scoreIncreased = true;
                }
            }
            else
            {
                _scoreIncreased = false;
            }

            if (info.Score > info.HighScore)
            {
                SetHighScore(info.Score);
                info.HighScore = info.Score;
            }

            int newLevel = Math.Min(info.Score / 5 + 1, 10);
            if (newLevel != info.Level)
            {
                info.Level = newLevel;
                _currentSpeed = SpeedTable[info.Level - 1];
                _timer = DateTime.Now;
            }
        }

        private void UpdateViewField()
        {
            if (info?.Field == null) throw new InvalidOperationException("Game info not initialized.");

            Array.Clear(info.Field, 0, info.Field.Length);

            DrawTexture(CarTexture, FieldHeight - 1, CarPosition * 3, 1);

            foreach (var (y, lane, type) in _obstacles)
            {
                DrawTexture(CarTexture, y, lane * 3, type);
            }
        }

        private void DrawTexture(int[,] texture, int startY, int startX, int type)
        {
            for (int y = 0; y < texture.GetLength(0); y++)
            {
                for (int x = 0; x < texture.GetLength(1); x++)
                {
                    int drawY = startY - y;
                    int drawX = startX + x;

                    if (drawY >= 0 && drawY < FieldHeight && drawX >= 0 && drawX < FieldWidth)
                    {
                        if (texture[y, x] != 0)
                        {
                            if (info.Field != null)
                                info.Field[drawY, drawX] = type;
                        }
                    }
                }
            }
        }

        public int GetHighScore()
        {
            try
            {
                return int.Parse(File.ReadAllText("score_race.txt"));
            }
            catch
            {
                return 0;
            }
        }

        public void SetHighScore(int score)
        {
            File.WriteAllText("score_race.txt", score.ToString());
        }
        
        public GameState GetGameState()
        {
            return _state;
        }

        public int GetCarPosition()
        {
            return CarPosition;
        }

        public List<(int y, int lane, int type)> GetObstacles()
        {
            return new List<(int y, int lane, int type)>(_obstacles);
        }

        public void SetObstacles(List<(int y, int lane, int type)> obstacles)
        {
            _obstacles = obstacles;
        }
    }
}
 