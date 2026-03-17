// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
public enum ProductCategory
{
    Electronics,
    Clothing,
    Food,
    Books,
    Furniture
}

interface IProduct 
{
    string Name { get; }
    decimal Price { get; }
    string GetDetails();
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Product Type")] // indicates that the type should be serialized polymorphically and specifies the property name that will be used to store the type information in the JSON.
[JsonDerivedType(typeof(PhysicalProduct), "Physical")]
[JsonDerivedType(typeof(DigitalProduct), "Digital")]
public abstract class Product : IProduct 
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public abstract string GetDetails();

    public ProductCategory Category { get;}
    private int _stockcount;

    public int Stockcount 
    {
        get { return _stockcount; }
        set 
        {
            if (value < 0)
                throw new ArgumentException("Stock count cannot be negative.");
            _stockcount = value;
        }
    }

    public Product(string Name, decimal Price, ProductCategory Category, int stock) 
    {
        this.Name = Name;
        this.Price = Price;
        this.Category = Category;
        this.Stockcount = stock;
    }


}

class PhysicalProduct : Product 
{
    public double weightKg { get; private set; }

    [JsonConstructor]
    public PhysicalProduct(string Name, decimal Price, ProductCategory Category,int StockCount, double weightKg) : base(Name, Price, Category, StockCount)//base constructor to initialize the properties of the base class
    {
        this.weightKg = weightKg;
    }
    public override string GetDetails()
    {
        return $"Physical Product \nProduct Name: {Name} \nProduct Price: {Price} \nProduct Category: {Category} \nProduct Weight(Kg): {weightKg} \nStock Count: {Stockcount}";
    }

   
}

class DigitalProduct : Product 
{
    public string? LicenseType { get; private set; }

    [JsonConstructor]
    public DigitalProduct(string Name, decimal Price, ProductCategory Category,int Stockcount, string? licenseType) : base(Name, Price, Category,Stockcount)
    {
        LicenseType = licenseType;
    }
    public override string GetDetails()
    {
        return $"Digital Product \nProduct Name: {Name} \nProduct Price: {Price} \nProduct Category: {Category} \nLicense Type: {LicenseType} \nStock Count: {Stockcount}";
    }
}

public class Inventory 
{
    List<Product> products = new List<Product>();
    // To add to the list you add PhysicalProduct or DigitalProduct objects to the list not Product objects because Product is an abstract class and cannot be instantiated.

    public void AddProduct(Product product) 
    {
        products.Add(product);
    }

    public void DisplayProduct() 
    {
        foreach (var p in products) 
        {
            Console.WriteLine(p.GetDetails());
        }

        // This method will display the details of all products in the list.
        // 
    }

    public void SaveToFile(string filePath) 
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() } //This ensures that the enum values are serialized as they are written and not as their numeric representations.
        };


        List<Product> productsToSerialize = new List<Product>();

        string folder = Directory.GetCurrentDirectory();
        // Directory.GetParent returns the parent directory of the specified file path. If the file path is invalid and does not contain a parent directory, it throws an exception.
        string fileName = Path.Combine(folder,filePath);

        if (File.Exists(fileName))
        {
            var existingJson = File.ReadAllText(fileName);
            var existingProducts = JsonSerializer.Deserialize<List<Product>>(existingJson);
            if (existingProducts != null) 
            {
                productsToSerialize.AddRange(existingProducts);
            }
            // You have deserialized the contents of the file into the list.
        }

        
        productsToSerialize.AddRange(products);//This is the file does not exist.

        //Now we serialize the list back into the file.
        string jsonString = JsonSerializer.Serialize(productsToSerialize, options);
        File.WriteAllText(fileName, jsonString, Encoding.UTF8);


    }
    public void LoadFromFile(string filepath) 
    {
        if (File.Exists(filepath)) 
        {
            var jsonString = File.ReadAllText(filepath, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var loadedProducts = JsonSerializer.Deserialize<List<Product>>(jsonString, options);
            if (loadedProducts != null) 
            {
                products.Clear(); // Clear the existing products list before loading new products from the file.
                products.AddRange(loadedProducts);
            }
        }

        else
        {
            throw new FileNotFoundException("The specified file was not found.");
        }

    }

    public List<Product> Requested(decimal threshold)
    {
        var requestedProducts = products.Where(p => p.Price < threshold).ToList();
        return requestedProducts;
    }
}

class Program 
{
    static void Main(string[] args) 
    {
        
        Inventory inventory = new Inventory();
        while (true)
        {
            Console.WriteLine("Welcome to the Product Inventory System!");
            Console.WriteLine("Enter 1 for physical product and 2 for digital product: ");
            if (int.TryParse(Console.ReadLine(), out int product_type))
            {
                if (product_type == 1)
                {
                    Console.WriteLine("Enter product name: ");
                    string? name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Product name cannot be empty.");
                        continue;
                    }
                    Console.WriteLine("Enter product price: ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                    {
                        Console.WriteLine("Invalid price input.");
                        continue;
                    }
                    Console.WriteLine("Enter product category (0 for Electronics, 1 for Clothing, 2 for Food, 3 for Books, 4 for Furniture): ");
                    if (!int.TryParse(Console.ReadLine(), out int categoryInput) || categoryInput < 0 || categoryInput > 4)
                    {
                        Console.WriteLine("Invalid category input.");
                        continue;
                    }
                    ProductCategory category = (ProductCategory)categoryInput;

                    Console.WriteLine("Enter product weight in Kg: ");
                    if (!double.TryParse(Console.ReadLine(), out double weightKg))
                    {
                        Console.WriteLine("Invalid weight input.");
                        continue;
                    }

                    Console.WriteLine("Enter stock count: ");
                    if (!int.TryParse(Console.ReadLine(), out int stockCount) || stockCount < 0)
                    {
                        Console.WriteLine("Invalid stock count input.");
                        continue;
                    }
                    PhysicalProduct physical = new PhysicalProduct(name, price, category,stockCount,weightKg);
                    inventory.AddProduct(physical);
                }
                else if (product_type == 2)
                {
                    Console.WriteLine("Enter product name: ");
                    string? name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Product name cannot be empty.");
                        continue;
                    }
                    Console.WriteLine("Enter product price: ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                    {
                        Console.WriteLine("Invalid price input.");
                        continue;
                    }
                    Console.WriteLine("Enter product category (0 for Electronics, 1 for Clothing, 2 for Food, 3 for Books, 4 for Furniture): ");
                    if (!int.TryParse(Console.ReadLine(), out int categoryInput) || categoryInput < 0 || categoryInput > 4)
                    {
                        Console.WriteLine("Invalid category input.");
                        continue;
                    }
                    Console.WriteLine("Enter license type: ");
                    string? licenseType = Console.ReadLine();
                    ProductCategory category = (ProductCategory)categoryInput;

                    Console.WriteLine("Enter stock count: ");
                    if (!int.TryParse(Console.ReadLine(), out int stockCount) || stockCount < 0)
                    {
                        Console.WriteLine("Invalid stock count input.");
                        continue;
                    }
                    DigitalProduct digital = new DigitalProduct(name, price, category, stockCount, licenseType);
                    inventory.AddProduct(digital);
                }
                else
                {
                    Console.WriteLine("Invalid product type input.");
                    continue;
                }

                Console.WriteLine("Do you want to add another product? (y/n): ");
                string? continueInput = Console.ReadLine();
                if (continueInput == null || continueInput.ToLower() != "y")
                {
                    Console.WriteLine("Enter 1 to save to a file or 2 to display product: ");
                    if (int.TryParse(Console.ReadLine(), out int action))
                    {
                        if (action == 1)
                        {
                            Console.WriteLine("Enter file path to save: ");
                            string? filePath = Console.ReadLine();
                            string defaultFilePath = "products.json";
                            if (string.IsNullOrEmpty(filePath))
                            {
                                filePath = defaultFilePath;
                            }

                            inventory.SaveToFile(filePath);

                            Console.WriteLine($"Products saved to {filePath}");
                            Console.WriteLine("Do you want to load products from a file? (y/n): ");
                            string? loadInput = Console.ReadLine();
                            if (loadInput != null && loadInput.ToLower() == "y")
                            {
                                Console.WriteLine("Enter file path to load: ");
                                string? loadFilePath = Console.ReadLine();
                                if (string.IsNullOrEmpty(loadFilePath))
                                {
                                    loadFilePath = defaultFilePath;
                                }
                                try
                                {
                                    inventory.LoadFromFile(loadFilePath);
                                    Console.WriteLine($"Products loaded from {loadFilePath}");
                                }
                                catch (FileNotFoundException ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }

                        }
                        else if (action == 2)
                        {
                            inventory.DisplayProduct();
                        }
                        else
                        {
                            Console.WriteLine("Invalid action input.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input.");
                    }

                }



            }
            else
            {
                Console.WriteLine("Invalid input.");
                continue;
            }
        }
    }
}
