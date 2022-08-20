namespace LunchBot;

public class HrPerson
{
    public string Name { get; }
    public string Surname { get; }
    public string Department { get; }

    public HrPerson(string name, string surname, string department)
    {
        Name = name.Trim();
        Surname = surname.Trim();
        Department = department.Trim();
    }

    public override string ToString()
    {
        return $"{Name} {Surname} - {Department}";
    }
}