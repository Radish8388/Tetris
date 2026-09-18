using Tetris.Bricks;
using Tetris.Enums;

namespace Tetris
{
    class TetrominoQueue
    {
        private static readonly Func<Tetromino>[] factories = new Func<Tetromino>[]
        {
            () => new IBrick(),
            () => new OBrick(),
            () => new TBrick(),
            () => new JBrick(),
            () => new LBrick(),
            () => new SBrick(),
            () => new ZBrick()
        };

        public PieceQueue pieceQueue;

        public TetrominoQueue(int pieces)
        {
            pieceQueue = new PieceQueue(pieces);
        }
        public Tetromino GetNextBlock()
        {
            int i = pieceQueue.Dequeue();
            return factories[i]();
        }

        public Tetromino GetBlockOfType(TetrominoShape shape)
        {
            int i = (int)shape;
            return factories[i-1]();
        }
    }

    public class PieceQueue
    {
        private readonly Queue<int> _queue = new Queue<int>();
        private readonly Random _rng = new Random();
        private readonly int _pieceCount; // N
        private readonly int _minBuffer;  // how many items to always keep ready

        public PieceQueue(int pieceCount, int minBuffer = 7)
        {
            _pieceCount = pieceCount;
            _minBuffer = minBuffer;
            FillIfNeeded();
        }

        public int Dequeue()
        {
            FillIfNeeded();
            return _queue.Dequeue();
        }

        // Peek at the next `count` pieces without removing them (for the preview display)
        public int[] PeekNext(int count)
        {
            FillIfNeeded();
            return _queue.Take(count).ToArray();
        }

        private void FillIfNeeded()
        {
            while (_queue.Count < _minBuffer)
            {
                foreach (int piece in ShuffledBag())
                    _queue.Enqueue(piece);
            }
        }

        private int[] ShuffledBag()
        {
            int[] bag = Enumerable.Range(0, _pieceCount).ToArray();

            // Fisher-Yates shuffle
            for (int i = bag.Length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (bag[i], bag[j]) = (bag[j], bag[i]);
            }

            return bag;
        }
    }
}
