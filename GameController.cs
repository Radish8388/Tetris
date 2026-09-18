// based on code borrowed from SphiweNdou

namespace Tetris
{
    class GameController
    {
        public event Action<int> LinesClearedAction;
        public event Action PieceLandedAction;

        private Tetromino currentBrick;

        public Tetromino CurrentBrick
        {
            get
            {
                return currentBrick;
            }

            private set
            {
                currentBrick = value;
                currentBrick.ResetPosition();
            }
        }

        public Tetromino GhostBrick;

        public Tetromino? OnHoldBrick { get; private set; }

        public Playfield Grid { get; }
        public TetrominoQueue BrickQue { get; }
        public bool GameOver { get; private set; }
        public int LinesCleared { get; private set; }
        public int Level { get; set; }
        public int Score { get; private set; }
        private bool canHold = true;

        public GameController(int rows, int columns, int pieces)
        {
            Grid = new Playfield(rows, columns);
            BrickQue = new TetrominoQueue(pieces);
            CurrentBrick = BrickQue.GetNextBlock();
            GhostBrick = BrickQue.GetBlockOfType(CurrentBrick.Type);
            GhostBrick.Color = CurrentBrick.Color + 8;
            OnHoldBrick = null;
            LinesCleared = 0;
            Level = 1;
            Score = 0;
        }

        private bool BrickFits(Tetromino brick)
        {
            foreach (Cell cell in brick.CellPositionsOnGrid())
            {
                if (!Grid.cellIsEmpty(cell.Row, cell.Column))
                    return false;
            }

            return true;
        }

        public bool RotateBrickCW()
        {
            CurrentBrick.RotateClockwise();
            if (!BrickFits(CurrentBrick))
            {
                CurrentBrick.RotateAntiClockwise();
                return false;
            }
            else
            {
                return true;
            }

        }

        public bool RotateBrickAntiCW()
        {
            CurrentBrick.RotateAntiClockwise();
            if (!BrickFits(CurrentBrick))
            {
                CurrentBrick.RotateClockwise();
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool MoveBrickLeft()
        {
            CurrentBrick.Move(0, -1);

            if (!BrickFits(CurrentBrick))
            {
                CurrentBrick.Move(0, 1);
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool MoveBrickRight()
        {
            CurrentBrick.Move(0, 1);

            if (!BrickFits(CurrentBrick))
            {
                CurrentBrick.Move(0, -1);
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool IsGameOver()
        {
            return !(Grid.RowIsEmpty(0) && Grid.RowIsEmpty(1));
        }

        private void LandBlock()
        {
            foreach (Cell cell in CurrentBrick.CellPositionsOnGrid())
            {
                Grid[cell.Row, cell.Column] = (int)CurrentBrick.Type;
            }

            int clearedRows = Grid.MarkFullRows();

            if (clearedRows > 0)
            {
                LinesClearedAction?.Invoke(clearedRows);
            }
            else if (!IsGameOver())
            {
                PieceLandedAction?.Invoke();
            }

            ComputeClearScore(clearedRows);

            // level up every 10 rows cleared
            if ((LinesCleared + clearedRows) / 10 > LinesCleared / 10)
                Level++;

            LinesCleared += clearedRows;

            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBrick = BrickQue.GetNextBlock();
                GhostBrick = BrickQue.GetBlockOfType(CurrentBrick.Type);
                GhostBrick.Color = CurrentBrick.Color + 8;

                canHold = true;
            }
        }

        public void MoveBrickDown(bool isSoftDrop)
        {
            Grid.ClearMarkedRows();
            CurrentBrick.Move(1, 0);

            if (!BrickFits(CurrentBrick))
            {
                CurrentBrick.Move(-1, 0);
                LandBlock();
            }

            if (isSoftDrop)
                Score += 1 * 1 * Level;
        }

        private void ComputeClearScore(int clearedRows)
        {
            int newScore = 0;
            switch (clearedRows)
            {
                case 0: newScore = 0; break;
                case 1: newScore = 100 * Level; break;
                case 2: newScore = 300 * Level; break;
                case 3: newScore = 500 * Level; break;
                case 4: newScore = 800 * Level; break;
                default: newScore = 0; break;
            }
            Score += newScore;
        }

        private int FindLanding(Tetromino brick)
        {
            int landingRow = 0;

            for (int row=0; row<Grid.Rows; row++)
            {
                brick.Move(row, 0);
                if (BrickFits(brick))
                {
                    landingRow = row;
                }
                else
                {
                    landingRow = row - 1;
                    brick.Move(-row, 0);
                    break;
                }
                brick.Move(-row, 0);
            }

            return landingRow;
        }

        public bool HardDrop()
        {
            int rowsToMove = FindLanding(CurrentBrick);
            CurrentBrick.Move(rowsToMove, 0);
            LandBlock();
            Score += 2 * rowsToMove * Level;
            return true;
        }

        public bool Hold()
        {
            if (canHold)
            {
                if (OnHoldBrick == null)
                {
                    OnHoldBrick = CurrentBrick;
                    CurrentBrick = BrickQue.GetNextBlock();
                    GhostBrick = BrickQue.GetBlockOfType(CurrentBrick.Type);
                    GhostBrick.Color = CurrentBrick.Color + 8;
                }
                else
                {
                    Tetromino swap = OnHoldBrick;
                    OnHoldBrick = CurrentBrick;
                    CurrentBrick = swap;
                    GhostBrick = BrickQue.GetBlockOfType(CurrentBrick.Type);
                    GhostBrick.Color = CurrentBrick.Color + 8;
                }
                canHold = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateGhost()
        {
            GhostBrick.Row = CurrentBrick.Row;
            GhostBrick.Col = CurrentBrick.Col;
            GhostBrick.rotationState = CurrentBrick.rotationState;
            GhostBrick.Row += FindLanding(GhostBrick); // drop it to the landing row
        }
    }
}
