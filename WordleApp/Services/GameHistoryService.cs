using System.Text.Json;
using WordleApp.Models;

namespace WordleApp.Services
{
    public static class GameHistoryService
    {
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "GameHistory.json");

        public static List<GameRecord> LoadRecords()
        {
            if (!File.Exists(FilePath))
                return new List<GameRecord>();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<GameRecord>>(json) ?? new List<GameRecord>();
        }

        public static void SaveRecord(GameRecord record)
        {
            var records = LoadRecords();
            records.Insert(0, record); // Add latest record at the beginning
            if (records.Count > 15)
                records = records.Take(15).ToList(); // Keep only the latest 15 records

            var json = JsonSerializer.Serialize(records);
            File.WriteAllText(FilePath, json);
        }
    }
}