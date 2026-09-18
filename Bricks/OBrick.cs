using Tetris.Enums;

namespace Tetris.Bricks
{
    class OBrick: Tetromino
    {
       private readonly Cell[][] originalTiles = new Cell[][]
       {
            new Cell[] { new Cell(0,0), new Cell(0,1), new Cell(1,0), new Cell(1,1) }
       };

        protected override Cell[][] BlueprintTiles => originalTiles;
        protected override Cell StartOffset => new Cell(0, 4);
        public override TetrominoShape Type => TetrominoShape.O;
    }
}
