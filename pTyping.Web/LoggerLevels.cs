using Kettu;

namespace pTyping.Web; 

public class LoggerLevelHTTPServer : LoggerLevel {
    public override string Name => "HTTP Server";

    public static readonly LoggerLevelHTTPServer Instance = new();
    
    private LoggerLevelHTTPServer() {
        
    }
}

public class LoggerLevelGopherServer : LoggerLevel {
    public override string Name => "Gopher Server";

    public static readonly LoggerLevelGopherServer Instance = new();
    
    private LoggerLevelGopherServer() {
        
    }
}

public class LoggerLevelServer : LoggerLevel {
    public override string Name => "Web";

    public static readonly LoggerLevelServer Instance = new();
    
    private LoggerLevelServer() {
        
    }
}
