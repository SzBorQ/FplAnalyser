# FPL ANALYSER
FplAnalyser is a C# console application designed for analyzing data from the official Fantasy Premier League API. The tool allows users to evaluate player form, compare players, and export reports in CSV format.

# Overview
Upon launching the application, three options are available:
- Use predefined queries answering common questions asked by FPL managers. There are 12 queries available, each with an intuitive, self-explanatory title.
- Create a custom comparison.
- Compare all players.

After generating any comparison, users can export it to a chosen .csv file (by default saved in the generated `Documents/FplAnalyser` folder).

## Creating Custom Comparisons
To build a custom comparison, select the players you want to compare. You can add them individually (typing part of a surname displays a list of matching players to choose from by index) or in bulk by team and position. Once selected, the players are ready for comparison. The user can configure which statistics to include (by default: surname, team, position, price, total points, and form).

# Project Structure
## Models.cs
Contains type definitions for JSON deserialization as well as the application's internal data model.
- `FplRoot`, `Player`, `Team`: Classes mapping the raw JSON structure from the Premier League API, utilizing `[JsonPropertyName]` attributes for property binding.
- `PlayerEnhanced`: An extended type created by combining the deserialized data models.

## Analyzer.cs
Constructs the `PlayerEnhanced` collection and contains a series of LINQ queries operating on it.

## Program.cs
Handles the main application loop, along with:
- Managing the comparison workflow.
- Providing the `PrintDynamicTable` method, which utilizes reflection to format and display objects as a console table.

## Services.cs
- Communicates with the external API.
- Handles exporting data to .csv format.
