namespace LunchBot;

public class UserMatrix
{
    public Guid Id { get; }
    public HalfMatrix<int> Matrix { get; }

    private readonly UserIndexer _indexer;

    public UserMatrix(Guid id, UserIndexer indexer)
    {
        Id = id;
        _indexer = indexer;
        Matrix = new HalfMatrix<int>(indexer.MaxIndex + 1);
    }

    public UserMatrix(Guid id, UserIndexer indexer, HalfMatrix<int> matrix)
    {
        Id = id;
        Matrix = matrix;
        _indexer = indexer;
    }

    public bool TryGetMeetCount(string id0, string id1, out int meetCount)
    {
        int index0 = _indexer.GetIndexForId(id0);
        int index1 = _indexer.GetIndexForId(id1);

        meetCount = Matrix[index0, index1];
        return true;
    }

    public void Add(Party party)
    {
        foreach (Group group in party.Groups)
        {
            UpdateForGroup(group);
        }
    }

    private void UpdateForGroup(Group group)
    {
        for (int i = 0; i < group.Users.Count; i++)
        {
            for (int j = i + 1; j < group.Users.Count; j++)
            {
                string id0 = group.Users[i].Id;
                string id1 = group.Users[j].Id;

                Increment(id0, id1);
            }
        }
    }

    private void Increment(string id0, string id1, int amount = 1)
    {
        int index0 = _indexer.GetIndexForId(id0);
        int index1 = _indexer.GetIndexForId(id1);

        Matrix[index0, index1] += amount;
    }
}