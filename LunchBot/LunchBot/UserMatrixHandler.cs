namespace LunchBot;

public class UserMatrixHandler
{
    private readonly UserMatrixFiler _userMatrixFiler;
    private readonly UserIndexerHandler _userIndexerHandler;
    private readonly LunchDataFiler _lunchDataFiler;

    public UserMatrixHandler(UserMatrixFiler userMatrixFiler, UserIndexerHandler userIndexerHandler, LunchDataFiler lunchDataFiler)
    {
        _userMatrixFiler = userMatrixFiler;
        _userIndexerHandler = userIndexerHandler;
        _lunchDataFiler = lunchDataFiler;
    }

    public async Task<UserMatrix> GetCumulative(bool includeDryRuns = false)
    {
        UserIndexer indexer = await _userIndexerHandler.Get();
        IEnumerable<LunchData> lunchDatas = await _lunchDataFiler.LoadAll(includeDryRuns);
        return await _userMatrixFiler.LoadCumulative(indexer, lunchDatas.Select(l => l.PartyDataId));
    }

    public async Task SaveSingle(PartyData partyData)
    {
        UserIndexer indexer = await _userIndexerHandler.Get();
        UserMatrix userMatrix = new(partyData.Id, indexer);
        userMatrix.Add(partyData.Party);
        await _userMatrixFiler.Save(userMatrix);
    }

    public async Task AddAndSaveCumulative(Party party)
    {
        UserMatrix cumulative = await GetCumulative();
        cumulative.Add(party);
        await _userMatrixFiler.Save(cumulative, true);
    }
}