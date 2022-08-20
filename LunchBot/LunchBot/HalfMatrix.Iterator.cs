using System.Collections;

namespace LunchBot;

public partial class HalfMatrix<T>
{
    public class Iterator : IEnumerable<(int x, int y)>
    {
        private readonly HalfMatrix<T> _matrix;

        public Iterator(HalfMatrix<T> matrix)
        {
            _matrix = matrix;
        }

        public IEnumerator<(int x, int y)> GetEnumerator()
        {
            for (int x = 1; x <= _matrix._matrix.Length; x++)
            {
                for (int y = 0; y < _matrix._matrix[x - 1].Length; y++)
                {
                    if (x == y)
                    {
                        continue;
                    }
                    
                    yield return (x, y);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}