using BudgetTracker.Models;
using BudgetTracker.Services;
using System.Globalization;
using System.Text.Json;

string root = Path.Combine(Path.GetTempPath(), "budget-tests-" + Guid.NewGuid());
int passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (JsonException) { Check(true, name); return; }
    throw new Exception("FAIL: " + name);
}
try
{
    string directory = Path.Combine(root, "data");
    var storage = new StorageService(directory);
    var date = new DateOnly(2026, 10, 18);
    var income = new Transaction(Guid.NewGuid(), new DateTimeOffset(2026, 10, 18, 0, 30, 0,
        TimeSpan.FromHours(2)), TransactionType.Income, "Výplata 💰", 1440.50m);
    var expense = income with { Id = Guid.NewGuid(), Type = TransactionType.Expense,
        Description = "Nákup", Amount = 110.25m };
    Check(storage.LoadTransactions(date).Count == 0 && !Directory.Exists(directory),
        "Missing directory loads as an empty day without creating files");
    storage.SaveTransaction(income);
    storage.SaveTransaction(expense);
    var reloaded = new StorageService(directory).LoadTransactions(date);
    Check(reloaded.SequenceEqual(new[] { income, expense }),
        "Income and expense survive a new service instance, preserving IDs, decimals, Unicode and offset");
    Check(reloaded.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount) == 1330.25m,
        "1440.50 - 110.25 = 1330.25");
    var nextDay = income with { Id = Guid.NewGuid(), Timestamp = income.Timestamp.AddDays(1) };
    storage.SaveTransaction(nextDay);
    Check(storage.LoadTransactions(date).Count == 2 && storage.LoadTransactions(date.AddDays(1)).Count == 1,
        "Different days use separate files");
    var originalCulture = CultureInfo.CurrentCulture;
    try
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
        Check(Path.GetFileName(storage.GetFilePath(income)) == "2026-10-18.json",
            "File name uses invariant calendar and the timestamp's local date");
    }
    finally { CultureInfo.CurrentCulture = originalCulture; }
    try { storage.SaveTransaction(income); throw new Exception("Duplicate accepted"); }
    catch (InvalidOperationException) { Check(storage.LoadTransactions(date).Count == 2, "Duplicate ID rejected"); }
    try { storage.SaveTransaction(income with { Amount = 0 }); throw new Exception("Zero accepted"); }
    catch (ArgumentException) { Check(true, "Nonpositive amount rejected"); }
    string path = storage.GetFilePath(income);
    foreach (string badJson in new[] { "{broken", "null", "[null]", "[{}]", "" })
    {
        File.WriteAllText(path, badJson);
        Reject(() => storage.LoadTransactions(date), "Invalid JSON/data rejected: " + badJson);
        Reject(() => storage.SaveTransaction(expense), "Saving refuses invalid existing data");
        Check(File.ReadAllText(path) == badJson, "Invalid original file remains unchanged");
    }
    string blocked = Path.Combine(root, "blocked");
    File.WriteAllText(blocked, "not a directory");
    try { new StorageService(blocked).SaveTransaction(income); throw new Exception("Write unexpectedly succeeded"); }
    catch (IOException) { Check(File.ReadAllText(blocked) == "not a directory", "I/O failure reported without destroying existing file"); }
    Check(!Directory.EnumerateFiles(root, "*.tmp", SearchOption.AllDirectories).Any(), "No temporary files left behind");
    Console.WriteLine($"All {passed} checks passed.");
}
finally
{
    if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
}
