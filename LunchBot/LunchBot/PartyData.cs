namespace LunchBot;

public class PartyData
{
    public readonly Guid Id;
    public readonly int Seed;
    public readonly Party Party;

    public PartyData(Guid id, int seed, Party party)
    {
        Id = id;
        Seed = seed;
        Party = party;
    }
}