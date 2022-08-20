using Microsoft.Graph;
using Serilog;

namespace LunchBot;

public static class Assert
{
    public static void UsersAreValid(IReadOnlyList<HrPerson> allPeople, IReadOnlyList<MyUser> allUsers,
        IReadOnlyList<HrPerson> unfoundPeople, ILogger logger)
    {
        if (allPeople.GroupBy(p => $"{p.Name} {p.Surname}").Count(g => g.Count() > 1) > 0)
        {
            throw new Exception("Potential duplicate person(s) read from file");
        }
        
        if (allUsers.GroupBy(u => u.Id).Count(g => g.Count() > 1) > 0)
        {
            throw new Exception("Duplicate user(s) found");
        }

        if (allPeople.Count != allUsers.Count)
        {
            throw new Exception($"Loaded {allPeople.Count} people but found {allUsers.Count} users");
        }

        if (unfoundPeople.Count > 0)
        {
            throw new Exception($"Did not find users for some people. {string.Join(", ", unfoundPeople)}");
        }
    }

    public static void PartyIsValid(Party party, IReadOnlyList<MyUser> allUsers, int expectedGroupSize)
    {
        int minGroupSize = expectedGroupSize - 1;

        if (party.Groups.Any(g => g.Users.Count > expectedGroupSize || g.Users.Count < minGroupSize))
        {
            throw new Exception("Party groups sizes wrong");
        }

        if (party.Groups.Count(g => g.Users.Count == minGroupSize) >= expectedGroupSize)
        {
            throw new Exception("Too many small groups");
        }
        
        IGrouping<string, MyUser>[] usersChosenMoreThanOnce = party
            .SelectMany(g => g.Users)
            .GroupBy(u => u.Id)
            .Where(g => g.Count() > 1)
            .ToArray();

        if (usersChosenMoreThanOnce.Length > 0)
        {
            throw new Exception("Some users are in more than one group");
        }

        IEnumerable<string> selectedUsers = party
            .SelectMany(g => g.Users)
            .Select(u => u.Id);
        string[] userIdsNotChosen = allUsers
            .Select(u => u.Id)
            .Except(selectedUsers)
            .ToArray();

        if (userIdsNotChosen.Length > 0)
        {
            throw new Exception("Some users have no been put into a group");
        }
    }

    public static void ConductorIsExpected(User conductor, string conductorName)
    {
        if (conductor is null)
        {
            throw new Exception("Conductor is null");
        }
        
        if (conductor.DisplayName != conductorName)
        {
            throw new Exception($"Expected conductor to have name of {conductorName}");
        }
    }
}