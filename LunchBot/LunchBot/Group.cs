using System.Collections;
using Newtonsoft.Json;

namespace LunchBot;

[JsonObject]
public class Group : IEnumerable<MyUser>
{
    public int Number { get; }
    public IReadOnlyList<MyUser> Users { get; }

    public Group(IReadOnlyList<MyUser> users, int number)
    {
        Number = number;
        Users = users;
    }

    public IEnumerator<MyUser> GetEnumerator()
    {
        return Users.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}