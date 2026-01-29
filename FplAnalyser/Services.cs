using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace FplAnalyser;

public static class FplService
{
    private const string ApiUrl = "https://fantasy.premierleague.com/api/bootstrap-static/";

    public static async Task<FplRoot?> GetFplDataAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "FplAnalyser");
        try
        {
            return await client.GetFromJsonAsync<FplRoot>(ApiUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Błąd sieci] {ex.Message}");
            return null;
        }
    }
}

public static class FileManager
{
   
    public static string GetDefaultDirectory()
    {
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string appFolder = Path.Combine(docPath, "FplAnalyser");

        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        return appFolder;
    }

    public static string AskForFilePath(string defaultFileName)
    {
        string defaultDir = GetDefaultDirectory();
        string defaultFull = Path.Combine(defaultDir, defaultFileName);

        Console.WriteLine($"\n--- ZAPIS PLIKU ---");
        Console.WriteLine($"Domyślna lokalizacja: {defaultFull}");
        Console.Write("Wciśnij ENTER aby zaakceptować lub wpisz własną nazwę pliku: ");

        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return defaultFull;

        if (!input.Contains(Path.DirectorySeparatorChar))
            return Path.Combine(defaultDir, input.EndsWith(".csv") ? input : input + ".csv");

        return input;
    }
}

public static class CsvExporter
{
    public static async Task SaveToCsvAsync<T>(IEnumerable<T> data, string filePath, List<string>? specificColumns = null)
    {
        if (!data.Any())
        {
            Console.WriteLine("[Info] Brak danych do zapisu.");
            return;
        }

        var type = data.First()!.GetType();
        var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

       
        var propsToExport = specificColumns == null
            ? allProps
            : allProps.Where(p => specificColumns.Contains(p.Name)).ToArray();
        
        if (!propsToExport.Any()) propsToExport = allProps;

        var sb = new StringBuilder();

     
        sb.AppendLine(string.Join(";", propsToExport.Select(p => p.Name)));

     
        foreach (var item in data)
        {
            var values = propsToExport.Select(p =>
            {
                var val = p.GetValue(item, null);
              
                return val switch
                {
                    double d => d.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    decimal dec => dec.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                    _ => val?.ToString() ?? ""
                };
            });
            sb.AppendLine(string.Join(";", values));
        }

        try
        {
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            Console.WriteLine($"[Sukces] Zapisano plik: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Błąd zapisu] {ex.Message}");
        }
    }
}