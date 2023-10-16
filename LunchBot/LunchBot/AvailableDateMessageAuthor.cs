using Microsoft.Extensions.Configuration;

namespace LunchBot;

public class AvailableDateMessageAuthor
{
	private readonly bool _doUseDates;
	
	public AvailableDateMessageAuthor(IConfigurationRoot configuration)
	{
		_doUseDates = configuration.GetValue<bool>("SendAvailableDates");
	}
	
	public IEnumerable<string> GetMessages(DateTime from)
	{
		if (!_doUseDates)
		{
			return Enumerable.Empty<string>();
		}
		
		return GetDates(from).Select(GetMessageForDate);
	}

	private string GetMessageForDate(DateTime date)
	{
		string ordinal = date.Day switch
		{
			1 or 21 or 31 => "st",
			2 or 22 => "nd",
			3 or 23 => "rd",
			_ => "th"
		};

		return $"{date.DayOfWeek} {date.Day}<sup>{ordinal}</sup>";
	}
	
	private IEnumerable<DateTime> GetDates(DateTime from)
	{
		return Enumerable.Range(1, DateTime.DaysInMonth(from.Year, from.Month))
			.Skip(from.Day - 1)
			.Select(day => new DateTime(from.Year, from.Month, day))
			.Where(date => date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday and not DayOfWeek.Thursday)
			.ToList();
	}
}