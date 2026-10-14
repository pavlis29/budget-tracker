using BudgetTracker.Models;
using System.Globalization;
namespace BudgetTracker.Services;

public class StorageService
{
    private readonly string _dataDirectory;
    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
    }
    public string GetFilePath(Transaction transaction)
    {
        string fileName = transaction.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".json";

        return Path.Combine(_dataDirectory, fileName);
    }
}