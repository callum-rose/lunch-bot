using System.Text;

namespace LunchBot;

public class PartyDataDisplayer
{
    public void DisplayData(PartyData partyData)
    {
        StringBuilder sb = new();

        sb.AppendLine($"{nameof(PartyData.Id)}: {partyData.Id}");
        sb.AppendLine($"{nameof(PartyData.Seed)}: {partyData.Seed}");
        sb.AppendLine("Party:");

        foreach (Group group in partyData.Party)
        {
            IEnumerable<string> userNames = group.Select(u => $"{u.Name} {u.Surname} ({u.Department.Substring(0, 4)})");
            string namesText = string.Join(", ", userNames);
            sb.AppendLine($"\t{group.Number}: {namesText}");
        }
        
        Console.WriteLine(sb);
    }
}