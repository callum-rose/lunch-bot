namespace LunchBot;

public class LunchedAndDepartmentPartyScorer : IPartyScorer
{
	private readonly UserMatrixHandler _userMatrixHandler;

	private bool _isInitialised;
	private UserMatrix _userMatrix;
	private double _averageMeetCount;

	public LunchedAndDepartmentPartyScorer(UserMatrixHandler userMatrixHandler)
	{
		_userMatrixHandler = userMatrixHandler;
	}

	public async Task Initialise(IReadOnlyList<MyUser> users)
	{
		if (_isInitialised)
		{
			return;
		}

		_userMatrix = await _userMatrixHandler.GetCumulative();
		_averageMeetCount = CalculateAverageMeetCount(users);
		_isInitialised = true;
	}

	public double ScoreGroup(Group group)
	{
		if (_userMatrix is null)
		{
			throw new Exception($"{nameof(LunchedAndDepartmentPartyScorer)} not initialised");
		}

		int metBeforeCount = GetMetBeforeCount(group);
		double meetDeviation = metBeforeCount - _averageMeetCount;
		int sign = Math.Sign(meetDeviation);
		double meetFactor = meetDeviation * meetDeviation * sign;

		int departmentPenalty = group
			.GroupBy(u => u.Department)
			.Sum(g => g.Count() - 1) * 2;

		return meetFactor + departmentPenalty;
	}

	public bool IsScoreBetter(double newScore, double? oldScore)
	{
		return newScore < (oldScore ?? double.MaxValue);
	}

	public bool IsAcceptable(Party party)
	{
		return true;
	}

	private int GetMetBeforeCount(Group group)
	{
		int metBeforeCount = 0;

		for (int i = 0; i < group.Users.Count; i++)
		{
			for (int j = i + 1; j < group.Users.Count; j++)
			{
				string id0 = group.Users[i].Id;
				string id1 = group.Users[j].Id;

				if (_userMatrix!.TryGetMeetCount(id0, id1, out int meetCount))
				{
					metBeforeCount += meetCount;
				}
			}
		}

		return metBeforeCount;
	}

	private double CalculateAverageMeetCount(IReadOnlyList<MyUser> users)
	{
		int totalMeetCount = 0;

		for (int i = 0; i < users.Count; i++)
		{
			for (int j = i + 1; j < users.Count; j++)
			{
				string iId = users[i].Id;
				string jId = users[j].Id;

				if (_userMatrix!.TryGetMeetCount(iId, jId, out int meetCount))
				{
					totalMeetCount += meetCount;
				}
			}
		}

		int relationshipCount = (users.Count - 1) * users.Count / 2;
		double averageMeetCount = (double)totalMeetCount / relationshipCount;
		return averageMeetCount;
	}
}