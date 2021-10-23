using Kettu;

namespace pTyping.LoggingLevels {
    public class LoggerLevelSongInfo : LoggerLevel {
        private LoggerLevelSongInfo() {}
        public override string Name => "SongInfo";

        public static LoggerLevelSongInfo Instance = new();
    }

    public class LoggerLevelSongManagerUpdateInfo : LoggerLevel {
        private LoggerLevelSongManagerUpdateInfo() {}
        
        public override string Name => "SongManagerInfo";

        public static LoggerLevelSongManagerUpdateInfo Instance = new();
    }

    public class LoggerLevelOnlineInfo : LoggerLevel {
        private LoggerLevelOnlineInfo() {}

        public override string Name => "OnlineInfo";

        public static LoggerLevelOnlineInfo Instance = new();
    }

    public class LoggerLevelChatMessage : LoggerLevel {
        private LoggerLevelChatMessage() {}

        public override string Name => "ChatMessage";

        public static LoggerLevelChatMessage Instance = new();
    }

    public class LoggerLevelModInfo : LoggerLevel {
        private LoggerLevelModInfo() {}

        public override string Name => "ModInfo";

        public static LoggerLevelModInfo Instance = new();
    }
}
