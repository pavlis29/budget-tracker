# Budget Tracker

A simple C# / .NET 10 console application for tracking income and expenses.

```sh
dotnet run --project BudgetTracker
```

- Option **1** saves a positive income or expense amount. Amount input uses your system's decimal separator.
- Option **3** reloads a daily report from disk. Enter `yyyy-MM-dd`, or press Enter for today.
- Option **2** (removal) is still a placeholder.

## Daily JSON storage

Files are stored in `data/yyyy-MM-dd.json`, relative to the working directory from which you start the app. Use the same working directory on subsequent runs to see the same data. The filename uses the date in the transaction's timestamp, including its original offset, and an invariant Gregorian calendar.

Each file contains a JSON array. `StorageService.SaveTransaction` loads the existing array, adds one transaction and serializes the complete list using `System.Text.Json`. `LoadTransactions(DateOnly)` deserializes it back into records; a missing file means an empty day. Enum names are readable strings (`Income` / `Expense`); IDs, timestamps and decimal amounts round-trip unchanged.

The service creates the data directory on the first save. It writes to a temporary file in the same directory before replacing the daily file, so an interrupted serialization/write does not truncate the previous file. Invalid JSON or invalid transaction data stops saving instead of silently resetting the file. The console reports read/write errors and confirms success only after the save completes. Duplicate IDs are rejected within a day. Local data and logs are ignored by Git.

This is intended for **one running application instance**. Concurrent writers are not synchronized; they could overwrite each other's changes. File replacement is not a backup or a guarantee against power loss.

## Verification

Run the dependency-free regression checks:

```sh
dotnet run --project BudgetTracker.Tests
```

Checks cover multiple saves, fresh-instance loading, separate dates, decimals, Unicode, offsets, invariant filenames, missing files, invalid JSON/data, duplicate IDs and I/O failure. Test data is created in a temporary directory and removed afterward.

Manual restart example (with a dot-decimal locale): add Income `1440.50` and Expense `110.25`, exit, restart from the same working directory and choose option 3 for the date of entry. Expect two transactions, income `1440.50`, expense `110.25` and balance `1330.25`. With a comma-decimal locale, enter `1440,50` and `110,25` instead.
