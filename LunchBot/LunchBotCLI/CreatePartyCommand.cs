using System.Globalization;
using LunchBot;
using McMaster.Extensions.CommandLineUtils;
using Serilog;
using Prompt = McMaster.Extensions.CommandLineUtils.Prompt;

namespace LunchBotCLI;

[Command(Name = "create",
    Description = "Run the party generator to create the groups for a party",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
internal class CreatePartyCommand : CommandBase
{
    [Option(ShortName = "p", Description = "The absolute path of the .xlsx file to read all people from")]
    private string WorkSheetPath { get; set; }

    [Option(ShortName = "n", Description = "Optional name override for the party instead of the current month")]
    private string PartyName { get; set; }

    private readonly ILogger _logger;
    private readonly UserFinder _userFinder;
    private readonly PeopleFileReader _peopleReader;
    private readonly UserMatrixHandler _userMatrixHandler;
    private readonly UserIndexerHandler _userIndexerHandler;
    private readonly PartyGenerator _partyGenerator;
    private readonly IGroupSizer _groupSizer;
    private readonly PartyDataFiler _partyDataFiler;
    private readonly PartyDataDisplayer _partyDataDisplayer;

    public CreatePartyCommand(ILogger logger,
        UserFinder userFinder,
        PeopleFileReader peopleReader,
        UserMatrixHandler userMatrixHandler,
        UserIndexerHandler userIndexerHandler,
        PartyGenerator partyGenerator,
        IGroupSizer groupSizer,
        PartyDataFiler partyDataFiler,
        PartyDataDisplayer partyDataDisplayer)
    {
        _logger = logger;
        _userFinder = userFinder;
        _peopleReader = peopleReader;
        _userMatrixHandler = userMatrixHandler;
        _userIndexerHandler = userIndexerHandler;
        _partyGenerator = partyGenerator;
        _groupSizer = groupSizer;
        _partyDataFiler = partyDataFiler;
        _partyDataDisplayer = partyDataDisplayer;
    }

    protected override async Task<int> OnExecute(CommandLineApplication app)
    {
        if (string.IsNullOrEmpty(WorkSheetPath))
        {
            WorkSheetPath = Prompt.GetString("Enter People WorkSheet Path:");
        }
        else
        {
            bool useLast = Prompt.GetYesNo("Use last worksheet path?", false);

            if (!useLast)
            {
                WorkSheetPath = Prompt.GetString("Enter People WorkSheet Path:");
            }
        }

        if (!_peopleReader.TryLoadPeople(WorkSheetPath, out IReadOnlyList<HrPerson> people))
        {
            _logger.Error("Exiting");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        (IReadOnlyList<MyUser> allUsers, IReadOnlyList<HrPerson> unfoundPeople) = await _userFinder.GetAllUsers(people);

        // IReadOnlyList<HrPerson> unfoundPeople = new List<HrPerson>();
        // IReadOnlyList<MyUser> allUsers = JsonConvert.DeserializeObject<MyUser[]>(
        //         "[{\"Name\":\"Alexandros\",\"Surname\":\"Ntentes\",\"Id\":\"8ca39277-d527-4d40-9d81-e194a84aebed\",\"Department\":\"Venue Operations\"},{\"Name\":\"Rob\",\"Surname\":\"Malvern\",\"Id\":\"4e3fef6c-f528-4b12-81e8-5010338c652a\",\"Department\":\"Technology\"},{\"Name\":\"Amy\",\"Surname\":\"Livesey\",\"Id\":\"27994282-e4ff-4e65-830c-d354499dbd7a\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Sophie\",\"Surname\":\"Beresford\",\"Id\":\"e0fa0e35-e2db-4c1e-b26a-507cd16ff4ea\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Nikki\",\"Surname\":\"Lowry\",\"Id\":\"3a3c91d4-d259-4650-927a-d4172b998962\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Jo\",\"Surname\":\"Johnstone\",\"Id\":\"99523c5d-5421-4850-8102-505638e928f0\",\"Department\":\"Reservations\"},{\"Name\":\"Emma\",\"Surname\":\"Harding\",\"Id\":\"47dfc648-1810-452c-91dd-e7f30dc53eb6\",\"Department\":\"Reservations\"},{\"Name\":\"Joseph\",\"Surname\":\"Page\",\"Id\":\"34023f61-265d-4d37-8897-bc1e3de8df93\",\"Department\":\"Reservations\"},{\"Name\":\"Simon\",\"Surname\":\"Denney\",\"Id\":\"fe6d712d-bd9b-4c0c-aa4c-5a88e84c1692\",\"Department\":\"Technology\"},{\"Name\":\"Tilly\",\"Surname\":\"Watters\",\"Id\":\"339e2252-396c-47d7-8665-0c2959cc9f89\",\"Department\":\"Reservations\"},{\"Name\":\"Daniel\",\"Surname\":\"Shapiro\",\"Id\":\"394563fd-e7e3-45a6-9403-6a1c076ef897\",\"Department\":\"Technology\"},{\"Name\":\"Anna\",\"Surname\":\"Kelk\",\"Id\":\"05ac4ca9-6600-4f54-beb6-3de4556e7f70\",\"Department\":\"People\"},{\"Name\":\"Lois\",\"Surname\":\"Skelding\",\"Id\":\"c7298b98-b1ce-4f82-9436-973d83868f0d\",\"Department\":\"Interior Design\"},{\"Name\":\"Megan\",\"Surname\":\"Darrall\",\"Id\":\"f5c82725-2d39-4547-95bf-9e707eef550d\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Deirdre\",\"Surname\":\"Hayes\",\"Id\":\"000becb7-dbb7-4d40-b282-ad9d0ae4cf94\",\"Department\":\"Interior Design\"},{\"Name\":\"Nena\",\"Surname\":\"Gauci\",\"Id\":\"eeb0f450-4032-4143-a29a-d80c71e7d7c4\",\"Department\":\"Property\"},{\"Name\":\"Vijay\",\"Surname\":\"Dale\",\"Id\":\"f1ac89f4-9ef3-4abf-a730-32870e6b6670\",\"Department\":\"Property\"},{\"Name\":\"Zoe\",\"Surname\":\"Wood\",\"Id\":\"b5026ee2-f376-444f-827e-1e65c0468677\",\"Department\":\"Interior Design\"},{\"Name\":\"Esther\",\"Surname\":\"Gorner\",\"Id\":\"7385474a-65f5-4e4e-9b08-99dbcc562cab\",\"Department\":\"Interior Design\"},{\"Name\":\"Alix\",\"Surname\":\"Cumersdale\",\"Id\":\"3e97be04-c8e1-41bc-9c1f-e72e9a1e7164\",\"Department\":\"Interior Design\"},{\"Name\":\"Alfie\",\"Surname\":\"Alliston\",\"Id\":\"c72bc7ba-dbd7-440e-a7de-a068e069719a\",\"Department\":\"Technology\"},{\"Name\":\"Alexandra\",\"Surname\":\"Nicolescu\",\"Id\":\"ae5eb5f5-9ce4-441e-9cb6-e5af7bd68444\",\"Department\":\"Reservations\"},{\"Name\":\"Vikki\",\"Surname\":\"Iggulden\",\"Id\":\"c9ebaa20-c64e-472b-90eb-3f807f18c1eb\",\"Department\":\"People\"},{\"Name\":\"Fiona\",\"Surname\":\"Radloff\",\"Id\":\"175fe681-204a-48f3-9b85-be2387dede3a\",\"Department\":\"Finance\"},{\"Name\":\"Frank\",\"Surname\":\"Burden\",\"Id\":\"08fb59b3-d5c8-47f3-805e-6c9375a18dbc\",\"Department\":\"Venue Operations\"},{\"Name\":\"Fran\",\"Surname\":\"Carpenter\",\"Id\":\"46a0fea5-0f67-499d-a275-666d2cd7d3a1\",\"Department\":\"People\"},{\"Name\":\"Rebecca\",\"Surname\":\"Love\",\"Id\":\"8fd6bba9-c57f-4dd1-be8f-1f4820460615\",\"Department\":\"People\"},{\"Name\":\"Lucy\",\"Surname\":\"Thompson\",\"Id\":\"bc94a9ea-7e15-42ac-8dc6-5351db41232c\",\"Department\":\"Finance\"},{\"Name\":\"Mike\",\"Surname\":\"Palu\",\"Id\":\"6bb3c7df-1c2b-4317-a737-12957f3e4187\",\"Department\":\"Technology\"},{\"Name\":\"Francesco\",\"Surname\":\"Perrotta\",\"Id\":\"379d4c95-f8f4-487d-a555-db2dde052a04\",\"Department\":\"Technology\"},{\"Name\":\"Andy\",\"Surname\":\"Faulkner\",\"Id\":\"983c8a6c-6657-487e-8597-3869a7b95520\",\"Department\":\"Technology\"},{\"Name\":\"Raimonda\",\"Surname\":\"Yadav\",\"Id\":\"f8de13b6-c0ea-462a-96ac-e76a1ecb2054\",\"Department\":\"Reservations\"},{\"Name\":\"Becky\",\"Surname\":\"Troock\",\"Id\":\"b3ea716d-a308-4309-bfc4-f066d782a905\",\"Department\":\"Reservations\"},{\"Name\":\"Angela\",\"Surname\":\"Turner\",\"Id\":\"2f69bfe4-cc0f-4206-9c4b-4ebabedd3c39\",\"Department\":\"Reservations\"},{\"Name\":\"Debbie\",\"Surname\":\"Mbemba\",\"Id\":\"65365897-e159-4b1a-9230-b13c2b1428d2\",\"Department\":\"Reservations\"},{\"Name\":\"Oliver\",\"Surname\":\"Durrant\",\"Id\":\"8e81239e-dbf4-47ea-a498-48b4d8e665e9\",\"Department\":\"Reservations\"},{\"Name\":\"Alexandra\",\"Surname\":\"Ciliboiu\",\"Id\":\"fb3b1953-8c72-44d2-aaa1-a88072085225\",\"Department\":\"People\"},{\"Name\":\"Amelia\",\"Surname\":\"Wotton\",\"Id\":\"890ad15b-772a-42ab-98c8-d4d002648b5e\",\"Department\":\"Creative\"},{\"Name\":\"Ross\",\"Surname\":\"Shepley-Smith\",\"Id\":\"fc177514-78d0-41a9-861a-be5a4c93bcc4\",\"Department\":\"Finance\"},{\"Name\":\"Bradley\",\"Surname\":\"Phillips\",\"Id\":\"b4e3b141-ca94-4675-b378-3105aceb6c35\",\"Department\":\"Finance\"},{\"Name\":\"Jimmy\",\"Surname\":\"Dare\",\"Id\":\"87394424-5a85-496e-8846-a07f89302e39\",\"Department\":\"Interior Design\"},{\"Name\":\"Simran\",\"Surname\":\"Nagra\",\"Id\":\"1976d844-f0d0-42de-a773-061a6d70524e\",\"Department\":\"Finance\"},{\"Name\":\"Sandy\",\"Surname\":\"Kaczmarek\",\"Id\":\"2668d509-bd18-435e-a888-2ab976cbd377\",\"Department\":\"Reservations\"},{\"Name\":\"Deborah\",\"Surname\":\"Adeladun\",\"Id\":\"a5e4fcb2-39f6-479f-98da-f3489b721713\",\"Department\":\"Finance\"},{\"Name\":\"Dalton\",\"Surname\":\"Sowah\",\"Id\":\"ae86d3b1-7ea6-40cb-a34f-12fec8937f0c\",\"Department\":\"Creative\"},{\"Name\":\"Amie\",\"Surname\":\"Cracknell\",\"Id\":\"1e8543cd-3880-4f9e-9bb4-524d1a50abe1\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Josephine\",\"Surname\":\"Wisth\",\"Id\":\"a97e7695-6657-4d12-a3c0-140f072230dc\",\"Department\":\"Reservations\"},{\"Name\":\"Anna\",\"Surname\":\"Szymkowska\",\"Id\":\"a5efa729-408a-43d3-a00f-119f7819d310\",\"Department\":\"Reservations\"},{\"Name\":\"Camilo\",\"Surname\":\"Cardona\",\"Id\":\"d402fa99-49a8-4ee7-afea-157609fd11c1\",\"Department\":\"Reservations\"},{\"Name\":\"Ben\",\"Surname\":\"Dzeve\",\"Id\":\"44720ede-fc50-46dc-b578-bc2f4a8f6f6d\",\"Department\":\"Property\"},{\"Name\":\"Samantha\",\"Surname\":\"Lyras\",\"Id\":\"250a260e-aa97-4e6a-afc9-9fdc0a4c9f47\",\"Department\":\"Interior Design\"},{\"Name\":\"Chuen\",\"Surname\":\"Chow\",\"Id\":\"a8541c7c-cd04-4e89-92a0-31c2ae9fd3ba\",\"Department\":\"Interior Design\"},{\"Name\":\"Steve\",\"Surname\":\"Moore\",\"Id\":\"fd73a270-0e2b-4d26-891b-e6ef629ca4dd\",\"Department\":\"Global Development\"},{\"Name\":\"David\",\"Surname\":\"Ramsbottom\",\"Id\":\"fd5b3ea3-fb02-44e8-b4b5-1463a5e676bf\",\"Department\":\"Creative\"},{\"Name\":\"Sengagh\",\"Surname\":\"Cowan\",\"Id\":\"e470dae2-002a-402f-9115-06615fcd0002\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Phil\",\"Surname\":\"Whyte\",\"Id\":\"8e54be00-8592-4709-a22a-857ea65dd6d8\",\"Department\":\"Property\"},{\"Name\":\"Gavin\",\"Surname\":\"Ferris\",\"Id\":\"ea591cbb-efd9-4e93-9b1b-9eaf516d6a8e\",\"Department\":\"Property\"},{\"Name\":\"Rebecca\",\"Surname\":\"Walker\",\"Id\":\"ba907842-b034-4821-8548-ebecaa9c169d\",\"Department\":\"Interior Design\"},{\"Name\":\"Amy\",\"Surname\":\"Gerrard\",\"Id\":\"7a24902a-4432-4ba5-a0d0-660c0e4bf46a\",\"Department\":\"People\"},{\"Name\":\"Marta\",\"Surname\":\"Mantoan\",\"Id\":\"b4c57b80-1e8a-48ec-af76-cf36a3578258\",\"Department\":\"Interior Design\"},{\"Name\":\"Paul\",\"Surname\":\"O'Brien\",\"Id\":\"5da7f379-7a70-45d2-9c5f-4fe45194d188\",\"Department\":\"Creative\"},{\"Name\":\"Chelsea\",\"Surname\":\"Koch-Bailey\",\"Id\":\"b4c95a1b-3e3d-4579-b6f4-c631626ff7a5\",\"Department\":\"Reservations\"},{\"Name\":\"Carmi\",\"Surname\":\"Caetano\",\"Id\":\"d25bed8a-1358-448a-b266-beacdda8e8f4\",\"Department\":\"People\"},{\"Name\":\"Paul\",\"Surname\":\"Barham\",\"Id\":\"7a13c070-5942-4df5-bdfa-570a0085ca8b\",\"Department\":\"Property\"},{\"Name\":\"David\",\"Surname\":\"Piazzani\",\"Id\":\"c695c9fa-f277-4cc9-b062-f0b5557eb678\",\"Department\":\"Venue Operations\"},{\"Name\":\"Alison\",\"Surname\":\"Pike\",\"Id\":\"08190975-7aab-4d09-b2cc-e26d2b9e2672\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Louis\",\"Surname\":\"Atkins\",\"Id\":\"8da0a6f7-0cdc-4e96-a750-580ddb7d1207\",\"Department\":\"Venue Operations\"},{\"Name\":\"Liam\",\"Surname\":\"Watkins\",\"Id\":\"c3583f6c-c2bf-4725-84ac-838129dd065c\",\"Department\":\"Technology\"},{\"Name\":\"James\",\"Surname\":\"Reilly\",\"Id\":\"f6533dd4-afb9-410c-91f4-656f38d26e95\",\"Department\":\"Venue Operations\"},{\"Name\":\"Mason\",\"Surname\":\"Hall\",\"Id\":\"90e53fb0-a3eb-4cc5-bb97-43b739ba6596\",\"Department\":\"Technology\"},{\"Name\":\"Ludovica\",\"Surname\":\"Pirazzi\",\"Id\":\"b5bae8c5-b34f-42d2-94ff-3f63a9130020\",\"Department\":\"Venue Operations\"},{\"Name\":\"Dustin\",\"Surname\":\"Acton\",\"Id\":\"e6a842b4-130f-43d8-90e3-a587bc65df19\",\"Department\":\"Venue Operations\"},{\"Name\":\"Jason\",\"Surname\":\"Dale\",\"Id\":\"b2192280-6e3a-41ea-a97a-6586c1c1853f\",\"Department\":\"Technology\"},{\"Name\":\"Chloe\",\"Surname\":\"Wooldridge\",\"Id\":\"0c812ad1-aed1-497e-8023-0c6191b21160\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Laura\",\"Surname\":\"Thuau\",\"Id\":\"70ce1a65-fa70-4f90-af2c-d34c3f5afe5d\",\"Department\":\"Reservations\"},{\"Name\":\"Nicolas\",\"Surname\":\"Flores\",\"Id\":\"ae87b11d-90ba-42da-b50f-352e21a25359\",\"Department\":\"Reservations\"},{\"Name\":\"Samantha\",\"Surname\":\"Acton\",\"Id\":\"708ec282-b9cb-43d3-95b1-63bcbaa1d51b\",\"Department\":\"Reservations\"},{\"Name\":\"Kim\",\"Surname\":\"Buddle\",\"Id\":\"04a3ccbe-be84-48d6-9e67-503b670bbb3a\",\"Department\":\"Interior Design\"},{\"Name\":\"Alec\",\"Surname\":\"North\",\"Id\":\"f3f83d4e-2432-4677-ba31-fb405623c253\",\"Department\":\"Creative\"},{\"Name\":\"Olivia\",\"Surname\":\"Newall\",\"Id\":\"d948fa82-2fc1-4fe6-8b7b-0be3834866d0\",\"Department\":\"Creative\"},{\"Name\":\"Juliette\",\"Surname\":\"Keyte\",\"Id\":\"de75b7b6-d75c-4bc2-81c1-a5c10392739a\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Louise\",\"Surname\":\"Felton\",\"Id\":\"f2519a3a-b5fc-4760-b517-b57dd9ed4672\",\"Department\":\"Creative\"},{\"Name\":\"Sebastian\",\"Surname\":\"Cyde\",\"Id\":\"a576849b-9a1f-41b5-9992-69b2be4716fa\",\"Department\":\"Creative\"},{\"Name\":\"Jade\",\"Surname\":\"Curtin\",\"Id\":\"4348f9cf-38a4-45a1-820d-19eb07562f5b\",\"Department\":\"Venue Operations\"},{\"Name\":\"Molly\",\"Surname\":\"Capel\",\"Id\":\"468c276f-f960-4d38-87d9-8329b5b1d37a\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Frederique\",\"Surname\":\"Rapier\",\"Id\":\"e186b8d0-18f0-41a6-897b-73ae9f391621\",\"Department\":\"Reservations\"},{\"Name\":\"Chiara\",\"Surname\":\"Nieddu\",\"Id\":\"4c4b4f1e-4a68-49f6-b69c-9c4ae7c77b1d\",\"Department\":\"Reservations\"},{\"Name\":\"Shannen\",\"Surname\":\"Dooling\",\"Id\":\"a1b59996-5252-4fa5-8550-1681ebb3e82e\",\"Department\":\"Reservations\"},{\"Name\":\"Carys\",\"Surname\":\"Green\",\"Id\":\"21ec8595-a3f0-4ed7-9699-15df9ac89a0a\",\"Department\":\"Interior Design\"},{\"Name\":\"Matthew\",\"Surname\":\"Snow\",\"Id\":\"bd74184d-e3ff-4b0e-9b52-7597073a890c\",\"Department\":\"Creative\"},{\"Name\":\"Callum\",\"Surname\":\"Rose\",\"Id\":\"6dc80e91-698d-48ee-a792-8fd0c0a17e68\",\"Department\":\"Creative\"},{\"Name\":\"Ben\",\"Surname\":\"Morris\",\"Id\":\"4a0c13fa-f538-4253-b26a-ae7b195de109\",\"Department\":\"Creative\"},{\"Name\":\"Ciana\",\"Surname\":\"Ayre\",\"Id\":\"27352e98-6e28-4885-ba5d-c1398df9fb3f\",\"Department\":\"Creative\"},{\"Name\":\"Tom\",\"Surname\":\"Morgan\",\"Id\":\"d88b8645-5151-4a23-aef3-ed258e9ec084\",\"Department\":\"Venue Operations\"},{\"Name\":\"Laura\",\"Surname\":\"Thornes\",\"Id\":\"9128a9bb-4c24-44f5-85d6-14eadb6847c5\",\"Department\":\"Sales & Marketing\"},{\"Name\":\"Kate\",\"Surname\":\"Coppinger\",\"Id\":\"64edf606-ee84-47d8-91cc-9767133f82dd\",\"Department\":\"Reservations\"},{\"Name\":\"Hannah\",\"Surname\":\"Melia\",\"Id\":\"f20bec7c-20c8-423e-8e04-025a06eab698\",\"Department\":\"People\"},{\"Name\":\"Laura\",\"Surname\":\"Theobald\",\"Id\":\"4dc8695b-caf0-4d5a-8039-4603cd5c09fc\",\"Department\":\"People\"},{\"Name\":\"Lisa\",\"Surname\":\"Ascherl\",\"Id\":\"c3b1c9d4-ab49-47c7-be83-b07852767d4f\",\"Department\":\"People\"},{\"Name\":\"Joe\",\"Surname\":\"Roberts\",\"Id\":\"655553f9-e64d-428c-b9f0-664c6c2487a5\",\"Department\":\"Interior Design\"},{\"Name\":\"Natalie\",\"Surname\":\"Bish\",\"Id\":\"7d79ac60-4f3e-4a17-9a41-0ce5be5b9bea\",\"Department\":\"Finance\"},{\"Name\":\"Sophie\",\"Surname\":\"Pickup\",\"Id\":\"87b09e9e-a749-4b79-a488-b7043045fcca\",\"Department\":\"Creative\"},{\"Name\":\"Andy\",\"Surname\":\"McNeil\",\"Id\":\"6f816eb2-c90a-4e67-b2e4-c4757ef27b41\",\"Department\":\"Creative\"},{\"Name\":\"Dee\",\"Surname\":\"Appleby\",\"Id\":\"9c75d5c6-81f3-44c2-a0ee-bad4fcd30863\",\"Department\":\"Creative\"}]")
        //     .OrderBy(u => u.Id)
        //     .ToArray();
        // User conductor = null;

        try
        {
            Assert.UsersAreValid(people, allUsers, unfoundPeople, _logger);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Users are invalid");
            _logger.Information("Exiting");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        await _userIndexerHandler.AddUsersAndSave(allUsers);

        PartyData partyData = await _partyGenerator.Generate(allUsers);

        try
        {
            Assert.PartyIsValid(partyData.Party, allUsers, _groupSizer.ExpectedGroupSize);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Party is invalid");
            return await CommandHelper.ExecuteRootCommand(app);
        }

        if (string.IsNullOrWhiteSpace(PartyName))
        {
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Today);
            PartyName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month);
        }

        await _partyDataFiler.Save(partyData, PartyName);
        await _userMatrixHandler.SaveSingle(partyData);

        Console.WriteLine("Generated Party Data:");

        _partyDataDisplayer.DisplayData(partyData);

        return await CommandHelper.ExecuteRootCommand(app);
    }
}