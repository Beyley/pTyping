using Furball.Engine.Engine.Config;
using Furball.Volpe.Evaluation;

namespace pTyping.Engine;

// ReSharper disable once InconsistentNaming
public class pTypingConfig : VolpeConfig {
    public override string Name => "ptyping";

    public bool   FpsBasedOnMonitorHz  => this.Values["fps_based_on_monitorhz"].ToBoolean().Value;
    public double GameplayFpsMult      => this.Values["gameplay_fps_mult"].ToNumber().Value;
    public double MenuFpsMult          => this.Values["menu_fps_mult"].ToNumber().Value;
    public bool   UnlimitedFpsMenu     => this.Values["unlimited_fps_menu"].ToBoolean().Value;
    public bool   UnlimitedFpsGameplay => this.Values["unlimited_fps_gameplay"].ToBoolean().Value;

    public string Username => this.Values["username"].ToStringValue().Value;
    public string Password => this.Values["password"].ToStringValue().Value;
    
    public static pTypingConfig Instance = new();
    
    public pTypingConfig() {
        this.Values["fps_based_on_monitorhz"] = new Value.Boolean(true);
        this.Values["gameplay_fps_mult"]      = new Value.Number(4);
        this.Values["menu_fps_mult"]          = new Value.Number(1);

        this.Values["unlimited_fps_menu"]     = new Value.Boolean(false);
        this.Values["unlimited_fps_gameplay"] = new Value.Boolean(false);

        this.Values["username"] = new Value.String("");
        this.Values["password"] = new Value.String("");
    }
}
