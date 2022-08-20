using Serilog;

namespace LunchBot;

public class UserIndexerHandler
{
    private readonly UserIndexerFiler _userIndexerFiler;
    private readonly ILogger _logger;
    private readonly Lazy<Task<UserIndexer>> _userIndexer;

    public UserIndexerHandler(UserIndexerFiler userIndexerFiler, ILogger logger)
    {
        _userIndexerFiler = userIndexerFiler;
        _logger = logger;
        _userIndexer = new Lazy<Task<UserIndexer>>(async () => await _userIndexerFiler.Load());
    }

    public async Task<UserIndexer> Get()
    {
        return await _userIndexer.Value;
    }

    public async Task AddUsersAndSave(IReadOnlyList<MyUser> users)
    {
        UserIndexer indexer = await Get();
        
        IEnumerable<MyUser> addedUsers = indexer.AddAll(users);
        
        if (addedUsers.Any())
        {
            _logger.Information($"Added users to indexer: {string.Join(", ", addedUsers)}");
        }
        
        await _userIndexerFiler.Save(indexer);
    }
}