namespace games
{
    public class TetrisGame : BrickGame
    {
        private const int FieldWidth = 10;
        private const int FieldHeight = 20;
        private const int SpawnX = 5;
        private const int SpawnY = 1;

        private static readonly TimeSpan[] SpeedTable =
        {
            TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(720), TimeSpan.FromMilliseconds(640),
            TimeSpan.FromMilliseconds(560), TimeSpan.FromMilliseconds(480), TimeSpan.FromMilliseconds(400),
            TimeSpan.FromMilliseconds(320), TimeSpan.FromMilliseconds(240), TimeSpan.FromMilliseconds(160),
            TimeSpan.FromMilliseconds(80)
        };

        private int[,] _field;
        private int _currentFigureId;
        private int _nextFigureId;
        private int _figureX;
        private int _figureY;
        private int _figureOrientation;
        private GameState _state;
        private DateTime _timer;

        private static readonly int[,,] Figures =
        {
            // Линия
            { { 0, 1, 2, 3 }, { 0, 0, 0, 0 } },

            // L
            { { 0, 1, 1, 1 }, { 0, 0, 0, 1 } },

            // J
            { { 0, 1, 1, 1 }, { 0, 0, 1, 0 } },

            // Квадрат
            { { 0, 1, 1, 0 }, { 0, 0, 1, 1 } },

            // T
            { { 0, 1, 2, 1 }, { 1, 1, 1, 0 } },

            // S
            { { 1, 1, 0, 0 }, { 0, 1, 1, 0 } },

            // Z
            { { 0, 1, 1, 2 }, { 0, 0, 1, 1 } }
        };

        public TetrisGame()
        {
            InitializeGame();
        }
        

        private enum GameState
        {
            GameStart,
            GameEnd,
            Spawn,
            Moving,
            Attaching
        }

        private void InitializeGame()
        {
            _field = new int[FieldHeight + 2, FieldWidth + 2];
            info = new GameInfo
            {
                Field = new int[FieldHeight + 2, FieldWidth + 2],
                Next = new int[2, 4],
                Score = 0,
                HighScore = GetHighScore(),
                Level = 1,
                Speed = 1,
                Pause = 0
            };

            for (int i = 0; i < FieldHeight + 2; i++)
            {
                for (int j = 0; j < FieldWidth + 2; j++)
                {
                    if (i == 0 || i == FieldHeight + 1 || j == 0 || j == FieldWidth + 1)
                    {
                        _field[i, j] = 9;
                    }
                    else
                    {
                        _field[i, j] = 0;
                    }
                }
            }

            _state = GameState.GameStart;
            _timer = DateTime.Now;
            _currentFigureId = -1;
            _nextFigureId = -1;
        }

        public override void UserInput(UserAction action, bool hold)
        {
            if (_state == GameState.GameStart && action == UserAction.Start)
            {
                StartGame();
            }
            else if (_state == GameState.GameEnd && action == UserAction.Start)
            {
                InitializeGame();
                StartGame();
            }

            if (_state == GameState.Moving)
            {
                ProcessMoving(action);
            }

            if (action == UserAction.Terminate)
            {
                Environment.Exit(0);
            }
        }

        public override GameInfo UpdateCurrentState()
        {
            if (_state == GameState.Moving && (DateTime.Now - _timer) > SpeedTable[info.Level - 1])
            {
                ClearFigure(_field, _figureX, _figureY, _currentFigureId, _figureOrientation);

                _figureY++;

                if (IsColliding(_field, _figureX, _figureY, _figureOrientation, _currentFigureId))
                {
                    _figureY--;
                    AttachFigure();
                }
                else
                {
                    DrawFigure(_field, _figureX, _figureY, _currentFigureId, _figureOrientation);
                }

                _timer = DateTime.Now;
            }

            UpdateViewField();
            return info;
        }

        private void StartGame()
        {
            _state = GameState.Spawn;
            SpawnFigure();
        }

        private void SpawnFigure()
        {
            _currentFigureId = _nextFigureId == -1 ? new Random().Next(0, Figures.GetLength(0)) : _nextFigureId;
            _figureX = SpawnX;
            _figureY = SpawnY;
            _figureOrientation = 0;

            _nextFigureId = new Random().Next(0, Figures.GetLength(0));
            ClearViewNext();
            if (info.Next != null)
                DrawFigure(info.Next, 0, 0, _nextFigureId, 0);

            if (IsColliding(_field, _figureX, _figureY, _figureOrientation, _currentFigureId))
            {
                _state = GameState.GameEnd;
            }
            else
            {
                _state = GameState.Moving;
            }

            _timer = DateTime.Now;
        }

        private void ProcessMoving(UserAction action)
        {
            if (info.Pause == 1)
            {
                _timer = DateTime.Now;
                if (action == UserAction.Pause) info.Pause = 0;
                return;
            }

            ClearFigure(_field, _figureX, _figureY, _currentFigureId, _figureOrientation);

            if (action == UserAction.Down)
            {
                if (!IsColliding(_field, _figureX, _figureY + 1, _figureOrientation, _currentFigureId))
                {
                    _figureY++;
                }
                else
                {
                    AttachFigure();
                }
            }
            else if (action == UserAction.Left || action == UserAction.Right)
            {
                int dx = action == UserAction.Left ? -1 : 1;
                if (!IsColliding(_field, _figureX + dx, _figureY, _figureOrientation, _currentFigureId))
                {
                    _figureX += dx;
                }
            }
            else if (action == UserAction.Action)
            {
                int newOrientation = (_figureOrientation + 1) % 4;
                if (_currentFigureId != 3 && !IsColliding(_field, _figureX, _figureY, newOrientation, _currentFigureId))
                {
                    _figureOrientation = newOrientation;
                }
            }
            else if (action == UserAction.Pause)
            {
                info.Pause = 1;
            }

            DrawFigure(_field, _figureX, _figureY, _currentFigureId, _figureOrientation);
        }

        private void ClearFigure(int[,] dest, int x, int y, int id, int orientation)
        {
            for (int i = 0; i < 4; i++)
            {
                int dx = Figures[id, 0, i];
                int dy = Figures[id, 1, i];

                switch (orientation)
                {
                    case 1:
                        (dx, dy) = (-dy, dx);
                        break;
                    case 2:
                        (dx, dy) = (-dx, -dy);
                        break;
                    case 3:
                        (dx, dy) = (dy, -dx);
                        break;
                }

                if (x + dx >= 1 && x + dx <= FieldWidth && y + dy >= 1 && y + dy <= FieldHeight)
                {
                    dest[y + dy, x + dx] = 0;
                }
            }
        }

        private void AttachFigure()
        {
            DrawFigure(_field, _figureX, _figureY, _currentFigureId, _figureOrientation);
            int linesCleared = CheckLines();

            info.Score += linesCleared * 100;
            if (info.Score / 1000 >= info.Level)
            {
                info.Level++;
                _timer = DateTime.Now;
            }

            if (info.Score > GetHighScore())
            {
                info.HighScore = info.Score;
                SetHighScore(info.Score);
            }

            _state = GameState.Spawn;
            SpawnFigure();
        }

        private int CheckLines()
        {
            int linesCleared = 0;
            bool[] fullLines = new bool[FieldHeight + 1];

            for (int y = FieldHeight; y >= 1; y--)
            {
                bool fullLine = true;
                for (int x = 1; x <= FieldWidth; x++)
                {
                    if (_field[y, x] == 0)
                    {
                        fullLine = false;
                        break;
                    }
                }

                if (fullLine)
                {
                    fullLines[y] = true;
                    linesCleared++;
                }
            }

            if (linesCleared > 0)
            {
                int targetY = FieldHeight;

                for (int y = FieldHeight; y >= 1; y--)
                {
                    if (fullLines[y])
                    {
                        continue;
                    }

                    for (int x = 1; x <= FieldWidth; x++)
                    {
                        _field[targetY, x] = _field[y, x];
                    }

                    targetY--;
                }

                for (int y = targetY; y >= 1; y--)
                {
                    for (int x = 1; x <= FieldWidth; x++)
                    {
                        _field[y, x] = 0;
                    }
                }
            }

            return linesCleared;
        }

        private static bool IsColliding(int[,] field, int x, int y, int orientation, int id)
        {
            for (var i = 0; i < 4; i++)
            {
                var dx = Figures[id, 0, i];
                var dy = Figures[id, 1, i];

                switch (orientation)
                {
                    case 1:
                        (dx, dy) = (-dy, dx);
                        break;
                    case 2:
                        (dx, dy) = (-dx, -dy);
                        break;
                    case 3:
                        (dx, dy) = (dy, -dx);
                        break;
                }

                if (x + dx < 1 || x + dx > FieldWidth || y + dy < 1 || y + dy > FieldHeight ||
                    field[y + dy, x + dx] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DrawFigure(int[,] field, int x, int y, int id, int orientation)
        {
            for (var i = 0; i < 4; i++)
            {
                var dx = Figures[id, 0, i];
                var dy = Figures[id, 1, i];

                (dx, dy) = orientation switch
                {
                    1 => (-dy, dx),
                    2 => (-dx, -dy),
                    3 => (dy, -dx),
                    _ => (dx, dy)
                };

                if (x + dx >= 1 && x + dx <= FieldWidth && y + dy >= 1 && y + dy <= FieldHeight)
                {
                    field[y + dy, x + dx] = id + 1;
                }
            }
        }

        private void UpdateViewField()
        {
            Array.Copy(_field, info.Field, _field.Length);
        }

        private void ClearViewNext()
        {
            for (int i = 0; i < info.Next.GetLength(0); i++)
            {
                for (int j = 0; j < info.Next.GetLength(1); j++)
                {
                    info.Next[i, j] = 0;
                }
            }
        }

        private int GetHighScore()
        {
            try
            {
                return int.Parse(File.ReadAllText("score_tetris.txt"));
            }
            catch
            {
                return 0;
            }
        }

        private void SetHighScore(int score)
        {
            File.WriteAllText("score_tetris.txt", score.ToString());
        }
    }
}