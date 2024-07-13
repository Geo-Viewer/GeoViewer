namespace GeoViewer.Controller.Util
{
    public class AvgBufferInt
    {
        public int Size { get; }

        private readonly int[] _elements;

        private int _currentIndex;

        public float Average { get; private set; }

        public AvgBufferInt(int size)
        {
            Size = size;
            _elements = new int[Size];
        }

        public void Add(int number)
        {
            var removed = _elements[_currentIndex];
            _elements[_currentIndex] = number;

            Average += (float)(number - removed) / Size;

            _currentIndex = (_currentIndex + 1) % Size;
        }
    }
}