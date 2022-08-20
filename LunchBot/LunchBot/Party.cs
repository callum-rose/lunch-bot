using System.Collections;
using Newtonsoft.Json;

namespace LunchBot;

[JsonObject]
public class Party : IEnumerable<Group>
{
    public readonly IReadOnlyList<Group> Groups;

    public Party(IReadOnlyList<Group> groups)
    {
        Groups = groups;
    }

    [JsonConstructor]
    private Party(Group[] groups)
    {
        Groups = groups;
    }

    public IEnumerator<Group> GetEnumerator()
    {
        return Groups.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}