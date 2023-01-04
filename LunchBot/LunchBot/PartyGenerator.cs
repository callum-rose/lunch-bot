using Serilog;
using ShellProgressBar;

namespace LunchBot;

public class PartyGenerator
{
    private readonly IGroupSelector _groupSelector;
    private readonly IPartyScorer _partyScorer;
    private readonly ILogger _logger;

    private volatile int _progress;

    public PartyGenerator(IGroupSelector groupSelector, IPartyScorer partyScorer, ILogger logger)
    {
        _groupSelector = groupSelector;
        _partyScorer = partyScorer;
        _logger = logger;
    }

    public async Task<PartyData> Generate(IReadOnlyList<MyUser> users)
    {
        _logger.Information("Starting party generation");

        using (LogWatch.Start(nameof(Generate), _logger))
        {
            try
            {
                await _partyScorer.Initialise(users);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to init {nameof(_partyScorer)}");
                return PartyData.Invalid;
            }

            int offset = new Random().Next();
            MyUser[] allUsersArray = users.ToArray();

            int bestSeed = await GetBestPartySeed(offset, allUsersArray);

            Party bestParty = _groupSelector.Choose(users.ToArray(), bestSeed);
            
            _logger.Information($"Best party found. Seed {bestSeed}");
            
            if (!_partyScorer.IsAcceptable(bestParty))
            {
                _logger.Error("Best party has failed the acceptability test");
            }

            return new PartyData(Guid.NewGuid(), bestSeed, bestParty);
        }
    }

    private async Task<int> GetBestPartySeed(int offset, MyUser[] allUsersArray)
    {
        (double? score, int seed) best = (null, -1);

        ProgressBarOptions options = new()
        {
            ForegroundColor = ConsoleColor.White,
            DisplayTimeInRealTime = true
        };

        using (ProgressBar progressBar = new(_groupSelector.Iterations, "Crunching", options))
        {
            IProgress<double> progress = progressBar.AsProgress<double>();

            _progress = 0;

            Task<(double score, int seed)>[] tasks = Enumerable.Range(offset, _groupSelector.Iterations)
                .Select(i => Task.Run(() => GeneratePartyInternal(i, allUsersArray, progress)))
                .ToArray();

            await Task.WhenAll(tasks);

            foreach ((double score, int seed) result in tasks.Select(t => t.Result))
            {
                if (_partyScorer.IsScoreBetter(result.score, best.score))
                {
                    best = result;
                }
            }
        }

        return best.seed;
    }

    private (double score, int seed) GeneratePartyInternal(int seed, IReadOnlyList<MyUser> users,
        IProgress<double> progress)
    {
        Party party = _groupSelector.Choose(users, seed);
        double score = _partyScorer.ScoreParty(party);

        Interlocked.Increment(ref _progress);
        progress.Report((double)_progress / _groupSelector.Iterations);

        return (score, seed);
    }
}