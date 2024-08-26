namespace games
{
    public class SnakeGame : BrickGame
    {
        private enum GameState
        {
            GameStart,
            GameEnd,
            Spawn,
            Move
        }

        private List<(int, int)> _snake = new List<(int, int)>();
        private GameState _state = GameState.GameStart;
        private DateTime _timer;
        private UserAction _action = UserAction.Nothing;

        private static readonly TimeSpan[] SpeedTable =
        {
            TimeSpan.FromMilliseconds(350),
            TimeSpan.FromMilliseconds(330),
            TimeSpan.FromMilliseconds(300),
            TimeSpan.FromMilliseconds(280),
            TimeSpan.FromMilliseconds(270),
            TimeSpan.FromMilliseconds(260),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(240),
            TimeSpan.FromMilliseconds(230),
            TimeSpan.FromMilliseconds(200)
        };

        private static readonly TimeSpan SkipSpeed = TimeSpan.FromMilliseconds(150);

        private readonly int _fieldWidth;
        private readonly int _fieldHeight;

        public SnakeGame(int width, int height)
        {
            _fieldWidth = width;
            _fieldHeight = height;
            info = new GameInfo
            {
                Field = new int[height, width],
                Score = 0,
                HighScore = GetHighScore(),
                Level = 1,
                Pause = 0
            };
        }

        public override void UserInput(UserAction action, bool hold)
        {
            _action = action;
        }

        public override GameInfo UpdateCurrentState()
        {
            if (_action == UserAction.Terminate)
            {
                Environment.Exit(0);
            }

            switch (_state)
            {
                case GameState.GameStart:
                    ProcessGameStart();
                    break;
                case GameState.GameEnd:
                    ProcessGameEnd();
                    break;
                case GameState.Spawn:
                    ProcessSpawn();
                    break;
                case GameState.Move:
                    ProcessMove();
                    break;
            }

            return info;
        }

        private void ProcessGameStart()
        {
            if (_action == UserAction.Start)
            {
                InitializeField();
                InitializeSnake();

                _timer = DateTime.Now;
                _state = GameState.Spawn;
                ProcessSpawn();
            }
        }

        private void InitializeField()
        {
            info.Field = new int[_fieldHeight, _fieldWidth];
        }

        private void InitializeSnake()
        {
            _snake.Clear();
            for (int i = 3; i <= 6; i++)
            {
                _snake.Add((i, _fieldHeight / 2));
                info.Field[_fieldHeight / 2, i] = 1;
            }
        }

        private void ProcessSpawn()
        {
            var random = new Random();
            int loc = random.Next(_fieldWidth * _fieldHeight - _snake.Count);
            int ptr = 0;

            for (int i = 0; i < _fieldHeight; i++)
            {
                for (int j = 0; j < _fieldWidth && loc != -1; j++)
                {
                    if (info.Field[i, j] == 0)
                    {
                        if (ptr == loc)
                        {
                            info.Field[i, j] = 9;
                            loc = -1;
                        }
                        else
                        {
                            ptr++;
                        }
                    }
                }
            }

            _state = GameState.Move;
        }

        private void ProcessMove()
        {
            if (_action == UserAction.Pause)
            {
                _timer = DateTime.Now;
                _action = UserAction.Nothing;
                info.Pause = 1 - info.Pause;
            }

            while (!Convert.ToBoolean(info.Pause) && _state == GameState.Move &&
                   (DateTime.Now - _timer) >= (_action == UserAction.Action ? SkipSpeed : SpeedTable[info.Level - 1]))
            {
                var next = SelectNext();

                if (IsOutOfBounds(next))
                {
                    _state = GameState.GameEnd;
                }
                else if (info.Field[next.Item2, next.Item1] == 9)
                {
                    info.Field[next.Item2, next.Item1] = 1;
                    _snake.Insert(0, next);
                    info.Score++;
                    if (info.Score > info.HighScore)
                    {
                        SetHighScore(info.Score);
                        info.HighScore = info.Score;
                    }

                    info.Level = Math.Min((info.Score / 5) + 1, 10);
                    _state = GameState.Spawn;
                }
                else if (info.Field[next.Item2, next.Item1] == 1)
                {
                    if (_snake[_snake.Count - 1] == next)
                    {
                        _snake.RemoveAt(_snake.Count - 1);
                        _snake.Insert(0, next);
                    }
                    else
                    {
                        _state = GameState.GameEnd;
                    }
                }
                else
                {
                    var tail = _snake[_snake.Count - 1];
                    info.Field[tail.Item2, tail.Item1] = 0;
                    _snake.RemoveAt(_snake.Count - 1);
                    _snake.Insert(0, next);
                    info.Field[next.Item2, next.Item1] = 1;
                }

                _timer = _timer.Add(_action == UserAction.Action ? SkipSpeed : SpeedTable[info.Level - 1]);
                _action = UserAction.Nothing;
            }
        }

        private (int, int) SelectNext()
        {
            UserAction prev = UserAction.Right, unc = UserAction.Left, inp = UserAction.Right;
            var head = _snake[0];
            var neck = _snake[1];

            if (head.Item1 > neck.Item1)
            {
                prev = UserAction.Right;
                unc = UserAction.Left;
            }
            else if (head.Item1 < neck.Item1)
            {
                prev = UserAction.Left;
                unc = UserAction.Right;
            }
            else if (head.Item2 > neck.Item2)
            {
                prev = UserAction.Down;
                unc = UserAction.Up;
            }
            else if (head.Item2 < neck.Item2)
            {
                prev = UserAction.Up;
                unc = UserAction.Down;
            }

            if ((_action != UserAction.Right && _action != UserAction.Left && _action != UserAction.Up && _action != UserAction.Down) ||
                _action == unc)
            {
                inp = prev;
            }
            else
            {
                inp = _action;
            }

            return inp switch
            {
                UserAction.Left => (head.Item1 - 1, head.Item2),
                UserAction.Right => (head.Item1 + 1, head.Item2),
                UserAction.Up => (head.Item1, head.Item2 - 1),
                UserAction.Down => (head.Item1, head.Item2 + 1),
                _ => head,
            };
        }

        private bool IsOutOfBounds((int, int) pos)
        {
            return pos.Item1 < 0 || pos.Item1 >= _fieldWidth || pos.Item2 < 0 || pos.Item2 >= _fieldHeight;
        }

        private void ProcessGameEnd()
        {
            if (_action == UserAction.Start)
            {
                FreeResources();
                _action = UserAction.Nothing;
                _state = GameState.GameStart;
                _timer = DateTime.MinValue;
                info.Score = 0;
                info.Level = 1;
            }
        }

        private int GetHighScore()
        {
            try
            {
                return int.Parse(File.ReadAllText("score_snake.txt"));
            }
            catch
            {
                return 0;
            }
        }

        private void SetHighScore(int score)
        {
            File.WriteAllText("score_snake.txt", score.ToString());
        }

        private void FreeResources()
        {
            _snake.Clear();
            Array.Clear(info.Field, 0, info.Field.Length);
        }
    }
}
