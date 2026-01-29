using System.Globalization;

namespace FplAnalyser;

public class Analyzer
{
    private readonly List<PlayerEnhanced> _players;

    public Analyzer(FplRoot rawData)
    {
        _players = (from p in rawData.Players
                    join t in rawData.Teams on p.TeamId equals t.Id
                    join pos in rawData.Positions on p.ElementType equals pos.Id
                    select new PlayerEnhanced(
                        p.Id, $"{p.FirstName} {p.SecondName}", p.WebName, t.Name, pos.NameShort,
                        p.NowCost / 10.0m,
                        p.TotalPoints,
                        p.Minutes, p.GoalsScored, p.Assists,
                        p.CleanSheets, p.YellowCards,
                        ParseDouble(p.Form), ParseDouble(p.SelectedByPercent),
                        ParseDouble(p.Threat), ParseDouble(p.Creativity), ParseDouble(p.IctIndex)
                    )).ToList();
    }

    public List<PlayerEnhanced> GetAllPlayers() => _players;

    public List<string> GetAllTeams() => _players.Select(p => p.TeamName).Distinct().OrderBy(x => x).ToList();
    public List<string> GetAllPositions() => _players.Select(p => p.Position).Distinct().ToList();
    public List<PlayerEnhanced> FindPlayersByName(string s) => _players.Where(p => p.FullName.Contains(s, StringComparison.OrdinalIgnoreCase) || p.WebName.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
    public List<PlayerEnhanced> GetPlayersByGroup(string team, string pos) => _players.Where(p => (team == "WSZYSCY" || p.TeamName == team) && (pos == "WSZYSCY" || p.Position == pos)).ToList();


    public Dictionary<string, Func<IEnumerable<object>>> GetInsights()
    {
        return new Dictionary<string, Func<IEnumerable<object>>>
        {
            {
                "Enablers ",
                // Najlepsi grający tani zawodnicy
                () => _players.Where(p => p.Cost <= 4.5m && p.Minutes > 500)
                              .OrderByDescending(p => p.Points)
                              .Select(p => new { p.WebName, p.TeamName, Pos = p.Position, p.Cost, p.Points }) // To będą kolumny
            },
            {
                "Różnice",
                // Najlepsi zawodnicy z niskim posiadaniem
                () => _players.Where(p => p.Form > 5.0 && p.SelectedBy < 5.0)
                              .OrderByDescending(p => p.Form)
                              .Select(p => new { p.WebName, p.TeamName, p.Form, Owned = p.SelectedBy + "%", p.Cost })
            },
            {
                "Kreatywni obrońcy",
                () => _players.Where(p => p.Position == "DEF")
                              .OrderByDescending(p => p.Creativity)
                              .Select(p => new { p.WebName, p.TeamName, p.Creativity, p.Assists, p.Points })
            },
            {
                "xG Underperformers",
                () => _players.Where(p => p.Threat > 250 && p.Goals < 2)
                              .OrderByDescending(p => p.Threat)
                              .Select(p => new { p.WebName, Goals = p.Goals, p.Threat, Diff = (p.Threat - p.Goals*100).ToString("F0") })
            },
            {
            "Talizmany (Liderzy udziału w punktach drużyny)",
            () => {
                // Suma punktów dla każdej drużyny
                var teamTotals = _players.GroupBy(p => p.TeamName)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Points));

                return _players
                    .Where(p => teamTotals[p.TeamName] > 0)
                    .Select(p => new {
                        p.WebName,
                        p.TeamName,
                        PlayerPts = p.Points,
                        TeamPts = teamTotals[p.TeamName],
                        Contribution = ((double)p.Points / teamTotals[p.TeamName] * 100)
                    })
                    .OrderByDescending(x => x.Contribution)
                    .Select(x => new {
                        x.WebName,
                        x.TeamName,
                        x.PlayerPts,
                        Share = x.Contribution.ToString("F1") + "%"
                    });
            }
        },
            {
                "Punkty per 90min",
                () => _players.Where(p => p.Minutes > 200)
                              .Select(p => new {
                                  p.WebName, p.TeamName, p.Minutes, p.Points,
                                  PtsPer90 = ((double)p.Points / p.Minutes * 90).ToString("F2")
                              })
                              .OrderByDescending(x => x.PtsPer90)
            },
            {
                "Zagrożeni zawieszeniem",
                () => _players.Where(p => p.YellowCards >= 4)
                              .OrderByDescending(p => p.YellowCards)
                              .Select(p => new { p.WebName, p.TeamName, Yellows = p.YellowCards, Risk = "HIGH" })
            },
            {
                "Punkty per milion",
                () => _players.Where(p => p.Minutes > 500 && p.Cost > 0)
                              .Select(p => new { p.WebName, p.TeamName, p.Points, p.Cost, PtsPerMil = ((double)p.Points / (double)p.Cost).ToString("F2") })
                              .OrderByDescending(x => x.PtsPerMil)
            },
            {
                "Bramkarze z dużą liczbą interwencji",
                () => _players.Where(p => p.Position == "GKP" && p.Minutes > 500)
                              .OrderByDescending(p => p.Points)
                              .Select(p => new { p.WebName, p.TeamName, p.Points, CleanSheets = p.CleanSheets, Cost = p.Cost })
            },
        
        {
            "Najwięcej pkt za czyste konta",
            () => _players
                .Select(p => {
                    //Każda pozycja dostaje inną liczbę punktów za czyste konto
                    int pointsPerCs = p.Position switch {
                        "GKP" => 4,
                        "DEF" => 4,
                        "MID" => 1,
                        _ => 0
                    };
                    return new { p.WebName, p.TeamName, p.Position, p.CleanSheets, CsPoints = p.CleanSheets * pointsPerCs };
                })
                .Where(x => x.CsPoints > 0)
                .OrderByDescending(x => x.CsPoints)
        },
        {
            "Największa kreatywność i wskaźnik ICT",
            () => _players
                .Where(p => p.Minutes > 450)
                .Select(p => {
                    double creativeEfficiency = (p.Creativity + p.IctIndex) * p.Form / (double)p.Cost;
                    return new {
                        p.WebName,
                        p.TeamName,
                        p.Cost,
                        p.Form,
                        Efficiency = creativeEfficiency.ToString("F2")
                    };
                })
                .OrderByDescending(x => double.Parse(x.Efficiency))
        },
        {
            "Forma względem Ceny",
            () => {
                // Szukamy zawodników, których forma rośnie najszybciej w stosunku do średniej formy ich przedziału cenowego
                var priceBrackets = _players.GroupBy(p => Math.Floor((double)p.Cost))
                    .ToDictionary(g => g.Key, g => g.Average(p => p.Form));

                return _players
                    .Where(p => p.Minutes > 200 && p.Form > 0)
                    .Select(p => new {
                        p.WebName,
                        p.TeamName,
                        p.Cost,
                        p.Form,
                        BracketAvg = priceBrackets[Math.Floor((double)p.Cost)],
                        PerformanceIndex = (p.Form / priceBrackets[Math.Floor((double)p.Cost)])
                    })
                    .OrderByDescending(x => x.PerformanceIndex)
                    .Select(x => new {
                        x.WebName,
                        x.TeamName,
                        Price = x.Cost,
                        x.Form,
                        VsAvg = x.PerformanceIndex.ToString("F2") + "x"
                    });
            }
        } };
    }

    private static double ParseDouble(string val) =>
        double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double res) ? res : 0.0;
}