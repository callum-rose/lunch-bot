namespace LunchBot;

public static class Blocker
{
    public static bool RequestUserCodeVerification(string message = null)
    {
        Random random = new();
        string code = string.Join("", Enumerable.Range(0, 4).Select(_ => random.Next(0, 10)));

        if (!string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine(message);
        }
        
        Console.WriteLine($"Enter code {code}:");
        string enteredCode = Console.ReadLine();

        return code == enteredCode;
    }
}