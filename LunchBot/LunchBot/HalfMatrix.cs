using Newtonsoft.Json;

namespace LunchBot;

public partial class HalfMatrix<T>
{
    [JsonIgnore] public int Size => _matrix.Length + 1;
    [JsonIgnore] public int EntryCount => (_matrix.Length - 1) * _matrix.Length / 2;

    public T this[int x, int y]
    {
        get
        {
            if (x == y)
            {
                throw new ArgumentOutOfRangeException($"Can't get data for pair of same index {x}");
            }

            return x >= y ? _matrix[x - 1][y] : _matrix[y - 1][x];
        }
        set
        {
            if (x == y)
            {
                throw new ArgumentOutOfRangeException($"Can't set data for pair of same index {x}");
            }

            if (x >= y)
            {
                _matrix[x - 1][y] = value;
            }
            else
            {
                _matrix[y - 1][x] = value;
            }
        }
    }

    [JsonProperty]
    private readonly T[][] _matrix;

    public HalfMatrix(int size)
    {
        _matrix = new T[size - 1][];

        for (int i = 0; i < size - 1; i++)
        {
            _matrix[i] = new T[i + 1];
        }
    }

    [JsonConstructor]
    private HalfMatrix(T[][] matrix)
    {
        _matrix = matrix;
    }

    public bool TryGetValue(int x, int y, out T value)
    {
        value = default;

        if (x == y)
        {
            return false;
        }

        if (x < y)
        {
            (x, y) = (y, x);
        }

        int xi = x - 1;

        if (xi < 0 || y < 0 || xi >= _matrix.Length || y >= _matrix[xi].Length)
        {
            return false;
        }

        value = this[x, y];
        return true;
    }

    public IEnumerable<(int x, int y)> GetIterator()
    {
        return new Iterator(this);
    }
}