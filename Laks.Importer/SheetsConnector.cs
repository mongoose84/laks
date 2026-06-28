using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace Laks.Importer;

public class SheetsConnector
{
    private const string ApplicationName = "Laks Importer";
    private const string SpreadsheetId = "1-52UHYkyix78jPkDsXV5ilOAjAlRksCjHxmPFwpZlEc";

    private readonly SheetsService _service;

    public SheetsConnector(string serviceAccountKeyPath)
    {
        var credential = GoogleCredential
            .FromFile(serviceAccountKeyPath)
            .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

        _service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }

    public async Task<IList<IList<object>>?> GetRowsAsync(string range = "A:L")
    {
        var request = _service.Spreadsheets.Values.Get(SpreadsheetId, range);
        var response = await request.ExecuteAsync();

        if (response.Values is { Count: > 0 })
            return response.Values;

        Console.WriteLine("No data found.");
        return null;
    }
}
