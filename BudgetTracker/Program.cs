using BudgetTracker.Models;

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
        TransactionType type;
        while(true)
        {
            System.Console.WriteLine("Enter type (Income/Expense):");
            string? typeInput = Console.ReadLine();
            bool typeParsed = Enum.TryParse(typeInput,
            true, out type);
            if (typeParsed && Enum.IsDefined(type))
            {
                break;
            }
            else
            {
                System.Console.WriteLine("Enter Income or Expense.");
            }
        }
        System.Console.WriteLine("Enter description: ");
        string? description = Console.ReadLine();
        decimal amount;
        while(true)
        {
            System.Console.WriteLine("Enter amount: ");
            string? amountInput = Console.ReadLine();
            bool parsed = decimal.TryParse(amountInput, out amount);
            if (parsed && amount > 0)
            {
                break;
            }
            else
            {
                System.Console.WriteLine("Enter a valid amount greater than zero.");
            }
        }
        Guid id = Guid.NewGuid();
        DateTimeOffset timestamp = DateTimeOffset.Now;
        Transaction transaction = new Transaction(
            id,
            timestamp,
            type,
            description ?? "",
            amount
        );
        System.Console.WriteLine($"Type: {transaction.Type} - Description: {transaction.Description} - Amount: {transaction.Amount} - Id: {transaction.Id} - Timestamp: {transaction.Timestamp}");
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
