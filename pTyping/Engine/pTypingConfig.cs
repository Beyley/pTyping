using System;
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

	public bool  Letterboxing  => this.Values["letterboxing"].ToBoolean().Value;
	public float LetterboxingX => (float)this.Values["letterboxing_x"].ToNumber().Value;
	public float LetterboxingY => (float)this.Values["letterboxing_y"].ToNumber().Value;
	public float LetterboxingW => (float)this.Values["letterboxing_w"].ToNumber().Value;
	public float LetterboxingH => (float)this.Values["letterboxing_h"].ToNumber().Value;

	public bool VideoBackgrounds {
		get => this.Values["video_backgrounds"].ToBoolean().Value;
		set => this.Values["video_backgrounds"] = new Value.Boolean(value);
	}

	public event EventHandler<double> MasterVolumeChanged;

	public double MasterVolume {
		get => this.Values["master_volume"].ToNumber().Value;
		set {
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (value == this.MasterVolume) return;

			this.Values["master_volume"] = new Value.Number(value);
			this.MasterVolumeChanged?.Invoke(this, value);
		}
	}

	public event EventHandler<double> BackgroundDimChanged;

	public double BackgroundDim {
		get => this.Values["background_dim"].ToNumber().Value;
		set {
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (value == this.BackgroundDim) return;

			this.Values["background_dim"] = new Value.Number(value);
			this.BackgroundDimChanged?.Invoke(this, value);
		}
	}

	public string ServerWebsocketUrl => this.Values["server_websocket_url"].ToStringValue().Value;
	public string ServerWebUrl       => this.Values["server_web_url"].ToStringValue().Value;

	public string Username => this.Values["username"].ToStringValue().Value;
	public string Password => this.Values["password"].ToStringValue().Value;

	public static pTypingConfig Instance = new pTypingConfig();

	public pTypingConfig() {
		this.Values["fps_based_on_monitorhz"] = new Value.Boolean(true);
		this.Values["gameplay_fps_mult"]      = new Value.Number(4);
		this.Values["menu_fps_mult"]          = new Value.Number(1);

		this.Values["unlimited_fps_menu"]     = new Value.Boolean(false);
		this.Values["unlimited_fps_gameplay"] = new Value.Boolean(false);

		this.Values["letterboxing"]   = new Value.Boolean(false);
		this.Values["letterboxing_x"] = new Value.Number(0);
		this.Values["letterboxing_y"] = new Value.Number(0);
		this.Values["letterboxing_w"] = new Value.Number(1280);
		this.Values["letterboxing_h"] = new Value.Number(720);

		this.Values["video_backgrounds"] = new Value.Boolean(true);

		this.Values["master_volume"]  = new Value.Number(1d);
		this.Values["background_dim"] = new Value.Number(0.5d);

		this.Values["server_websocket_url"] = new Value.String("wss://server.tataku.ca");
		this.Values["server_web_url"]       = new Value.String("https://scores.tataku.ca");

		this.Values["username"] = new Value.String("");
		this.Values["password"] = new Value.String("");
	}
}
