// See https://aka.ms/new-console-template for more information
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

[AttributeUsage(AttributeTargets.Property)]
public class DisplayLabelAttribute : Attribute 
{
    public string label { get; }
    public DisplayLabelAttribute(string label)
    {
        this.label = label;
    }
}

interface ITask 
{
    int Id { get; set; }
    string Title { get; set; }
    TaskStatus Status { get; set; }
    TaskPriority Priority { get; set; }
    string GetSummary();
}

public abstract class BaseTask: ITask
{
    [DisplayLabel("ID")]
    public int Id { get; set; }
    [DisplayLabel("Title")]
    public string Title { get; set; }
    [DisplayLabel("Status")]
    public TaskStatus Status { get; set; }
    [DisplayLabel("Priority")]
    public TaskPriority Priority { get; set; }

    [DisplayLabel("Due Date")]
    public DateTime DueDate { get; protected set; }
    [DisplayLabel("Created At")]
    public DateTime CreatedAt { get; private set; }
    public abstract string GetSummary();

    [JsonConstructor]
    public BaseTask(int Id, string Title, TaskStatus Status, TaskPriority Priority, DateTime DueDate)
    {
        CreatedAt = DateTime.Now; 
        this.Id = Id;
        this.Title = Title;
        this.Status = Status;
        this.Priority = Priority;
        this.DueDate = DueDate;
    }

}

public class  PersonalTask: BaseTask
{
    public PersonalTask(int Id, string Title, TaskStatus Status, TaskPriority Priority, DateTime DueDate,string Health) 
        : base(Id, Title, Status, Priority, DueDate)
    {
        this.Health = Health;
    }

    public string? Health { get; set; }

    public override string GetSummary()
    {
        string formatted = DueDate.ToString("dddd MMMM dd yyyy HH:MM"); // Example: "Monday January 01 2024 14:30"
        string created = CreatedAt.ToString("dddd MMMM dd yyyy HH:MM");

        return $"[{GetType().Name}]\nTitle: {Title}\nID: {Id}\nHealth: {Health}\nTask Status: {Status} \nTask Priority: {Priority} \nDue Date: {formatted} \nCreated At: {created}";
    }
}

public class WorkTask: BaseTask
{
    public List<string> Assignees { get; set; }

    public WorkTask(int Id, string Title, TaskStatus Status, TaskPriority Priority, DateTime DueDate, List<string> Assignees, string ProjectName) 
        : base(Id, Title, Status, Priority, DueDate)
    {
        this.Assignees = Assignees;
        this.ProjectName = ProjectName;

    }
    public string? ProjectName { get; set; }
    public override string GetSummary()
    {
        string formatted = DueDate.ToString("dddd MMMM dd yyyy HH:MM"); // Example: "Monday January 01 2024 14:30"
        string created = CreatedAt.ToString("dddd MMMM dd yyyy HH:MM");

        return $"[{GetType().Name}]\nTitle: {Title}\nID: {Id}\nProjectName: {ProjectName}\nTask Status: {Status} \nTask Priority: {Priority} \nDue Date: {formatted} \nCreated At: {created}";
    }
}

public class  TaskManager
{
    List<BaseTask> basetask = new List<BaseTask>();
    public void AddTask(BaseTask task) 
    {
        basetask.Add(task);

    }
    public BaseTask GetTaskById(int Id) 
    {
        return basetask.FirstOrDefault(t => t.Id == Id) ?? throw new Exception($"Task with ID {Id} not found.");
    }
    public void UpdateStatus(int Id, TaskStatus newStatus) 
    {
        var task = GetTaskById(Id);
        task.Status = newStatus;
        Console.WriteLine($"Task ID {Id} status updated to {newStatus}");
    }
    public void UpdateStatus(int Id,TaskStatus newStatus,string note) 
    {
        UpdateStatus(Id, newStatus);
        Console.WriteLine($"Note: {note}");
    }

    public string DisplayAll() 
    {
        //using polymorphism to call the GetSummary method of each task, which will return the appropriate summary based on the task type (PersonalTask or WorkTask).
        StringBuilder result = new StringBuilder();
        foreach (var task in basetask)
        {
            result.Append( task.GetSummary() + "\n\n");
        }
        return result.ToString();
    }

    public void SaveToJson(string filePath) 
    {
        List<BaseTask> baset = new List<BaseTask>();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        
        string current = Directory.GetCurrentDirectory();
        string path = Path.Combine(current, filePath);

        var fromfile = File.ReadAllText(path);
        var fromFile = JsonSerializer.Deserialize<List<BaseTask>>(fromfile, options);
        if (fromFile != null) 
        {
            baset.AddRange(fromFile);
        }

        baset.AddRange(basetask);
        var json = JsonSerializer.Serialize(baset,options);
        File.WriteAllText(path, json);
        Console.WriteLine($"Tasks saved to {path}");
    }

    public void LoadFromJson(string filePath) 
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
        try 
        {
            var json = File.ReadAllText(filePath);
            var tasks = JsonSerializer.Deserialize<List<BaseTask>>(json, options);
            if (tasks != null) 
            {
                basetask.Clear();
                basetask.AddRange(tasks);
            }
            Console.WriteLine($"Tasks loaded from {filePath}");
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error loading tasks from {filePath}: {ex.Message}");
        }
    }
}

public class ReflectionDisplay 
{
    public static void DisplayWithLabels(object obj) 
    {
        Type type = obj.GetType();
        foreach (PropertyInfo prop in type.GetProperties()) 
        {
            var labelAttr = prop.GetCustomAttribute<DisplayLabelAttribute>();
            string label = labelAttr?.label ?? prop.Name;
            Object? value = prop.GetValue(obj);
            if (value is List<string> list)
            {
                value = string.Join(", ", list);
            }
            Console.WriteLine($"{label}:{value?? "N/A"}");
        }
    }
}
public class Solution 
{
    static void Main(string[] args)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string projectName = assembly.GetName().Name; //This gives you the name of the current project you are working on
        TaskManager taskManager = new TaskManager();
        Console.WriteLine($"Project: {projectName}");
        string? input;
        int choice;


        do
        {
            Console.WriteLine("Enter 1 to View all task. \n2. Add a personal task. \n3. Add a work task \n4. Update Task Status \n5. View task details \n6.Exit");
            input = Console.ReadLine();
        }
        while (!int.TryParse(input, out choice));

        while (choice != 6)
        {
            if (choice == 1)
            {
                Console.WriteLine(taskManager.DisplayAll());
            }

            else if (choice == 2)
            {
                Console.WriteLine("Enter task ID");
                int id = int.Parse(Console.ReadLine() ?? "0");
                Console.WriteLine("Enter task title");
                string title = Console.ReadLine() ?? "";
                Console.WriteLine("Enter task status (Pending, InProgress, Completed, Cancelled)");
                TaskStatus status = Enum.Parse<TaskStatus>(Console.ReadLine() ?? "Pending");
                Console.WriteLine("Enter task priority (Low, Medium, High, Critical)");
                TaskPriority priority = Enum.Parse<TaskPriority>(Console.ReadLine() ?? "Low");
                Console.WriteLine("Enter Health condition: ");
                string health = Console.ReadLine() ?? "";
                Random rand = new Random();
                PersonalTask personalTask = new PersonalTask(id, title, status, priority, DateTime.Now.AddDays(rand.Next(7)), health);
                taskManager.AddTask(personalTask);
                break;
            }
            else if (choice == 3)
            {
                Console.WriteLine("Enter task ID");
                int id = int.Parse(Console.ReadLine() ?? "0");
                Console.WriteLine("Enter task title");
                string title = Console.ReadLine() ?? "";
                Console.WriteLine("Enter task status (Pending, InProgress, Completed, Cancelled)");
                TaskStatus status = Enum.Parse<TaskStatus>(Console.ReadLine() ?? "Pending");
                Console.WriteLine("Enter task priority (Low, Medium, High, Critical)");
                TaskPriority priority = Enum.Parse<TaskPriority>(Console.ReadLine() ?? "Low");
                Console.WriteLine("Enter project name: ");
                string projecs = Console.ReadLine() ?? "";
                Console.WriteLine("Enter assignees (comma separated): ");
                List<string> assignees = Console.ReadLine()?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
                Random rand = new Random();
                WorkTask workTask = new WorkTask(id, title, status, priority, DateTime.Now.AddDays(rand.Next(7)), assignees, projecs);
                taskManager.AddTask(workTask);
                break;
            }
            else if (choice == 4)
            {
                Console.WriteLine("Enter task ID to update status");
                int id = int.Parse(Console.ReadLine() ?? "0");
                var task = taskManager.GetTaskById(id);
                if (task != null)
                {
                    TaskStatus currentStatus = task.Status;
                    string status = "Pending, InProgress, Completed, Cancelled";
                    status = status.Replace(currentStatus.ToString(), "");
                    Console.WriteLine($"Enter new status ({status}) ");
                    TaskStatus newStatus = Enum.Parse<TaskStatus>(Console.ReadLine() ?? "Pending");
                    Console.WriteLine("Do you want to add a note for this status update? (y/n)");
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        Console.WriteLine("Enter note for status update (optional): ");
                        taskManager.UpdateStatus(id, newStatus, Console.ReadLine());
                    }
                    else
                    {
                        taskManager.UpdateStatus(id, newStatus);
                    }
                }
                else
                {
                    Console.WriteLine($"Task with ID {id} not found.");
                }
                break;
            }
            else
            {
                Console.WriteLine("Enter task ID to view details");
                int id = int.Parse(Console.ReadLine() ?? "0");
                var task = taskManager.GetTaskById(id);
                if (task != null)
                {
                    Console.WriteLine(task.GetSummary());
                    ReflectionDisplay.DisplayWithLabels(task);

                }
                else
                {
                    Console.WriteLine($"Task with ID {id} not found.");
                }
                break;

            }
        }
        if (choice == 6)
        {
            Console.WriteLine("Exiting Task Manager. Goodbye!");
            Environment.Exit(0); // This will terminate the application immediately with an exit code of 0, indicating a successful exit.
        }
    }
}