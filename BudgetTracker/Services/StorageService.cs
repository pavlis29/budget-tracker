using BudgetTracker.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BudgetTracker.Services;

public class StorageService
{
    private readonly string _dataDirectory;
    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        _jsonOptions.WriteIndented = true;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }
    public string GetFilePath(Transaction transaction)
    {
        string fileName = transaction.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".json";

        return Path.Combine(_dataDirectory, fileName);
    }

    public void SaveTransaction(Transaction transaction)
    {
        string path = GetFilePath(transaction);
        List<Transaction> transactions = new List<Transaction>();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            transactions = JsonSerializer.Deserialize<List<Transaction>>
            (
                json,
                _jsonOptions
            ) ?? new List<Transaction>();
        }
         
        transactions.Add(transaction);
    }
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();
}