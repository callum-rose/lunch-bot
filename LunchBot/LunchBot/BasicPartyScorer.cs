namespace LunchBot;

internal class BasicPartyScorer : IPartyScorer
{
    public Task Initialise(IReadOnlyList<MyUser> readOnlyList)
    {
        return Task.CompletedTask;
    }

    public double ScoreGroup(Group group)
    {
        int departmentPenalty = group
            .GroupBy(u => u.Department)
            .Sum(g => g.Count() - 1);

        return -departmentPenalty;
    }
    
    public bool IsScoreBetter(double newScore, double? oldScore)
    {
        return newScore > (oldScore ?? double.MinValue);
    }
}