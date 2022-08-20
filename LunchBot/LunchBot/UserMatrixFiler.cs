using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LunchBot;

public class UserMatrixFiler
{
    private readonly string _directory;

    private const string Extension = ".usermatrixdata";
    private const string DebugSuffix = "_debug";

    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented
    };

    public UserMatrixFiler(IConfigurationRoot configuration)
    {
        _directory = configuration.GetValue<string>("OutputDirectory");
    }

    public async Task<UserMatrix> LoadCumulative(UserIndexer indexer, IEnumerable<Guid> deliveredPartyIds)
    {
        Directory.CreateDirectory(_directory);

        Task<string>[] readTasks = Directory.EnumerateFiles(_directory)
            .Where(p => Path.GetExtension(p) == Extension)
            .Where(p => !p.Contains(DebugSuffix))
            .Where(p =>
            {
                string fileName = Path.GetFileNameWithoutExtension(p);
                
                if (!Guid.TryParse(fileName, out Guid guid))
                {
                    return false;
                }

                bool hasBeenDelivered = deliveredPartyIds.Any(i => i == guid);
                return hasBeenDelivered;
            })
            .Select(async p => await File.ReadAllTextAsync(p))
            .ToArray();

        if (readTasks.Length == 0)
        {
            return new UserMatrix(Guid.Empty, indexer);
        }

        await Task.WhenAll(readTasks);

        List<HalfMatrix<int>> matrices = new();

        foreach (string json in readTasks.Select(t => t.Result))
        {
            HalfMatrix<int> matrix = JsonConvert.DeserializeObject<HalfMatrix<int>>(json, _settings);
            matrices.Add(matrix);
        }

        int maxSize = indexer.MaxIndex + 1;
        HalfMatrix<int> aggregatedMatrix = new(maxSize);

        for (int x = 0; x < maxSize; x++)
        {
            for (int y = 0; y < x; y++)
            {
                int sum = matrices.Sum(m => m.TryGetValue(x, y, out int value) ? value : 0);
                aggregatedMatrix[x, y] = sum;
            }
        }

        return new UserMatrix(Guid.Empty, indexer, aggregatedMatrix);
    }

    public async Task Save(UserMatrix userMatrix, bool isCumulative = false)
    {
        Directory.CreateDirectory(_directory);

        string json = JsonConvert.SerializeObject(userMatrix.Matrix);

        if (isCumulative)
        {
            string cumulativePath = Path.Combine(_directory, "cumulative" + Extension);
            await File.WriteAllTextAsync(cumulativePath, json);
            return;
        }

        string GetPath(int? number)
        {
            string fileName = userMatrix.Id
                              + (number.HasValue ? $"_{number.Value}" : "")
                              + Extension;
            return Path.Combine(_directory, fileName);
        }

        string path = GetPath(null);

        for (int i = 1; File.Exists(path); i++)
        {
            path = GetPath(i);
        }

        await File.WriteAllTextAsync(path!, json);
    }
}