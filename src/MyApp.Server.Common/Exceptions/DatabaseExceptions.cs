namespace Server.Common.Exceptions
{
    public class DatabaseOperationException : Exception
    {
        public DatabaseOperationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    public class DatabaseInitializationException : Exception
    {
        public DatabaseInitializationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    public class MatchNotFoundException : Exception
    {
        public string MatchId { get; }

        public MatchNotFoundException(string matchId) 
            : base($"Match with ID {matchId} was not found")
        {
            MatchId = matchId;
        }
    }

    public class PlayerNotFoundException : Exception
    {
        public string UserId { get; }

        public PlayerNotFoundException(string userId) 
            : base($"Player with ID {userId} was not found")
        {
            UserId = userId;
        }
    }

    public class DuplicatePlayerException : Exception
    {
        public DuplicatePlayerException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 