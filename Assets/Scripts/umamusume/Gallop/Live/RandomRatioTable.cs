namespace Gallop.Live
{
    public class RandomRatioTable
    {
        private RandomNumber _randomNumber; // 0x10
        private int _randomSeed; // 0x18
        private float[,] _randomValueArray; // 0x20
        private int _majorLength; // 0x28
        private int _minorLength; // 0x2C
    }
}
