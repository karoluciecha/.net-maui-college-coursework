namespace WordleApp.Models
{
    public class GameRecord
    {
        public string PlayerName { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int Attempts { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}