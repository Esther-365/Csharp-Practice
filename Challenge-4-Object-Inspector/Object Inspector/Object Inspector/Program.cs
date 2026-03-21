using System.Text;
using System.IO;

[AttributeUsage(AttributeTargets.Property)]
public class ImportantAttribute : Attribute
{
  
}

[AttributeUsage(AttributeTargets.Property)]
public class  IgnorePropertyAttribute: Attribute
{
    
}

public class  Car
{
    public string? Brand { get; set; }
    private int _Year { get; set; }
    public int Year 
    { 
        get => _Year; 
        set
        {
            if (value < 1886) // The year the first car was invented
                throw new ArgumentException("Year cannot be less than 1886.");
            _Year = value;
        }
    }

    public decimal Mileage { get; set; }
    private string? _PlateNumber { get; set; }
    [Important]
    public string? PlateNumber 
    { 
        get => _PlateNumber; 
        set
        {
            if (value.Length < 8)
                throw new ArgumentException("Plate number must be at least 8 characters.");
            _PlateNumber = value;
        }
    }

    public Car(string? Brand, int Year, decimal Mileage, string PlateNumber) 
    {
        this.Brand = Brand;
        this.Year = Year;
        this.Mileage = Mileage;
        this.PlateNumber = PlateNumber;
    }
}

public class  Bank
{
    public string? Name { get; set; }
    private decimal _Amount { get; set; }
    public decimal Amount 
    { 
        get => _Amount; 
        set
        {
            if (value < 0)
                throw new ArgumentException("Amount cannot be negative.");
            _Amount = value;
        }
    }

    [IgnoreProperty]
    public string? Beneficary { get; set; }

    private long _AccountNumber { get; set; }
    [Important]
    public long AccountNumber 
    { 
        get => _AccountNumber; 
        set
        {
            if (value < 1000000000 || value > 9999999999)
                throw new ArgumentException("Account number must be a 10-digit number.");
            _AccountNumber = value;
        }
    }

    public Bank(string? FirstName, string? LastName, decimal amount, string? beneficary, long accountNumber)
    {
        this.Name = FirstName + LastName;
        Amount = amount;
        Beneficary = beneficary;
        AccountNumber = accountNumber;
    }

}

public class Attendance 
{
    private string? _ID { get; set; }
    [Important]
    public string? ID 
    { 
        get => _ID; 
        set
        {
            if (string.IsNullOrEmpty(value) || value.Length < 4)
                throw new ArgumentException("Invalid ID");
            _ID = value;
        }
    }
    public bool IsPresent { get; set; }
    public DateTime? TimeIn { get; private set; }
    public DateTime? TimeOut { get; private set; }
    private Random rand = new Random();


    private int hoursStayed;
    //

    public Attendance(string? ID, bool IsPresent)
    {
        this.ID = ID;
        this.IsPresent = IsPresent;
        hoursStayed = rand.Next(5, 9);
        TimeIn = IsPresent? DateTime.Now: (DateTime?)null;
        TimeOut = IsPresent && TimeIn.HasValue ? TimeIn.Value.AddHours(hoursStayed) : (DateTime?)null;
    }
}

public class ObjectInspector 
{
    public static string Inspect(object obj)
    {
        StringBuilder sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine($"Inspecting object of type: {type.Name}");
        sb.AppendLine($"Assembly: {type.Assembly.GetName().Name}");

        foreach (var prop in type.GetProperties())
        {
            if (Attribute.IsDefined(prop, typeof(IgnorePropertyAttribute)))
                continue;

            var attributes = prop.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                sb.AppendLine($"Attribute: [{attr.GetType().Name}]");
            }
            var value = prop.GetValue(obj);
            if (Attribute.IsDefined(prop, typeof(ImportantAttribute)))
            {
                if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                {
                    sb.AppendLine($"{prop.Name}: [Important property is null or empty]");
                    continue;
                }
                else
                {
                    sb.AppendLine($"Important Detail\n{prop.Name}: {value}");
                }
            }
            else
            {
                sb.AppendLine($"{prop.Name}: {value}");
            }
        }
        return sb.ToString();

    }

    public static void InspectToFile(object obj, string filePath) 
    {
        string report = Inspect(obj);
        File.WriteAllText(filePath, report);
    }
}

public class Program
{
    public static void Main()
    {
        Car car = new Car("Toyota", 2020, 15000, "ABC12345");
        Bank bank = new Bank("John", "Doe", 10000, "Jane Doe", 1234567890);
        Attendance attendance = new Attendance("EMP001", true);
        ObjectInspector.InspectToFile(car, "car_report.txt");
        ObjectInspector.InspectToFile(bank, "bank_report.txt");
        ObjectInspector.InspectToFile(attendance, "attendance_report.txt");


    }
}