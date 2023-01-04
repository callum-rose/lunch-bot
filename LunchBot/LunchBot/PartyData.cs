namespace LunchBot;

public class PartyData
{
    public static readonly PartyData Invalid = new(Guid.Empty, 0, new Party(Array.Empty<Group>()));
    
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