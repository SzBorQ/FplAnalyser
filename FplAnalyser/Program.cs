namespace FplAnalyser;

class Program
{
   
    private static List<string> _activeStats = new()
    {
        nameof(PlayerEnhanced.WebName), nameof(PlayerEnhanced.TeamName),
        nameof(PlayerEnhanced.Position), nameof(PlayerEnhanced.Cost),
        nameof(PlayerEnhanced.Points), nameof(PlayerEnhanced.Form)
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("Pobieranie danych...");
        var data = await FplService.GetFplDataAsync();
        if (data == null) return;

      
        var analyzer = new Analyzer(data);
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("--- FPLANALYSER ---");
            Console.WriteLine("1. Gotowe Zapytania");
            Console.WriteLine("2. Porównywarka");
            Console.WriteLine("3. Eksport Pełnej Bazy");
            Console.WriteLine("9. POMOC / INSTRUKCJA"); 
            Console.WriteLine("0. Wyjście");
            Console.Write("\nWybór: ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1": await RunInsightsModule(analyzer); break;
                case "2": await RunComparatorModule(analyzer); break;
                case "3":
                    string path = FileManager.AskForFilePath("FullDB.csv");
                    await CsvExporter.SaveToCsvAsync(analyzer.GetAllPlayers(), path);
                    break;
                case "9":
                    ShowHelp();
                    break;
                case "0": running = false; break;
                default:
                    Console.WriteLine($"\n[!] Nieznana komenda: '{input}'. Wyświetlam pomoc...");
                    Thread.Sleep(1000);
                    ShowHelp();
                    break;
            }
        }
    }


    static void ShowHelp()
    {
        Console.Clear();
        Console.WriteLine("FPLANALYSER  USAGE");

        Console.WriteLine("\n[NAWIGACJA]");
        Console.WriteLine(" - Wpisz numer opcji i wciśnij ENTER, aby zatwierdzić.");
        
        Console.WriteLine("\n[1. GOTOWE ZAPYTANIA]");
        Console.WriteLine(" - Po wyświetleniu tabeli możesz wpisać 'save', aby zapisać wynik do CSV.");
        
        Console.WriteLine("\n[2. PORÓWNYWARKA]");
        Console.WriteLine(" - Twórz własną listę zawodników do porównania."); ;
        Console.WriteLine(" - Dodając grupę możnesz jednocześnie dodać zawodników z jednej drużyny i pozycji");
        Console.WriteLine(" - 'Zarządzaj kolumnami' pozwala wybrać, jakie dane są widoczne.");
        Console.WriteLine(" - Opcja '5' generuje tabelę porównawczą dla graczy w koszyku.");

        Console.WriteLine("\n[3. EKSPORT PEŁNEJ BAZY]");
        Console.WriteLine(" - Eksportuje wszystkich zawodników i ich statystyki do jednego pliku CSV.");

        
        Console.WriteLine("Wciśnij dowolny klawisz, aby wrócić do menu...");
        Console.ReadKey();
    }

  
    static async Task RunInsightsModule(Analyzer analyzer)
    {
        var queries = analyzer.GetInsights();
        var keys = queries.Keys.ToList();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("--- WYBIERZ ZAPYTANIE ---");
            for (int i = 0; i < keys.Count; i++) Console.WriteLine($"{i + 1,2}. {keys[i]}");
            Console.WriteLine(" 0. Powrót");
            Console.Write("\nNumer: ");

            string? input = Console.ReadLine();
            
            if (!int.TryParse(input, out int choice))
            {
                Console.WriteLine($"\n[!] '{input}' nie jest poprawnym numerem.");
                Thread.Sleep(1500);
                ShowHelp();
                continue;
            }

            if (choice == 0) return;

            if (choice > 0 && choice <= keys.Count)
            {
                var queryName = keys[choice - 1];
                Console.WriteLine($"\nGeneruję: {queryName}...");

                
                var fullResult = queries[queryName]().ToList();

                if (!fullResult.Any())
                {
                    Console.WriteLine("Brak wyników dla tego zapytania.");
                    Console.ReadKey();
                    continue;
                }

                
                Console.Write($"Znaleziono {fullResult.Count} rekordów. Ile wyświetlić w tabeli? (Domyślnie 10): ");
                string? limitInput = Console.ReadLine();
                int limit = string.IsNullOrWhiteSpace(limitInput) ? 10 : (int.TryParse(limitInput, out int l) ? l : 10);

                
                var displayData = fullResult.Take(limit).ToList();
                PrintDynamicTable(displayData);
            
                Console.WriteLine($"\n[Enter] aby wrócić, lub wpisz 'save' aby zapisać WSZYSTKIE ({fullResult.Count}) wyniki do CSV.");
                if (Console.ReadLine()?.Trim().ToLower() == "save")
                {
                    string safeName = $"Raport_{DateTime.Now:HHmm}.csv";
                    string path = FileManager.AskForFilePath(safeName);
                    await CsvExporter.SaveToCsvAsync(fullResult, path);
                }
            }
            else
            {
                Console.WriteLine("\n[!] Wybrano numer spoza listy.");
                Thread.Sleep(1000);
            }
        }
    }

    static void PrintDynamicTable(IEnumerable<object> data)
    {
        if (data == null || !data.Any()) return;


        var firstItem = data.First();
        var properties = firstItem.GetType().GetProperties();


        int colWidth = 18;

        Console.WriteLine();

        foreach (var prop in properties)
        {
            string header = prop.Name.Length > colWidth ? prop.Name.Substring(0, colWidth - 1) : prop.Name;
            Console.Write($"{header.PadRight(colWidth)} | ");
        }
        Console.WriteLine("\n" + new string('-', properties.Length * (colWidth + 3)));
        
        foreach (var item in data)
        {
            foreach (var prop in properties)
            {
                var val = prop.GetValue(item);
                string display = val?.ToString() ?? "";

             
                if (display.Length > colWidth) display = display.Substring(0, colWidth - 3) + "..";

                Console.Write($"{display.PadRight(colWidth)} | ");
            }
            Console.WriteLine();
        }
        Console.WriteLine(new string('-', properties.Length * (colWidth + 3)));
    }

 
    static async Task RunComparatorModule(Analyzer analyzer)
    {
        var basket = new List<PlayerEnhanced>();
        var teams = analyzer.GetAllTeams();
        var positions = analyzer.GetAllPositions();
        teams.Insert(0, "WSZYSCY");
        positions.Insert(0, "WSZYSCY");

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"--- KREATOR PORÓWNAŃ (W koszyku: {basket.Count}) ---");
            Console.WriteLine("1. Dodaj zawodnika (Szukaj po nazwisku)");
            Console.WriteLine("2. Dodaj grupę (np. Napastnicy Chelsea)");
            Console.WriteLine("3. Zarządzaj widocznymi kolumnami");
            Console.WriteLine("4. Wyczyść koszyk");
            Console.WriteLine("5. GENERUJ TABELĘ / EKSPORT");
            Console.WriteLine("0. Powrót do menu głównego");
            Console.Write("\nWybór > ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "0": return;
                case "1":
                    AddToBasketByName(analyzer, basket);
                    break;
                case "2":
                    AddToBasketByGroup(analyzer, basket, teams, positions);
                    break;
                case "3": 
                    ManageColumns();
                    break;
                case "4": 
                    basket.Clear(); 
                    break;
                case "5":
                    if (basket.Any()) await ShowComparisonTable(basket);
                    else { Console.WriteLine("Koszyk jest pusty!"); Console.ReadKey(); }
                    break;
                default:
                
                    Console.WriteLine($"\n[!] Nieznana opcja: '{input}'. Wyświetlam pomoc...");
                    Thread.Sleep(1000);
                    ShowHelp();
                    break;
            }
        }
    }

    static void AddToBasketByName(Analyzer analyzer, List<PlayerEnhanced> basket)
    {
        Console.Write("Podaj fragment nazwiska: ");
        var hits = analyzer.FindPlayersByName(Console.ReadLine() ?? "");
        if (!hits.Any()) { Console.WriteLine("Brak wyników."); Thread.Sleep(1000); return; }

        Console.WriteLine("\nZnaleziono:");
        for (int i = 0; i < hits.Count; i++)
            Console.WriteLine($"{i + 1}. {hits[i].WebName} ({hits[i].TeamName} - {hits[i].Position})");

        Console.Write("Wybierz numer (0 anuluj): ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx > 0 && idx <= hits.Count)
        {
            var p = hits[idx - 1];
            if (!basket.Any(x => x.Id == p.Id)) basket.Add(p);
        }
    }

    static void AddToBasketByGroup(Analyzer analyzer, List<PlayerEnhanced> basket, List<string> teams, List<string> pos)
    {
        Console.WriteLine("\n--- Wybierz Drużynę ---");
        for (int i = 0; i < teams.Count; i++) Console.WriteLine($"{i}. {teams[i]}");
        if (!int.TryParse(Console.ReadLine(), out int tIdx) || tIdx < 0 || tIdx >= teams.Count) return;

        Console.WriteLine("\n--- Wybierz Pozycję ---");
        for (int i = 0; i < pos.Count; i++) Console.WriteLine($"{i}. {pos[i]}");
        if (!int.TryParse(Console.ReadLine(), out int pIdx) || pIdx < 0 || pIdx >= pos.Count) return;

        var newPlayers = analyzer.GetPlayersByGroup(teams[tIdx], pos[pIdx]);

        
        int addedCount = 0;
        foreach (var p in newPlayers)
        {
            if (!basket.Any(x => x.Id == p.Id))
            {
                basket.Add(p);
                addedCount++;
            }
        }
        Console.WriteLine($"Dodano {addedCount} unikalnych zawodników.");
        Thread.Sleep(1000);
    }

    static void ManageColumns()
    {
        var allProps = typeof(PlayerEnhanced).GetProperties().Select(p => p.Name).ToList();
        while (true)
        {
            Console.Clear();
            Console.WriteLine("--- KONFIGURACJA KOLUMN ---");
            for (int i = 0; i < allProps.Count; i++)
            {
                string marker = _activeStats.Contains(allProps[i]) ? "[X]" : "[ ]";
                Console.WriteLine($"{i + 1,2}. {marker} {allProps[i]}");
            }
            Console.WriteLine("\nWpisz numer aby przełączyć (0 by wrócić):");
            if (int.TryParse(Console.ReadLine(), out int c) && c > 0 && c <= allProps.Count)
            {
                string p = allProps[c - 1];
                if (_activeStats.Contains(p)) _activeStats.Remove(p); else _activeStats.Add(p);
            }
            else break;
        }
    }

    static async Task ShowComparisonTable(List<PlayerEnhanced> basket)
    {
        Console.Clear();
        Console.WriteLine("--- WYNIK PORÓWNANIA ---");

        
        foreach (var h in _activeStats) Console.Write($"{h.Substring(0, Math.Min(h.Length, 12)),-15} | ");
        Console.WriteLine("\n" + new string('-', _activeStats.Count * 18));

    
        var type = typeof(PlayerEnhanced);
        foreach (var p in basket)
        {
            foreach (var colName in _activeStats)
            {
                var val = type.GetProperty(colName)?.GetValue(p);
                string sVal = val switch
                {
                    double d => d.ToString("F1"),
                    decimal dec => dec.ToString("F1"),
                    _ => val?.ToString() ?? ""
                };
                Console.Write($"{sVal,-15} | ");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\nZapisać zestawienie do CSV? (t/n)");
        if (Console.ReadLine()?.ToLower() == "t")
        {
            string path = FileManager.AskForFilePath("Comparison.csv");
  
            await CsvExporter.SaveToCsvAsync(basket, path, _activeStats);
        }
        else
        {
            Console.WriteLine("Naciśnij klawisz...");
            Console.ReadKey();
        }
    }
}