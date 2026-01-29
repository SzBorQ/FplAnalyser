using System.Text.Json.Serialization;

namespace FplAnalyser;

public class FplRoot
{
    [JsonPropertyName("elements")] public List<Player> Players { get; set; } = new();
    [JsonPropertyName("teams")] public List<Team> Teams { get; set; } = new();
    [JsonPropertyName("element_types")] public List<Position> Positions { get; set; } = new();
}

public class Player
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = "";
    [JsonPropertyName("second_name")] public string SecondName { get; set; } = "";
    [JsonPropertyName("web_name")] public string WebName { get; set; } = "";
    [JsonPropertyName("team")] public int TeamId { get; set; }
    [JsonPropertyName("element_type")] public int ElementType { get; set; }
    [JsonPropertyName("now_cost")] public int NowCost { get; set; } 
    [JsonPropertyName("total_points")] public int TotalPoints { get; set; }
    [JsonPropertyName("minutes")] public int Minutes { get; set; }
    [JsonPropertyName("goals_scored")] public int GoalsScored { get; set; }
    [JsonPropertyName("assists")] public int Assists { get; set; }
    [JsonPropertyName("yellow_cards")] public int YellowCards { get; set; }
    [JsonPropertyName("clean_sheets")] public int CleanSheets { get; set; }
    
    [JsonPropertyName("form")] public string Form { get; set; } = "0";
    [JsonPropertyName("selected_by_percent")] public string SelectedByPercent { get; set; } = "0";
    [JsonPropertyName("threat")] public string Threat { get; set; } = "0";
    [JsonPropertyName("creativity")] public string Creativity { get; set; } = "0";
    [JsonPropertyName("ict_index")] public string IctIndex { get; set; } = "0";
}

public class Team
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class Position
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("singular_name_short")] public string NameShort { get; set; } = "";
}
public record PlayerEnhanced(
    int Id,
    string FullName,
    string WebName,
    string TeamName,
    string Position,
    decimal Cost,
    int Points,
    int Minutes,
    int Goals,
    int Assists,
    int CleanSheets,
    int YellowCards,
    double Form,
    double SelectedBy,
    double Threat,
    double Creativity,
    double IctIndex
);