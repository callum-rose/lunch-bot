using Microsoft.Extensions.Configuration;

namespace LunchBot;

public class SwapGroupSelector : IGroupSelector
{
    private class DynamicGroup : Group
    {
        public List<MyUser> UpdateableUsers { get; }
    
        public DynamicGroup(List<MyUser> users, int number) : base(users, number)
        {
            UpdateableUsers = users;
        }
    }
    
    public int Iterations { get; }
    
    private readonly IGroupSizer _groupSizer;
    private readonly IPartyScorer _partyScorer;
    
    public SwapGroupSelector(IGroupSizer groupSizer, IPartyScorer partyScorer, IConfigurationRoot configuration)
    {
        _groupSizer = groupSizer;
        _partyScorer = partyScorer;
        Iterations = configuration.GetValue<int>("GroupSelectedIterationCount");
    }

    public Party Choose(IReadOnlyList<MyUser> users, int? seed = null)
    {
        DynamicGroup[] userGroups = GetRandomGroups(users, seed);

        Party party = new(userGroups);
        
        double? bestScore = null;
        int scoreStagnantForIterations = 0;

        while (scoreStagnantForIterations < 5)
        {
            (double? score, DynamicGroup group) worst = (null, null);
            (double? score, DynamicGroup group) best = (null, null);

            foreach (DynamicGroup group in userGroups)
            {
                double score = _partyScorer.ScoreGroup(group);

                bool betterThanBest = _partyScorer.IsScoreBetter(score, best.score);

                if (!best.score.HasValue || betterThanBest)
                {
                    best = (score, group);
                }

                bool worseThanWorst = !_partyScorer.IsScoreBetter(score, worst.score);

                if (!worst.score.HasValue || worseThanWorst)
                {
                    worst = (score, group);
                }
            }

            (double? score, int i0, int i1) bestCombo = (null, 0, 0);

            for (int bestI = 0; bestI < best.group!.UpdateableUsers.Count; bestI++)
            {
                for (int worstI = 0; worstI < worst.group!.UpdateableUsers.Count; worstI++)
                {
                    List<MyUser> worstUsers = worst.group.Users.ToList();
                    List<MyUser> bestUsers = best.group.Users.ToList();

                    MyUser worstUserToSwap = worstUsers[worstI];
                    MyUser bestUserToSwap = bestUsers[bestI];

                    worstUsers.RemoveAt(worstI);
                    bestUsers.RemoveAt(bestI);

                    worstUsers.Add(bestUserToSwap);
                    bestUsers.Add(worstUserToSwap);

                    DynamicGroup testGroup0 = new(worstUsers, worst.group.Number);
                    DynamicGroup testGroup1 = new(worstUsers, best.group.Number);

                    double score0 = _partyScorer.ScoreGroup(testGroup0);
                    double score1 = _partyScorer.ScoreGroup(testGroup1);
                    double aggregate = score0 + score1;

                    if (_partyScorer.IsScoreBetter(aggregate, bestCombo.score))
                    {
                        bestCombo = (aggregate, bestI, worstI);
                    }
                }
            }

            MyUser bestUser = best.group.UpdateableUsers[bestCombo.i0];
            MyUser worstUser = worst.group.UpdateableUsers[bestCombo.i1];

            best.group.UpdateableUsers.RemoveAt(bestCombo.i0);
            worst.group.UpdateableUsers.RemoveAt(bestCombo.i1);

            best.group.UpdateableUsers.Add(worstUser);
            worst.group.UpdateableUsers.Add(bestUser);

            party = new Party(userGroups);

            double partyScore = _partyScorer.ScoreParty(party);

            if (_partyScorer.IsScoreBetter(partyScore, bestScore))
            {
                bestScore = partyScore;
                scoreStagnantForIterations = 0;
            }
            else
            {
                scoreStagnantForIterations++;
            }
        }
        
        return new Party(party.Select(g => new Group(g.Users, g.Number)).ToArray());
    }

    private DynamicGroup[] GetRandomGroups(IReadOnlyList<MyUser> users, int? seed)
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

        return userGroups.Select((g, i) => new DynamicGroup(g.ToList(), i)).ToArray();
    }
}