using Tetris.Enums;

namespace Tetris
{
    public abstract class Tetromino
    {
        protected abstract Cell[][] BlueprintTiles { get; }
        protected abstract Cell StartOffset { get; }
        public abstract TetrominoShape Type { get; }
        private readonly Cell offset;
        public int Row
        {
            get => offset.Row;
            set => offset.Row = value;
        }
        public int Col
        {
            get => offset.Column;
            set => offset.Column = value;
        }
        public RotationState rotationState { get; set; }
        public int Color { get; set; }

        protected Tetromino()
        {
            offset = new Cell(StartOffset.Row, StartOffset.Column);
            Color = (int)Type;
        }
        
        /* yield is cool new technique that makes it not neccesary to create a temp
         * variable and then return it in the end. also its more memory efficient
        */
        public IEnumerable<Cell> CellPositionsOnGrid()
        {
            var blah = (int)rotationState;
            foreach (Cell cell in BlueprintTiles[(int)rotationState])
            {
                yield return new Cell(cell.Row + offset.Row, cell.Column + offset.Column);
            }
        }

        public void RotateClockwise()
        {
            rotationState = (RotationState)( ( (int)rotationState +1 ) % BlueprintTiles.Length );
        }

        public void RotateAntiClockwise()
        {
            if (rotationState == RotationState.originalState)
                rotationState = (RotationState)BlueprintTiles.Length -1;
            else
                rotationState = (RotationState)(int)rotationState - 1;
        }

        public void Move(int rows, int columns)
        {
            offset.Row += rows;
            offset.Column += columns;
        }

        public void ResetPosition()
        {
            rotationState = RotationState.originalState;
            offset.Row = StartOffset.Row;
            offset.Column = StartOffset.Column;
        }
    }
}
