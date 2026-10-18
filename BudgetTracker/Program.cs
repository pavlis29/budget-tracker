using BudgetTracker.Models;
using BudgetTracker.Services;
using System.Globalization;
using System.Text.Json;
StorageService storage = new StorageService("data");
Console.WriteLine("Welcome to Budget Tracker!");
while (true)
{
    Console.WriteLine("1. Add transaction");
    Console.WriteLine("2. Remove transaction");
    Console.WriteLine("3. Show report");
    Console.WriteLine("0. Exit");
    Console.WriteLine("Choose an option: ");
    string? choice = Console.ReadLine();
    if (choice is null) break;
    if (choice == "1")
    {
        TransactionType type;
        while(true)
        {
            System.Console.WriteLine("Enter type (Income/Expense):");
            string? typeInput = Console.ReadLine();
            if (typeInput is null) return;
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
        if (description is null) return;
        decimal amount;
        while(true)
        {
            System.Console.WriteLine("Enter amount: ");
            string? amountInput = Console.ReadLine();
            if (amountInput is null) return;
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
        try
        {
            storage.SaveTransaction(transaction);
            Console.WriteLine($"Transaction saved to {storage.GetFilePath(transaction)}");
            Console.WriteLine($"Type: {transaction.Type} - Description: {transaction.Description} - Amount: {transaction.Amount} - Id: {transaction.Id} - Timestamp: {transaction.Timestamp}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            Console.WriteLine($"Transaction was not saved: {ex.Message}");
        }
    }
    else if (choice == "2")
    {
        System.Console.WriteLine("Remove transaction selected");
    }
    else if (choice == "3")
    {
        Console.WriteLine("Enter report date (yyyy-MM-dd), or press Enter for today:");
        string? input = Console.ReadLine();
        if (input is null) return;
        DateOnly date = DateOnly.FromDateTime(DateTime.Today);
        if (input.Length > 0 && !DateOnly.TryParseExact(input, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            Console.WriteLine("Invalid date. Use yyyy-MM-dd.");
            continue;
        }
        try
        {
            var transactions = storage.LoadTransactions(date);
            foreach (var transaction in transactions)
                Console.WriteLine($"{transaction.Type} - {transaction.Description} - {transaction.Amount} - {transaction.Id}");
            decimal income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            Console.WriteLine($"Transactions: {transactions.Count} | Income: {income} | Expense: {expense} | Balance: {income - expense}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or OverflowException)
        {
            Console.WriteLine($"Report could not be loaded: {ex.Message}");
        }
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
