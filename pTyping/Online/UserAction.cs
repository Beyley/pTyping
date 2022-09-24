using System;
using System.Diagnostics.CodeAnalysis;
using Furball.Engine.Engine.Helpers;

namespace pTyping.Online;

public class UserAction {
	public Bindable<UserActionType> Action     = new(UserActionType.Idle);
	public Bindable<string>         ActionText = new("");
	public Bindable<PlayMode>       Mode       = new(PlayMode.pTyping);

	public UserAction(UserActionType action, string actionText, PlayMode mode = PlayMode.pTyping) {
		this.Action     = new Bindable<UserActionType>(action);
		this.ActionText = new Bindable<string>(actionText);
		this.Mode       = new Bindable<PlayMode>(mode);
	}

	public UserAction() {}
}

public enum UserActionType : byte {
	Unknown = 0,
	Idle,
	Ingame,
	Leaving,
	Editing
}

public enum PlayMode : byte {
	Standard = 0,
	Taiko    = 1,
	Catch    = 2,
	Mania    = 3,
	Adofai   = 4,
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	pTyping = 5,
	Unknown = 255
}

internal static class PlayModeMethods {
	public static string GetString(this PlayMode mode) {
		return mode switch {
			PlayMode.Standard => "osu",
			PlayMode.Taiko    => "taiko",
			PlayMode.Catch    => "catch",
			PlayMode.Mania    => "mania",
			PlayMode.Adofai   => "adofai",
			PlayMode.pTyping  => "pTyping",
			PlayMode.Unknown  => "",
			_                 => throw new ArgumentOutOfRangeException(nameof (mode), mode, null)
		};
	}

	public static PlayMode FromString(string str) {
		return str switch {
			"osu"     => PlayMode.Standard,
			"taiko"   => PlayMode.Taiko,
			"catch"   => PlayMode.Catch,
			"mania"   => PlayMode.Mania,
			"adofai"  => PlayMode.Adofai,
			"pTyping" => PlayMode.pTyping,
			""        => PlayMode.Unknown,
			_         => throw new ArgumentOutOfRangeException(nameof (str), str, null)
		};
	}
}
