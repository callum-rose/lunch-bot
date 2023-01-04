namespace LunchBot;

public class LunchedAndDepartmentCapSameTeamPartyScorer : IPartyScorer
{
	private const int MaxSameTeamUsersPerGroup = 2;
	
	private readonly LunchedAndDepartmentPartyScorer _lunchedAndDepartmentPartyScorer;

	public LunchedAndDepartmentCapSameTeamPartyScorer(LunchedAndDepartmentPartyScorer lunchedAndDepartmentPartyScorer)
	{
		_lunchedAndDepartmentPartyScorer = lunchedAndDepartmentPartyScorer;
	}

	public Task Initialise(IReadOnlyList<MyUser> users)
	{
		return _lunchedAndDepartmentPartyScorer.Initialise(users);
	}

	public double ScoreGroup(Group group)
	{
		if (GroupHasTooManyUsersOfSameDepartment(group))
		{
			return 1e6;
		}

		return _lunchedAndDepartmentPartyScorer.ScoreGroup(group);
	}

	public bool IsScoreBetter(double newScore, double? oldScore)
	{
		return _lunchedAndDepartmentPartyScorer.IsScoreBetter(newScore, oldScore);
	}

	public bool IsAcceptable(Party party)
	{
		return party.All(g => !GroupHasTooManyUsersOfSameDepartment(g));
	}

	private bool GroupHasTooManyUsersOfSameDepartment(Group group)
	{
		return group.GroupBy(u => u.Department).Max(g => g.Count()) > MaxSameTeamUsersPerGroup;
	}
}