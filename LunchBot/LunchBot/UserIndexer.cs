using Newtonsoft.Json;

namespace LunchBot;

public class UserIndexer
{
    [JsonIgnore]
    public int MaxIndex => _idToIndex.Count > 0 ? _idToIndex.Values.Max() : -1;
    
    [JsonProperty] private readonly Dictionary<string, int> _idToIndex;

    public UserIndexer()
    {
        _idToIndex = new Dictionary<string, int>();
    }

    [JsonConstructor]
    private UserIndexer(Dictionary<string, int> idToIndex)
    {
        _idToIndex = idToIndex;
    }

    /// <summary>
    /// Adds users that aren't already indexed to the indexer
    /// </summary>
    /// <param name="users">Users to add</param>
    /// <returns>All the added users</returns>
    public IEnumerable<MyUser> AddAll(IEnumerable<MyUser> users)
    {
        List<MyUser> added = new();
        
        foreach (MyUser user in users)
        {
            if (!Contains(user.Id))
            {
                Add(user.Id);
                added.Add(user);
            }
        }

        return added;
    }

    public void Add(string id)
    {
        _idToIndex.TryAdd(id, MaxIndex + 1);
    }

    public bool Contains(string id)
    {
        return _idToIndex.ContainsKey(id);
    }

    public int GetIndexForId(string id)
    {
        return _idToIndex[id];
    }
    
    public bool TryGetIdForIndex(int index, out string id)
    {
        foreach (KeyValuePair<string,int> keyValuePair in _idToIndex)
        {
            if (keyValuePair.Value == index)
            {
                id = keyValuePair.Key;
                return true;
            }
        }

        id = string.Empty;
        return false;
    }
}