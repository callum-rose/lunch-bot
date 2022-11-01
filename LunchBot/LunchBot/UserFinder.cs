using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Serilog;

namespace LunchBot;

public partial class UserFinder
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly AppDataFiler _appDataFiler;
    private readonly ILogger _logger;

    public UserFinder(GraphServiceClient graphServiceClient, AppDataFiler appDataFiler, ILogger logger)
    {
        _graphServiceClient = graphServiceClient;
        _appDataFiler = appDataFiler;
        _logger = logger;
    }

    public async Task<User> GetConductingUser()
    {
        return await _graphServiceClient.Me.Request().GetAsync();
    }

    public async Task<(IReadOnlyList<MyUser> users, IReadOnlyList<HrPerson> unfoundPeople)> GetAllUsers(
        IReadOnlyList<HrPerson> people)
    {
        _logger.Information("Finding users");

        // TODO Bit janky but this ensures we're logged in before running lots of tasks to get the users
        var _ = await _graphServiceClient.Me.Request().GetAsync();

        AppData appData = await _appDataFiler.Load();

        ConcurrentBag<MyUser> users = new();
        ConcurrentBag<HrPerson> unfoundPeople = new();

        IEnumerable<Task> tasks = people.Select(GetUserAndAdd);

        async Task GetUserAndAdd(HrPerson person)
        {
            (bool success, MyUser user) result = await GetUser(person, appData);

            if (result.success)
            {
                users.Add(result.user!);
            }
            else
            {
                unfoundPeople.Add(person);
            }
        }

        await Task.WhenAll(tasks);

        List<MyUser> usersList = users.OrderBy(u => u.Id).ToList();
        List<HrPerson> unfoundPeopleList = unfoundPeople.ToList();

        _logger.Information($"Found: {string.Join(", ", usersList.Select(u => u.ToString()))}");

        if (unfoundPeopleList.Any())
        {
            _logger.Error($"Could not find: {string.Join(", ", unfoundPeopleList.Select(p => p.ToString()))}");
        }

        return (usersList, unfoundPeopleList);
    }

    public async Task<(bool success, MyUser user)> GetUser(HrPerson person, AppData appData)
    {
        (string firstName, string surname) = GetNames(person, appData);

        (bool success, IList<User> currentUsers) = await GetUsersWithName(firstName, surname);

        if (!success)
        {
            return Fail();
        }

        return currentUsers.Count switch
        {
            0 => Fail(),
            1 => Success(currentUsers.First(), person),
            _ => FilterCorrectUser(person, firstName, surname, currentUsers)
        };
    }

    private (string firstName, string surname) GetNames(HrPerson person, AppData appData)
    {
        string firstName;
        string surname;

        if (appData.NameMappings.TryGetValue($"{person.Name}, {person.Surname}", out string nameOverride))
        {
            string[] split = nameOverride.Split(',');
            firstName = split[0];
            surname = split[1];
        }
        else
        {
            firstName = person.Name;
            surname = person.Surname;
        }

        return (firstName, surname);
    }

    private async Task<(bool success, IList<User> users)> GetUsersWithName(string firstName, string surname)
    {
        async Task<IGraphServiceUsersCollectionPage> GetPage(IGraphServiceUsersCollectionRequest request)
        {
            return await request
                .Select(x => new
                {
                    x.Id,
                    x.GivenName,
                    x.Surname,
                    x.Mail
                })
                .Filter($"startsWith(givenName,'{firstName}') or startsWith(surname, '{surname}')")
                .GetAsync();
        }

        List<User> users = new();

        try
        {
            IGraphServiceUsersCollectionRequest request = _graphServiceClient.Users.Request();
            IGraphServiceUsersCollectionPage page;

            do
            {
                page = await GetPage(request);
                users.AddRange(page.CurrentPage);
            }
            while ((request = page.NextPageRequest) is not null);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Can't get user: {firstName} {surname}");
            return (false, users);
        }

        return (true, users);
    }

    private (bool success, MyUser user) FilterCorrectUser(HrPerson person, string firstName, string surname,
        IList<User> currentUsers)
    {
        string Strip(string name) => name.Replace("\'", "").Replace("-", "").Replace(" ", "");

        string strippedPersonName = Strip(firstName);
        string strippedPersonSurname = Strip(surname);

        User user = currentUsers
            .Where(NameIsSimilar)
            .MaxBy(u => u, new UserMailComparer());

        bool NameIsSimilar(User u)
        {
            string strippedUserNameGiven = Strip(u.GivenName);
            string strippedUserSurname = Strip(u.Surname);

            bool isNameSimilar = (strippedUserNameGiven.Contains(strippedPersonName) ||
                                  strippedPersonName.Contains(strippedUserNameGiven))
                                 && (strippedUserSurname.Contains(strippedPersonSurname) ||
                                     strippedPersonSurname.Contains(strippedUserSurname));
            return isNameSimilar;
        }

        return user is not null ? Success(user, person) : Fail();
    }

    private (bool, MyUser) Success(User user, HrPerson person)
    {
        return (true, new MyUser(user, person.Department));
    }

    private (bool, MyUser) Fail()
    {
        return (false, null);
    }
}