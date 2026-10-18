using BudgetTracker.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetTracker.Services;

public class StorageService
{
    private readonly string _dataDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StorageService(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        _dataDirectory = dataDirectory;
    }

    public string GetFilePath(Transaction transaction) =>
        GetFilePath(DateOnly.FromDateTime(transaction.Timestamp.Date));

    private string GetFilePath(DateOnly date) => Path.Combine(_dataDirectory,
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".json");

    public List<Transaction> LoadTransactions(DateOnly date)
    {
        string path = GetFilePath(date);
        try
        {
            string json = File.ReadAllText(path);
            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, JsonOptions)
                ?? throw new JsonException($"Expected a transaction array in {path}.");
            if (transactions.Any(t => t is null || t.Amount <= 0 ||
                !Enum.IsDefined(t.Type) || t.Id == Guid.Empty || t.Description is null ||
                DateOnly.FromDateTime(t.Timestamp.Date) != date))
            {
                throw new JsonException($"Invalid transaction data in {path}.");
            }
            return transactions;
        }
        catch (FileNotFoundException) { return new List<Transaction>(); }
        catch (DirectoryNotFoundException) { return new List<Transaction>(); }
    }

    public void SaveTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (transaction.Amount <= 0 || !Enum.IsDefined(transaction.Type) ||
            transaction.Id == Guid.Empty || transaction.Description is null)
        {
            throw new ArgumentException("Invalid transaction.", nameof(transaction));
        }

        var date = DateOnly.FromDateTime(transaction.Timestamp.Date);
        var transactions = LoadTransactions(date);
        if (transactions.Any(t => t.Id == transaction.Id))
            throw new InvalidOperationException("This transaction has already been saved.");
        transactions.Add(transaction);

        Directory.CreateDirectory(_dataDirectory);
        string path = GetFilePath(transaction);
        string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            // Finish writing before replacing the existing daily file.
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(transactions, JsonOptions));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
