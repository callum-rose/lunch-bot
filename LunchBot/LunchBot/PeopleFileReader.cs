using NanoXLSX;
using Serilog;

namespace LunchBot;

public class PeopleFileReader
{
    private readonly ILogger _logger;

    public PeopleFileReader(ILogger logger)
    {
        _logger = logger;
    }

    public bool TryLoadPeople(string path, out IReadOnlyList<HrPerson> people)
    {
        if (path is null)
        {
            _logger.Error("Path is null");
            people = null;
            return false;
        }

        path = path.Trim('"');

        Workbook workbook;

        try
        {
            workbook = Workbook.Load(path);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Couldn't read worksheet at: \"{path}\"");
            people = null;
            return false;
        }

        _logger.Information($"Reading worksheet at: {path}");

        Worksheet worksheet = workbook.CurrentWorksheet;
        int lastRowNumber = worksheet.GetLastRowNumber();

        List<HrPerson> peopleInternal = new();

        for (int i = 0; i <= lastRowNumber; i++)
        {
            Cell a = worksheet.GetCell(0, i);
            Cell b = worksheet.GetCell(1, i);

            string aStr = (string)a.Value;
            string bStr = (string)b.Value;

            if (aStr == "Employee" || bStr == "Division")
            {
                continue;
            }

            string[] names = aStr.Split(',');

            bool nameInvalid = names.Length < 2;

            if (nameInvalid)
            {
                _logger.Error($"Skipped row with data: {aStr}, {bStr}");
                continue;
            }

            string surname = names[0];
            string firstName = names[1];
            string department = bStr;

            HrPerson person = new(firstName, surname, department);
            peopleInternal.Add(person);
        }

        _logger.Information("Read people: " + string.Join(", ", peopleInternal.Select(p => p.ToString())));

        people = peopleInternal;

        return true;
    }
}