using Laks.Importer;

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("LAKS_CONNECTION_STRING")
      ?? throw new InvalidOperationException(
          "Provide a connection string as the first argument or set the LAKS_CONNECTION_STRING environment variable.");

var db = new FishDatabase(connectionString);

// Get data from Google Sheets
Console.WriteLine("Connecting to Google Sheets...");
var sheetsConnector = new SheetsConnector();
var rows = await sheetsConnector.GetRowsAsync();
if (rows is null)
{
    Console.WriteLine("No data found!");
    return;
}
Console.WriteLine($"Sheet received: {rows.Count} rows");

// Resolve person names → IDs
var names = await db.GetAllNamesAsync();
Console.WriteLine($"Loaded {names.Count} known anglers");

// Convert rows to catch models
Console.WriteLine("Creating data models...");
var converter = new DataConverter(names);
var catches = converter.GetCatches(rows);
Console.WriteLine($"Created {catches.Count} catches");

// Insert into database
Console.WriteLine("Adding catches to the database...");
var added = await db.AddCatchesAsync(catches);
Console.WriteLine($"Done — {added} new catches added ({catches.Count - added} skipped as duplicates)");
