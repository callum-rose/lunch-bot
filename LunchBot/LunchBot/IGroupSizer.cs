namespace LunchBot;

public interface IGroupSizer
{
    public int ExpectedGroupSize { get; }
    
    int[] GetGroupSizes(int total);
}