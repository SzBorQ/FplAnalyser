# FPL ANALYSER
FplAnalyser to konsolowa aplikacja w języku C# służąca do analizy danych z oficjalnego API Fantasy Premier League. Narzędzie pozwala na analizę formy zawodników, porównywanie graczy oraz generowanie raportów w formacie CSV.

# Zarys obsługi
Uruchamiając aplikację do dyspozycji są 3 możliwości:
- Skorzystanie z przygotowanych wcześniej zapytań odpowiadających na często zadawane pytania przez graczy FPL. Do dyspozycji jest 12 zapytań, każde opatrzone w miarę idiomatycznym tytułem,
- Utworzenie własnego porównania,
- Porównanie wszystkich zawodników.

Po każdorazowym utworzeniu porównania istnieje możliwość jego eksportu do pliku formatu .csv wybranego przez użytkownika (domyślnie w utworzonym folderze Documents/FplAnalyser.

## Tworzenie własnych porównań
Aby utworzyć własne porównania należy wybrać zawodników do porównywania. Dodawać można ich pojedynczo, po wpisaniu części nazwiska wyświetli się lista pasujących, z których należy wybrać numer, lub grupowo, ze względu na drużynę oraz pozycję. Wybrani zawodnicy są gotowi do porównania. Użytkownik może skonfigurować jakie dane mają zostać uwzględnione w porównaniu (domyślnie nazwisko, drużyna, pozycja, cena, suma punktów, forma).

# Struktura projektu
## Models.cs
Plik z definicjami typów służących do deserializacji JSON oraz wewnętrzny model aplikacji.
- FplRoot, Player, Team: Klasy odwzorowujące surową strukturę JSON z API Premier League. Używają atrybutów [JsonPropertyName] do mapowania pól.
- PlayerEnhanced: Typ powstały poprzez połączenie zdeserializowanych danych.

## Analyzer.cs
Tworzy listę PlayerEnhanced i zawiera serię zapytań LINQ odnośnie nich.

## Program.cs
Obsługuje główną pętle programu a ponadto:
- Zarządza porównywarką
- Zawiera metodę PrintDynamicTable wykorzystującą refleksję do wyświetlania obiektu w formie tabelki

## Services.cs
- Komunikuje się z API
- Zarządza eksportem danych do .csv


