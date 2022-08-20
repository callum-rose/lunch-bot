namespace LunchBot;

public class LunchData
{
    public readonly Guid PartyDataId;
    public readonly bool WasDryRun;
    public readonly bool Successful;
    public readonly IReadOnlyList<GroupChat> Chats;

    public LunchData(Guid partyDataId, bool wasDryRun, bool successful, IReadOnlyList<GroupChat> chats)
    {
        PartyDataId = partyDataId;
        WasDryRun = wasDryRun;
        Successful = successful;
        Chats = chats;
    }
}