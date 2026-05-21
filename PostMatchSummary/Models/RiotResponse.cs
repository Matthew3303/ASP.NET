using System.Text.Json.Serialization;

namespace PostMatchSummary.Models
{
    public class RiotResponse
    {
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; } = new();

        [JsonPropertyName("info")]
        public MatchInfo Info { get; set; } = new();
    }

    public class Metadata
    {
        [JsonPropertyName("matchId")]
        public string MatchId { get; set; } = string.Empty;
    }

    public class MatchInfo
    {
        [JsonPropertyName("gameDuration")]
        public int GameDuration { get; set; }

        [JsonPropertyName("gameMode")]
        public string GameMode { get; set; } = string.Empty;

        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;

        [JsonPropertyName("gameCreation")]
        public long GameCreation { get; set; }

        [JsonPropertyName("participants")]
        public List<Participant> Participants { get; set; } = new();
    }

    public class Participant
    {
        [JsonPropertyName("riotIdGameName")]
        public string RiotIdGameName { get; set; } = string.Empty;

        [JsonPropertyName("riotIdTagLine")]
        public string RiotIdTagLine { get; set; } = string.Empty;

        [JsonPropertyName("summonerName")]
        public string SummonerName { get; set; } = string.Empty;

        [JsonPropertyName("championName")]
        public string ChampionName { get; set; } = string.Empty;

        [JsonPropertyName("individualPosition")]
        public string IndividualPosition { get; set; } = string.Empty;

        [JsonPropertyName("kills")]
        public int Kills { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("assists")]
        public int Assists { get; set; }

        [JsonPropertyName("totalMinionsKilled")]
        public int TotalMinionsKilled { get; set; }

        [JsonPropertyName("neutralMinionsKilled")]
        public int NeutralMinionsKilled { get; set; }

        [JsonPropertyName("goldEarned")]
        public int GoldEarned { get; set; }

        [JsonPropertyName("win")]
        public bool Win { get; set; }

        [JsonPropertyName("teamId")]
        public int TeamId { get; set; }
    }
}