namespace LunchBot;

public interface IGroupSelector
{
    int Iterations { get; }
    
    Party Choose(IReadOnlyList<MyUser> users, int? seed = null);
}