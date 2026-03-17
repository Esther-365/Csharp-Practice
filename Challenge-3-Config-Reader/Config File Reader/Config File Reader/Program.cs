// See https://aka.ms/new-console-template for more information
using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigKeyAttribute : Attribute 
{
    public string? KeyName { get;}
    // I used get only because the metadata is not suppposed to change after compilation.
    public ConfigKeyAttribute(string KeyName) 
    { 
        this.KeyName = KeyName;
        // This stores the string message when the Attribute is used so reflection can read it.
    }
}

class AppSettings 
{
    [ConfigKey("App name")]
    public string? AppName { get; set; }
    [ConfigKey("Max users")]
    public int MaxUsers { get; set; }
    [ConfigKey("Debug mode")]
    public bool Debug { get; set; }
}

class ConfigLoader 
{
    public static T Load<T>(string filePath) where T : new() 
    { // :new() means that T must have a parameterless constructor
        // Load<T> means T is a generic type which is determined when the method is called.
        string[] lines = File.ReadAllLines(filePath);
        var loader = new T();
        Type type = typeof(T);
        foreach (string line in lines) 
        {
            string[] parts = line.Split("=", StringSplitOptions.RemoveEmptyEntries);
            if (parts is null) 
            {
                continue;
            }
            string key = parts[0].Trim();
            string val = parts[1].Trim();

            foreach (PropertyInfo prop in type.GetProperties()) 
            {
                //Get the ConfigKeyAttribute applied to the property, if it exists and check if the KeyName matches the key from the config file
                var attr = (ConfigKeyAttribute?)Attribute.GetCustomAttribute(prop, typeof(ConfigKeyAttribute));

                if (attr != null && attr.KeyName == key)
                {
                    object value = Convert.ChangeType(val, prop.PropertyType);
                    //Set the value of the property on the loader object using reflection
                    prop.SetValue(loader, value);
                    break; // Exit the loop once the matching property is found and set
                }

                
                //Note .GetType() returns the type of the PropertyInfo object itself, which is System.Reflection.PropertyInfo

                
            }
        }
        // This instance of T represnts the generic object T that will be returned.
        return loader;


    }
}

class Solution 
{
    static void Main(string[] args)
    {
        AppSettings settings = ConfigLoader.Load<AppSettings>("C:\\C_Sharp\\Config File Reader\\Config File Reader\\appsettingz.cfg");
        //I call it like this because Load is a static method and we are passing a generic object(which is setting of AppSettings) into it
        Type type = typeof(AppSettings);
        PropertyInfo[] props = type.GetProperties();
        foreach (var prop in props) 
        {
            Console.WriteLine($"{prop.Name}: {prop.GetValue(settings)}");
        }
    }
}