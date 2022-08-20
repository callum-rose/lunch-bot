using Microsoft.Graph;
using Newtonsoft.Json;

namespace LunchBot;

public class MyUser : IEqualityComparer<MyUser>
{
    public string Name { get; }
    public string Surname { get; }
    public string Id { get; }
    public string Department { get; }

    public MyUser(User user, string department)
    {
        Name = user.GivenName;
        Surname = user.Surname;
        Id = user.Id;
        Department = department;
    }
    
    [JsonConstructor]
    public MyUser(string name, string surname, string id, string department)
    {
        Name = name;
        Surname = surname;
        Id = id;
        Department = department;
    }

    public override string ToString()
    {
        return $"{Name} {Surname}";
    }

    public bool Equals(MyUser x, MyUser y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(MyUser obj)
    {
        return obj.Id.GetHashCode();
    }
}