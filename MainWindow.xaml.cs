// must install NuGet Package: NAudio 3.1
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

/*
 * TODO LIST
 * 
 * FUTURE:
 * line clear animation
 * wall kicks
 * T-Spins
 */

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _frameTimer;
        Stopwatch _dropStopwatch = new Stopwatch();
        XboxController _controller = new XboxController(0);
        Inputs _inputs = new Inputs();
        GameData _gameData = new GameData();
        bool _gameOver = true;
        bool _gameStarted = false;
        bool _isPaused = false;
        double _tileSize = 1;
        double _topMargin = 0;
        double _leftMargin = 0;
        double _nextTileSize = 1;
        double _nextTopMargin = 0;
        double _nextLeftMargin = 0;
        int _gridRows = 22;
        int _gridColumns = 10;
        double _dropIntervalMs = 500;
        int _previousLevel = 1;
        DateTime _gameCompletion;
        bool _isSoundOn = true;

        private ImageSource[] cellImages = new ImageSource[]
        {
            new BitmapImage(new Uri("images/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileRed.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostPurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/GhostRed.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/TileWhite.png", UriKind.Relative))
        };

        private readonly ImageSource[] brickImages = new ImageSource[]
        {
            new BitmapImage(new Uri("images/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("images/Block-Z.png", UriKind.Relative))
        };
        private Image[,] imageControls;
        private GameController gameController;

        public MainWindow()
        {
            InitializeComponent();

            // initialize game timer
            _frameTimer = new DispatcherTimer();
            _frameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 fps polling
            _frameTimer.Tick += FrameTimer_Tick;

            imageControls = new Image[_gridRows, _gridColumns];
            gameController = new GameController(_gridRows, _gridColumns, Properties.Settings.Default.NumberOfPieces);
            for (int r = 0; r < _gridRows; r++)
                for (int c = 0; c < _gridColumns; c++)
                    imageControls[r, c] = new Image();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // load the properties from disk
            Properties.Settings.Default.Reload();

            // check for upgrade
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            this.Left = Properties.Settings.Default.WindowLeft;
            this.Top = Properties.Settings.Default.WindowTop;
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;

            // load other properties here

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            // ensure window size doesn't exceed screen size
            if (this.Width > screenWidth) this.Width = screenWidth;
            if (this.Height > screenHeight) this.Height = screenHeight;

            // ensure window is not off the left or top
            if (this.Left < 0) this.Left = 0;
            if (this.Top < 0) this.Top = 0;

            // ensure window is not off the right or bottom
            if (this.Left + this.Width > screenWidth)
                this.Left = screenWidth - this.Width;
            if (this.Top + this.Height > screenHeight)
                this.Top = screenHeight - this.Height;

            if (Properties.Settings.Default.WindowState == "Maximized")
                this.WindowState = WindowState.Maximized;

            // do other initialization
            if (Properties.Settings.Default.ShowHelp)
                Help_Click(this, new RoutedEventArgs());

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowState = this.WindowState.ToString();
            if (this.WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowTop = this.Top;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowHeight = this.Height;
            }

            // save other properties here
            if (!_gameOver)
            {
                _gameCompletion = DateTime.Now;
                RecordHighScore(gameController.Score, _gameCompletion);
            }

            Properties.Settings.Default.Save();

        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            // restart timer if not paused or game over
            if (_gameStarted && !_gameOver && !_isPaused)
            {
                await Countdown();
                _frameTimer.Start();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // stop timer if not already stopped
            if (_gameStarted && !_gameOver && !_isPaused)
                _frameTimer.Stop();
        }

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            // deal with previous game
            if (_gameStarted && !_gameOver) // clicked Play mid game
            {
                _gameOver = true;
                _frameTimer.Stop();
                Abandon dialog = new Abandon();
                dialog.Owner = Window.GetWindow(this);
                bool? result = dialog.ShowDialog();
                if (result == false)
                {
                    _gameOver = false;
                    _frameTimer.Start();
                    return;
                }
                else if (result == true)
                {
                    _gameCompletion = DateTime.Now;
                    RecordHighScore(gameController.Score, _gameCompletion);
                }
            }

            // start new game
            StartNewGame();
        }

        private async void StartNewGame()
        {
            // start new game
            _frameTimer.Stop();
            gameController = new GameController(_gridRows, _gridColumns, Properties.Settings.Default.NumberOfPieces);
            gameController.Level = Properties.Settings.Default.StartingLevel;
            _previousLevel = Properties.Settings.Default.StartingLevel;
            _dropIntervalMs = Math.Max(500 * Math.Pow(0.935, gameController.Level - 1), 100);

            gameController.PieceLandedAction += () => PlaySoundEffect("land");
            gameController.LinesClearedAction += (count) =>
            {
                PlaySoundEffect("clear");
                // later: trigger flash-then-collapse animation, using `count` if you want
                // different visual treatment for a Tetris (4 lines) vs a single
            };

            RedrawGame();
            await Countdown();
            _gameStarted = true;
            _gameOver = false;
            _dropStopwatch.Restart();
            _frameTimer.Start();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            int previousLevel = Properties.Settings.Default.StartingLevel;
            int previousNumber = Properties.Settings.Default.NumberOfPieces;
            Settings dialog = new Settings();
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();

            if (result == true && _gameStarted && !_gameOver)
            {
                if ((previousLevel != Properties.Settings.Default.StartingLevel) ||
                    (previousNumber != Properties.Settings.Default.NumberOfPieces))
                {
                    _gameCompletion = DateTime.Now;
                    RecordHighScore(gameController.Score, _gameCompletion);
                    StartNewGame();
                }
            }
        }

        private async void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;

            if (_isPaused)
            {
                // Resume
                await Countdown();
                _frameTimer.Start();
                PauseMenuItem.Header = "Pause";
            }
            else
            {
                // Pause
                _frameTimer.Stop();
                PauseMenuItem.Header = "Resume";
            }

            _isPaused = !_isPaused;
        }

        private void Scores_Click(object sender, RoutedEventArgs e)
        {
            HighScores dialog = new HighScores();
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.WriteLine($"Window width = {this.Width}, height = {this.Height}");
            // The window's total size (including title bar and borders)
            Debug.WriteLine($"WindowSizeChanged - Window: {this.Width} x {this.Height}");

            // The canvas's actual rendered size (usually what you want)
            Debug.WriteLine($"WindowSizeChanged - Canvas: {GameCanvas.ActualWidth} x {GameCanvas.ActualHeight}");

            // The inner content area of the window (excludes title bar/borders)
            Debug.WriteLine($"WindowSizeChanged - Content: {this.ActualWidth} x {this.ActualHeight}");
        }

        private void GameCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // compute game canvas first
            double w = GameCanvas.ActualWidth;
            double h = GameCanvas.ActualHeight;

            _tileSize = Math.Min(w / 10.0, h / 20.0);
            _topMargin = (h - 20 * _tileSize) / 2.0;
            _leftMargin = (w - 10 * _tileSize) / 2.0;

            // now computer next canvas
            w = NextCanvas.ActualWidth;
            h = NextCanvas.ActualHeight;
            _nextTileSize = Math.Min(w / 1.5, h / 4.0);
            _nextTopMargin = _nextTileSize * 0.25;
            _nextLeftMargin = (w - _nextTileSize) * 0.5;

            // redraw everything
            RedrawGame();

            // clip everything outside the 20x10 grid
            GameCanvas.Clip = new RectangleGeometry
            {
                Rect = new Rect(_leftMargin, _topMargin, _tileSize*10, _tileSize*20)
            };
        }

        private void FrameTimer_Tick(object? sender, EventArgs e)
        {
            if (gameController.GameOver)
            {
                _frameTimer.Stop();
                _gameOver = true;
                _gameStarted = false;
                AnnounceGameOver();
            }
            else
            {
                PollKeyboard();
                PollController();
                if (_dropStopwatch.Elapsed.TotalMilliseconds >= _dropIntervalMs)
                {
                    gameController.MoveBrickDown(false);
                    _dropStopwatch.Restart();
                }
                gameController.UpdateGhost();
                RedrawGame();
            }
        }

        private void RedrawGame()
        {
            SolidColorBrush veryDarkGray = new SolidColorBrush(Color.FromRgb(48, 48, 48));

            GameCanvas.Children.Clear();

            // draw grid
            for (int row = 0; row < _gridRows-2; row++)
                for (int col = 0; col < _gridColumns; col++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = _tileSize;
                    r.Height = _tileSize;
                    r.Stroke = veryDarkGray;
                    r.Fill = Brushes.Black;
                    Canvas.SetLeft(r, _leftMargin + col * _tileSize);
                    Canvas.SetTop(r, _topMargin + row * _tileSize);
                    GameCanvas.Children.Add(r);
                }

            // set image controls size and position
            for (int row = 0; row < _gridRows; row++)
                for (int col = 0; col < _gridColumns; col++)
                {
                    imageControls[row, col].Width = _tileSize;
                    imageControls[row, col].Height = _tileSize;
                    Canvas.SetTop(imageControls[row, col], _topMargin + (row - 2) * _tileSize);
                    Canvas.SetLeft(imageControls[row, col], _leftMargin + col * _tileSize);
                    GameCanvas.Children.Add(imageControls[row, col]);
                }

            // draw occupied cells
            DrawGrid(gameController.Grid);
            DrawBrick(gameController.GhostBrick);
            DrawBrick(gameController.CurrentBrick);

            UpdateStats();

            // draw next display
            NextCanvas.Children.Clear();
            int[] queue = gameController.BrickQue.pieceQueue.PeekNext(3);
            if (_gameStarted && queue.Length >= 3)
            {
                for (int i=0; i<3; i++)
                {
                    Image img = new Image();
                    img.Source = brickImages[queue[i]+1];
                    img.Width = _nextTileSize;
                    img.Height = _nextTileSize;
                    Canvas.SetLeft(img, _nextLeftMargin);
                    Canvas.SetTop(img, _nextTopMargin + i * (_nextTileSize * 1.25));
                    NextCanvas.Children.Add(img);
                }
            }

            // draw hold display
            HoldCanvas.Children.Clear();
            if (_gameStarted && gameController.OnHoldBrick != null)
            {
                int type = (int)gameController.OnHoldBrick.Type;
                Image img = new Image();
                img.Source = brickImages[type];
                img.Width = _nextTileSize;
                img.Height = _nextTileSize;
                Canvas.SetLeft(img, _nextLeftMargin);
                Canvas.SetTop(img, _nextTopMargin);
                HoldCanvas.Children.Add(img);
            }
        }

        private void DrawGrid(Playfield grid)
        {
            for (int row = 0; row < grid.Rows; row++)
            {
                for (int column = 0; column < grid.Columns; column++)
                {
                    int cellType = grid[row, column];
                    imageControls[row, column].Source = cellImages[cellType];
                }
            }
        }

        private void DrawBrick(Tetromino brick)
        {
            foreach (Cell cell in brick.CellPositionsOnGrid())
            {
                if (gameController.Grid.IndexExists(cell.Row, cell.Column))
                    imageControls[cell.Row, cell.Column].Source = cellImages[brick.Color];
            }
        }

        private void UpdateStats()
        {
            ScoreText.Text = gameController.Score.ToString("N0");
            LevelText.Text = gameController.Level.ToString();
            LinesText.Text = gameController.LinesCleared.ToString();

            if (gameController.Level > _previousLevel)
            {
                _dropIntervalMs = Math.Max(_dropIntervalMs * 0.935, 100);
                _previousLevel = gameController.Level;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameController.GameOver)
            {
                return;
            }
            if (e.Key == Key.Escape)
            {
                Pause_Click(this, new RoutedEventArgs());
            }
            RedrawGame();
        }

        private async Task Countdown()
        {
            // call Countdown from start game, resume from pause, and activate window
            for (int i = 3; i >= 1; i--)
            {
                CountdownText.Text = i.ToString();
                CountdownText.Visibility = Visibility.Visible;
                await Task.Delay(1000);
            }
            CountdownText.Visibility = Visibility.Collapsed;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help dialog = new Help();
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();
        }

        private void AnnounceGameOver()
        {
            RedrawGame();
            _gameCompletion = DateTime.Now;
            RecordHighScore(gameController.Score, _gameCompletion);

            PlaySoundEffect("gameover");
            GameOver dialog = new GameOver(gameController.Score);
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();
        }

        private void RecordHighScore(int newScore, DateTime completionTime)
        {
            List<Score> scores = HighScores.ReadScores();

            for (int i = 0; i < scores.Count; i++)
            {
                if (newScore > scores[i].HighScore)
                {
                    for (int j = scores.Count - 1; j > i; j--)
                    {
                        scores[j].HighScore = scores[j - 1].HighScore;
                        scores[j].DateOfScore = scores[j - 1].DateOfScore;
                    }
                    scores[i].HighScore = newScore;
                    scores[i].DateOfScore = completionTime;
                    break;
                }
            }

            string json = JsonSerializer.Serialize(scores);
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string asteroidsFolder = System.IO.Path.Combine(appDataFolder, "Radish");
            asteroidsFolder = System.IO.Path.Combine(asteroidsFolder, "Asteroids");
            string filePath = System.IO.Path.Combine(asteroidsFolder, "highscores.json");
            Directory.CreateDirectory(asteroidsFolder); // ensure the folder exists first
            File.WriteAllText(filePath, json);
        }

        private void PollKeyboard()
        {
            bool success = false;
            // handle left arrow : move left
            bool leftIsDown = Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.NumPad4);
            if (leftIsDown && !_inputs.Keyboard.MoveLeft.IsHeld)
            {
                success = gameController.MoveBrickLeft(); // fire immediately on initial press
                if (success) PlaySoundEffect("move");
                _inputs.Keyboard.MoveLeft.Press();
            }
            else if (!leftIsDown)
            {
                _inputs.Keyboard.MoveLeft.Release();
            }
            else if (_inputs.Keyboard.MoveLeft.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
            {
                success = gameController.MoveBrickLeft();
                if (success) PlaySoundEffect("move");
            }

            // handle right arrow : move right
            bool rightIsDown = Keyboard.IsKeyDown(Key.Right) || Keyboard.IsKeyDown(Key.NumPad6);
            if (rightIsDown && !_inputs.Keyboard.MoveRight.IsHeld)
            {
                success = gameController.MoveBrickRight(); // fire immediately on initial press
                if (success) PlaySoundEffect("move");
                _inputs.Keyboard.MoveRight.Press();
            }
            else if (!rightIsDown)
            {
                _inputs.Keyboard.MoveRight.Release();
            }
            else if (_inputs.Keyboard.MoveRight.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
            {
                success = gameController.MoveBrickRight();
                if (success) PlaySoundEffect("move");
            }

            // handle down arrow : soft drop
            bool downIsDown = Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.NumPad2);
            if (downIsDown && !_inputs.Keyboard.SoftDrop.IsHeld)
            {
                gameController.MoveBrickDown(true); // fire immediately on initial press
                _inputs.Keyboard.SoftDrop.Press();
            }
            else if (!downIsDown)
            {
                _inputs.Keyboard.SoftDrop.Release();
            }
            else if (_inputs.Keyboard.SoftDrop.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
            {
                gameController.MoveBrickDown(true);
            }

            // handle up arrow : rotate right
            bool rotateRightIsDown = Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.NumPad8);
            if (_inputs.Keyboard.RotateRight.JustPressed(rotateRightIsDown))
            {
                success = gameController.RotateBrickCW();
                if (success) PlaySoundEffect("rotate");
            }

            // handle Z : rotate left
            bool rotateLeftIsDown = Keyboard.IsKeyDown(Key.Z);
            if (_inputs.Keyboard.RotateLeft.JustPressed(rotateLeftIsDown))
            {
                success = gameController.RotateBrickAntiCW();
                if (success) PlaySoundEffect("rotate");
            }

            // handle space : hard drop
            bool hardDropIsDown = Keyboard.IsKeyDown(Key.Space);
            if (_inputs.Keyboard.HardDrop.JustPressed(hardDropIsDown))
            {
                success = gameController.HardDrop();
            }

            // handle C : hold
            bool holdIsDown = Keyboard.IsKeyDown(Key.C);
            if (_inputs.Keyboard.Hold.JustPressed(holdIsDown))
            {
                success = gameController.Hold();
            }
        }

        private void PollController()
        {
            bool success = false;
            if (_controller == null) return;
            _controller.Poll();
            if (_controller.IsConnected)
            {
                // handle DPadLeft : move left
                bool leftIsDown = _controller.WasButtonJustPressed(XInputButtons.DPadLeft) || _controller.IsButtonHeld(XInputButtons.DPadLeft);
                if (leftIsDown && !_inputs.Controller.MoveLeft.IsHeld)
                {
                    success = gameController.MoveBrickLeft(); // fire immediately on initial press
                    if (success) PlaySoundEffect("move");
                    _inputs.Controller.MoveLeft.Press();
                }
                else if (!leftIsDown)
                {
                    _inputs.Controller.MoveLeft.Release();
                }
                else if (_inputs.Controller.MoveLeft.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
                {
                    success = gameController.MoveBrickLeft();
                    if (success) PlaySoundEffect("move");
                }

                // handle DPadRight : move right
                bool rightIsDown = _controller.WasButtonJustPressed(XInputButtons.DPadRight) || _controller.IsButtonHeld(XInputButtons.DPadRight);
                if (rightIsDown && !_inputs.Controller.MoveRight.IsHeld)
                {
                    success = gameController.MoveBrickRight(); // fire immediately on initial press
                    if (success) PlaySoundEffect("move");
                    _inputs.Controller.MoveRight.Press();
                }
                else if (!rightIsDown)
                {
                    _inputs.Controller.MoveRight.Release();
                }
                else if (_inputs.Controller.MoveRight.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
                {
                    success = gameController.MoveBrickRight();
                    if (success) PlaySoundEffect("move");
                }

                // handle DPadDown : soft drop
                bool downIsDown = _controller.WasButtonJustPressed(XInputButtons.DPadDown) || _controller.IsButtonHeld(XInputButtons.DPadDown);
                if (downIsDown && !_inputs.Controller.SoftDrop.IsHeld)
                {
                    gameController.MoveBrickDown(true); // fire immediately on initial press
                    _inputs.Controller.SoftDrop.Press();
                }
                else if (!downIsDown)
                {
                    _inputs.Controller.SoftDrop.Release();
                }
                else if (_inputs.Controller.SoftDrop.ShouldRepeat(_gameData.AutoRepeatDelay, _gameData.AutoRepeatSpeed))
                {
                    gameController.MoveBrickDown(true);
                }

                // handle B : rotate right
                bool rotateRightIsDown = _controller.WasButtonJustPressed(XInputButtons.B) || _controller.IsButtonHeld(XInputButtons.B);
                if (_inputs.Controller.RotateRight.JustPressed(rotateRightIsDown))
                {
                    success = gameController.RotateBrickCW();
                    if (success) PlaySoundEffect("rotate");
                }

                // handle X : rotate left
                bool rotateLeftIsDown = _controller.WasButtonJustPressed(XInputButtons.X) || _controller.IsButtonHeld(XInputButtons.X);
                if (_inputs.Controller.RotateLeft.JustPressed(rotateLeftIsDown))
                {
                    success = gameController.RotateBrickAntiCW();
                    if (success) PlaySoundEffect("rotate");
                }

                // handle DPadUp : hard drop
                bool hardDropIsDown = _controller.WasButtonJustPressed(XInputButtons.DPadUp) || _controller.IsButtonHeld(XInputButtons.DPadUp);
                if (_inputs.Controller.HardDrop.JustPressed(hardDropIsDown))
                {
                    success = gameController.HardDrop();
                }

                // handle Start : pause
                bool pauseIsDown = _controller.WasButtonJustPressed(XInputButtons.Start) || _controller.IsButtonHeld(XInputButtons.Start);
                if (_inputs.Controller.Pause.JustPressed(pauseIsDown))
                {
                    Pause_Click(this, new RoutedEventArgs());
                }

                // handle RightShoulder : hold
                bool holdIsDown = _controller.WasButtonJustPressed(XInputButtons.RightShoulder) || _controller.IsButtonHeld(XInputButtons.RightShoulder);
                if (_inputs.Controller.Hold.JustPressed(holdIsDown))
                {
                    success = gameController.Hold();
                }

            }
        }

        private void PlaySoundEffect(string sound)
        {
            string resourcePath = $"pack://application:,,,/Tetris;component/audio/{sound}.wav";
            if (_isSoundOn)
            {
                Task.Run(() =>
                {
                    var uri = new Uri(resourcePath, UriKind.Absolute);
                    var resourceStream = Application.GetResourceStream(uri);
                    using (var reader = new WaveFileReader(resourceStream.Stream))
                    using (var output = new WaveOut())
                    {
                        output.Init(reader);
                        output.Volume = (float)(Properties.Settings.Default.EffectsVolume / 100.0);
                        output.Play();
                        while (output.PlaybackState == PlaybackState.Playing)
                            System.Threading.Thread.Sleep(100);
                    }
                });
            }
        }

    }

    public class GameData
    {
        public double AutoRepeatDelay { get; set; } = 170;
        public double AutoRepeatSpeed { get; set; } = 50;
    }

    public class InputSet
    {
        public RepeatableInput MoveLeft { get; set; } = new RepeatableInput();
        public RepeatableInput MoveRight { get; set; } = new RepeatableInput();
        public RepeatableInput SoftDrop { get; set; } = new RepeatableInput();
        public SingleFireInput RotateRight { get; set; } = new SingleFireInput();
        public SingleFireInput RotateLeft { get; set; } = new SingleFireInput();
        public SingleFireInput HardDrop { get; set; } = new SingleFireInput();
        public SingleFireInput Hold { get; set; } = new SingleFireInput();
        public SingleFireInput Pause { get; set; } = new SingleFireInput();
    }

    public class Inputs
    {
        public InputSet Keyboard { get; set; } = new InputSet();
        public InputSet Controller { get; set; } = new InputSet();
    }
}