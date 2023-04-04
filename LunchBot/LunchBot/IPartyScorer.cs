namespace LunchBot;

public interface IPartyScorer
{
    Task Initialise(IReadOnlyList<MyUser> users, Party lastParty);
    
    double ScoreParty(Party party)
    {
        return party.Sum(ScoreGroup);
    }

    double ScoreGroup(Group group);
    bool IsScoreBetter(double newScore, double? oldScore);

    bool IsAcceptable(Party party);
}