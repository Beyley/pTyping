using Kettu;

namespace pTyping.Engine;

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

public class LoggerLevelEditorInfo : LoggerLevel {
    private LoggerLevelEditorInfo() {}

    public override string Name => "EditorInfo";

    public static LoggerLevelEditorInfo Instance = new();
}

public class LoggerLevelFurballFormInfo : LoggerLevel {
    private LoggerLevelFurballFormInfo() {}

    public override string Name => "FurballFormInfo";

    public static LoggerLevelFurballFormInfo Instance = new();
}

public class LoggerLevelPlayerInfo : LoggerLevel {
    private LoggerLevelPlayerInfo() {}

    public override string Name => "FurballFormInfo";

    public static LoggerLevelPlayerInfo Instance = new();
}