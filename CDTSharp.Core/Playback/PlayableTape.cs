namespace CDTSharp.Core.Playback
{
    public class PlayableTape {
        private readonly CDTFile _fileData;

        private int _blockIndex = -1;
        private IBlockPlayer _currentBlockPlayer;

        private bool _currentState = false;

        public PlayableTape(CDTFile fileData) {
            _fileData = fileData;
        }

        /// <summary>
        /// Returns the amplitude to set for the tape reading bit that's read by the CPU
        /// </summary>
        /// <returns>True for high amplitude, false for low amplitude</returns>
        public bool ClockTick() {
            while (_currentBlockPlayer == null && _blockIndex < _fileData.Blocks.Length - 1) {
                // Create a new block player
                _blockIndex++;
                var currentBlock = _fileData.Blocks[_blockIndex];
                _currentBlockPlayer = currentBlock.CreateBlockPlayer(_currentState);
                
                System.Console.WriteLine($"Changing block: {currentBlock.Description}");
            }

            if (_currentBlockPlayer != null) {
                _currentState = _currentBlockPlayer.ClockTick();
                if (_currentBlockPlayer.IsComplete) {
                    _currentBlockPlayer = null;
                }
                return _currentState;
            }
            //System.Console.WriteLine("Tape finished");
            return false;
        }

        public void Rewind() {
            _currentBlockPlayer = null;
            _blockIndex = 0;
        }
    }
}
