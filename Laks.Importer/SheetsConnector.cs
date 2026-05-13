using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace Laks.Importer;

public class SheetsConnector
{
    private static readonly string[] Scopes = [SheetsService.Scope.SpreadsheetsReadonly];
    private const string ApplicationName = "Laks Importer";
    private const string SpreadsheetId = "1-52UHYkyix78jPkDsXV5ilOAjAlRksCjHxmPFwpZlEc";

    public async Task<IList<IList<object>>?> GetRowsAsync(string range = "A:L")
    {
        await using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true));

        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });

        var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        var response = await request.ExecuteAsync();

        if (response.Values is { Count: > 0 })
            return response.Values;

        Console.WriteLine("No data found.");
        return null;
    }
}