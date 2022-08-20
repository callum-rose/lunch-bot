namespace LunchBot;

internal class RandomGroupSelector : IGroupSelector
{
    public int Iterations { get; }
    
    private readonly IGroupSizer _groupSizer;

    public RandomGroupSelector(IGroupSizer groupSizer, int loops)
    {
        _groupSizer = groupSizer;
        Iterations = loops;
    }

    public Party Choose(IReadOnlyList<MyUser> users, int? seed = null)
    {
        int[] groupSizes = _groupSizer.GetGroupSizes(users.Count);
        int groupCount = groupSizes.Length;
        
        MyUser[][] userGroups = new MyUser[groupCount][];
        int takenCount = 0;

        MyUser[] shuffledUsers = users.Shuffle(seed).ToArray();
        
        for (int i = 0; i < groupCount; i++)
        {
            int groupSize = groupSizes[i];
            userGroups[i] = shuffledUsers.Skip(takenCount).Take(groupSize).ToArray();
            takenCount += groupSize;
        }

        Group[] groups = userGroups.Select((g, i) => new Group(g, i)).ToArray();
        return new Party(groups);
    }
}