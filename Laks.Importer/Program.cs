using System.Text.Json;
using Laks.Importer;

var credentialsJson = await File.ReadAllTextAsync("credentials.json");
var credentials = JsonDocument.Parse(credentialsJson);
var connectionString = credentials.RootElement.GetProperty("databaseString").GetString()
    ?? throw new InvalidOperationException("databaseString not found in credentials.json");

var db = new FishDatabase(connectionString);

// Get data from Google Sheets
Console.WriteLine("Connecting to Google Sheets...");
var sheetsConnector = new SheetsConnector("service-account.json");
var rows = await sheetsConnector.GetRowsAsync();
if (rows is null)
{
    Console.WriteLine("No data found!");
    return;
}
Console.WriteLine($"Sheet received: {rows.Count} rows");

// Add any new person names from the sheet
var sheetNames = rows
    .Where(r => r.Count > 1 && !string.IsNullOrWhiteSpace(r[1].ToString()))
    .Select(r => r[1].ToString()!)
    .Distinct()
    .ToList();

var existingNames = await db.GetAllNamesAsync();
var newNames = sheetNames.Where(n => !existingNames.ContainsKey(n)).ToList();
foreach (var name in newNames)
    await db.AddPersonAsync(name);

if (newNames.Count > 0)
    Console.WriteLine($"Added {newNames.Count} new anglers");

// Resolve person names → IDs
var names = newNames.Count > 0 ? await db.GetAllNamesAsync() : existingNames;
Console.WriteLine($"Loaded {names.Count} known anglers");

// Convert rows to catch models
Console.WriteLine("Creating data models...");
var converter = new DataConverter(names);
var catches = converter.GetCatches(rows);
Console.WriteLine($"Created {catches.Count} catches");

// Upsert into database
Console.WriteLine("Syncing catches to the database...");
var (added, updated) = await db.UpsertCatchesAsync(catches);
Console.WriteLine($"Done — {added} new catches added, {updated} existing catches updated");
