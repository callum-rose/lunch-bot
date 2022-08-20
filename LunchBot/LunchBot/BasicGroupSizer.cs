using Microsoft.Extensions.Configuration;

namespace LunchBot;

public class BasicGroupSizer : IGroupSizer
{
    public int ExpectedGroupSize { get; }
    
    public BasicGroupSizer(IConfigurationRoot configuration)
    {
        ExpectedGroupSize = configuration.GetValue<int>("GroupSize");
    }

    public int[] GetGroupSizes(int total)
    {
        double fractionalGroupCount = (double)total / ExpectedGroupSize;
        int groupCount = (int)Math.Ceiling(fractionalGroupCount);
        double remainder = groupCount - fractionalGroupCount;
        
        int minGroupSize = ExpectedGroupSize - 1;

        int[] groupSizes = new int[groupCount];
        
        for (int i = 0; i < groupCount; i++)
        {
            double ratio = (double)i / minGroupSize;
            int groupSize = ratio < remainder ? minGroupSize : ExpectedGroupSize;
            groupSizes[i] = groupSize;
        }

        return groupSizes;
    }
}