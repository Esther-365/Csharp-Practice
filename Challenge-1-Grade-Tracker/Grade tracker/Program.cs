// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

enum GradeLevel 
{
    A,
    B,
    C,
    D,
    F
}

class Student 
{
   
    public string Name { get; private set; } //This sets name to read-only after initialization from outside the class

    private double Score;
    public double score 
    {
        get { return Score; }
        private set 
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException("Score must be between 0 and 100.");
            }
            Score = value;
        }
    }

    private GradeLevel grade;

    [JsonConstructor]
    public Student(string name, double score)
    {
        Name = name;
        this.score = score;
    }

    public GradeLevel GetGrade()
    {
        if (score >= 90)
        {
            grade = GradeLevel.A;
        }
        else if (score >= 80)
        {
            grade = GradeLevel.B;
        }
        else if (score >= 70)
        {
            grade = GradeLevel.C;
        }
        else if (score >= 60)
        {
            grade = GradeLevel.D;
        }
        else
        {
            grade = GradeLevel.F;
        }
        return grade;
    }

    public string GetSummary() 
    {
        return $"{Name} | Score = {Score} | Grade {GetGrade()}";
    }

    public void UpdateScore(double newscore) 
    {
        if (newscore < 0 || newscore > 100) 
        {
            throw new ArgumentOutOfRangeException("Score must be between 0 and 100.");
        }
        while (newscore == Score) 
        {
            Console.WriteLine("New score is the same as the current score. Please enter a different score.");
            Console.Write("Enter new score: ");
            if (double.TryParse(Console.ReadLine(), out newscore)) 
            {
                if (newscore < 0 || newscore > 100) 
                {
                    Console.WriteLine("Score must be between 0 and 100. Please try again.");
                    continue;
                }
            }
            else 
            {
                Console.WriteLine("Invalid input. Please enter a numeric value.");
                continue;
            }
        }
        Score = newscore;
    }
    public void UpdateScore(double newscore, string reason) 
    {
        UpdateScore(newscore); // Call the existing UpdateScore method to handle validation and updating
        Console.WriteLine($"Score updated for {Name}. Reason: {reason}");
    }

}

class Classroom
{
    static void Main(string[] args) 
    {
        List<Student> students = new List<Student>();
        //string[] oruko = new string[students.Count];

        string? name = "";
        double score;
        
        do
        {
            Console.WriteLine("Enter student name: ");
            name = Console.ReadLine();

            if (string.IsNullOrEmpty(name)) 
            {
                Console.WriteLine("Student name cannot be empty. Please try again.");
            }
        }
        while (string.IsNullOrEmpty(name));

        while (true)
        {
            Console.WriteLine("Enter student score: ");
            if (double.TryParse(Console.ReadLine(), out score))
            {
                try
                {
                    Student student = new Student(name, score);
                    students.Add(student);
                    Console.WriteLine(student.GetSummary());
                    break;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a numeric value for the score.");
                continue;
            }
        }

        
        string folder = Directory.GetCurrentDirectory();
        Console.WriteLine($"Saving student data to: {folder}");
        string filePath = Path.Combine(folder, "students.json");
        

        if (File.Exists(filePath)) 
        {
            string existingJson = File.ReadAllText(filePath);
            if (!string.IsNullOrEmpty(existingJson)) 
            {
                List<Student>? existingStudents = JsonSerializer.Deserialize<List<Student>>(existingJson);
                if (existingStudents != null) 
                {
                    students.AddRange(existingStudents);
                }
            }
        }
        string json = JsonSerializer.Serialize(students);
        File.WriteAllText(filePath, json);



        string jsonData = File.ReadAllText(filePath);
        List<Student>? Stud = JsonSerializer.Deserialize<List<Student>>(jsonData);
        Stud = Stud?.OrderByDescending(s => s.score).ToList();
        Console.WriteLine("Students sorted by score (highest to lowest):");
        if (Stud != null)
        {
            foreach (var pupil in Stud)
            {
                Console.WriteLine(pupil.GetSummary());
            }
        }
    }

}