Console.WriteLine("Welcome to Budget Tracker!");
while (true)
{
    Console.WriteLine("1. Add transaction");
    Console.WriteLine("2. Remove transaction");
    Console.WriteLine("3. Show report");
    Console.WriteLine("0. Exit");
    Console.WriteLine("Choose an option: ");
    string? choice = Console.ReadLine();
    if (choice == "1")
    {
        System.Console.WriteLine("Add transaction selected");
    }
    else if (choice == "2")
    {
        System.Console.WriteLine("Remove transaction selected");
    }
    else if (choice == "3")
    {
        System.Console.WriteLine("Show report selected");
    }
    else if (choice == "0")
    {
        System.Console.WriteLine("Goodbye");
        break;
    }
    else {
        System.Console.WriteLine("Invalid option");
    }
}
