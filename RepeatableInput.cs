using System.Diagnostics;

namespace Tetris
{
    public class RepeatableInput
    {
        public bool IsHeld { get; private set; }
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _hasRepeatedOnce;

        public void Press()
        {
            IsHeld = true;
            _hasRepeatedOnce = false;
            _stopwatch.Restart();
        }

        public void Release()
        {
            IsHeld = false;
            _stopwatch.Stop();
        }

        // Returns true when it's time to fire a move (initial press already
        // handled separately, this only governs the repeat behavior)
        public bool ShouldRepeat(double repeatDelayMs, double repeatSpeedMs)
        {
            if (!IsHeld)
                return false;

            double threshold = _hasRepeatedOnce ? repeatSpeedMs : repeatDelayMs;

            if (_stopwatch.Elapsed.TotalMilliseconds >= threshold)
            {
                _hasRepeatedOnce = true;
                _stopwatch.Restart();
                return true;
            }

            return false;
        }
    }

    public class SingleFireInput
    {
        private bool _wasDown;

        // Returns true only on the frame the key transitions from up to down
        public bool JustPressed(bool isDownNow)
        {
            bool fire = isDownNow && !_wasDown;
            _wasDown = isDownNow;
            return fire;
        }
    }
}
