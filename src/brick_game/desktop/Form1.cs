using System.Text;
using games;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace desktop
{
    public class GamePanel : Panel
    {
        private BrickGame.GameInfo? _gameState;

        public GamePanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }

        public void DrawGame(BrickGame.GameInfo? gameState)
        {
            _gameState = gameState;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_gameState == null) return;

            var graphics = e.Graphics;
            graphics.Clear(Color.Black);
            var pen = new Pen(Color.Black);

            if (_gameState?.Field != null)
            {
                var cellSize = 20;

                for (int i = 0; i < _gameState.Field.GetLength(0); i++)
                {
                    for (int j = 0; j < _gameState.Field.GetLength(1); j++)
                    {
                        Color cellColor = _gameState.Field[i, j] switch
                        {
                            9 => Color.Blue,
                            0 => Color.White,
                            _ => Color.Red,
                        };

                        using (var brush = new SolidBrush(cellColor))
                        {
                            graphics.FillRectangle(brush, j * cellSize, i * cellSize, cellSize, cellSize);
                        }

                        graphics.DrawRectangle(pen, j * cellSize, i * cellSize, cellSize, cellSize);
                    }
                }

                graphics.DrawString($"Score: {_gameState.Score}", this.Font, Brushes.White, 250, 40);
                graphics.DrawString($"High Score: {_gameState.HighScore}", this.Font, Brushes.White, 250, 70);
                graphics.DrawString($"Level: {_gameState.Level}", this.Font, Brushes.White, 250, 100);
            }
            else
            {
                graphics.DrawString("Game Over", this.Font, Brushes.Red, 100, 100);
                graphics.DrawString("Press Enter to restart.", this.Font, Brushes.Red, 100, 130);
            }
        }
    }

    
    public partial class Form1 : Form
    {
        private static readonly HttpClient Client = new HttpClient();
        private const string apiUrl = "http://localhost:5109/api/game/";

        private bool _isGameStarted;
        private int _gameId;
        private Timer _timer;
        private Graphics _graphics;
        private GamePanel _gamePanel;

        public Form1()
        {
            InitializeComponents();
            InitializeGame();
        }

        private void InitializeComponents()
        {
            _gamePanel = new GamePanel();
            _gamePanel.Dock = DockStyle.Fill;
            this.Controls.Add(_gamePanel);
            
            this.menuStrip = new MenuStrip();
            this.startMenu = new ToolStripMenuItem();
            this.snakeMenuItem = new ToolStripMenuItem();
            this.tetrisMenuItem = new ToolStripMenuItem();
            this.raceMenuItem = new ToolStripMenuItem();

            this.menuStrip.Items.AddRange(new ToolStripItem[]
            {
                this.startMenu
            });

            this.startMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                this.snakeMenuItem,
                this.tetrisMenuItem,
                this.raceMenuItem
            });

            this.startMenu.Text = "Start";
            this.snakeMenuItem.Text = "Snake";
            this.tetrisMenuItem.Text = "Tetris";
            this.raceMenuItem.Text = "Race";

            this.snakeMenuItem.Click += (s, e) => _ = StartGame(1);
            this.tetrisMenuItem.Click += (s, e) => _ = StartGame(2);
            this.raceMenuItem.Click += (s, e) => _ = StartGame(3);

            this.MainMenuStrip = this.menuStrip;
            this.Controls.Add(this.menuStrip);
            this.Text = "Game Application";
            this.ClientSize = new Size(800, 600);
            this.BackColor = Color.Black;
            this.KeyDown += OnKeyDown;
        }

        private void InitializeGame()
        {
            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Tick += async (s, e) => await UpdateGameState();
            _timer.Start();

            _graphics = this.CreateGraphics();
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_isGameStarted)
        {
            string action = e.KeyCode switch
            {
                Keys.Left => "Left",
                Keys.Right => "Right",
                Keys.Up => "Up",
                Keys.Down => "Down",
                Keys.Space => "Action",
                Keys.Enter => "Start",
                _ => "Nothing",
            };

            await PostAction(action);
        }
    }

    private async Task StartGame(int gameId)
    {
        _gameId = gameId;
        _isGameStarted = true;

        var response = await Client.PostAsync(apiUrl + "start",
            new StringContent(JsonConvert.SerializeObject(gameId), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
    }

    private async Task UpdateGameState()
    {
        if (!_isGameStarted) return;

        var response = await Client.GetAsync(apiUrl + "state");

        if (!response.IsSuccessStatusCode)
        {
            await response.Content.ReadAsStringAsync();
            return;
        }

        var gameStateJson = await response.Content.ReadAsStringAsync();
        var gameState = JsonConvert.DeserializeObject<BrickGame.GameInfo>(gameStateJson);

        _gamePanel.DrawGame(gameState);
    }

    private async Task PostAction(string action)
    {
        try
        {
            var response = await Client.PostAsync(apiUrl + "actions",
                new StringContent(JsonConvert.SerializeObject(action), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                MessageBox.Show(
                    $"Error posting action: {response.StatusCode} {response.ReasonPhrase}. Response: {responseBody}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error posting action: {ex.Message}");
        }
    }

        private MenuStrip menuStrip;
        private ToolStripMenuItem startMenu;
        private ToolStripMenuItem snakeMenuItem;
        private ToolStripMenuItem tetrisMenuItem;
        private ToolStripMenuItem raceMenuItem;
    }
}