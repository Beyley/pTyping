using System.ComponentModel;
using Newtonsoft.Json;
using Realms;

namespace pTyping.Shared.ObjectModel;

[JsonObject(MemberSerialization.OptIn)]
public class DatabaseUser : RealmObject, IClonable<DatabaseUser>, IEquatable<DatabaseUser> {
	[JsonProperty, Description("The online ID of the user")]
	public long UserId { get; set; } //NOTE: the reason it is a long and not a uint is because Realm does not support unsigned integers

	[JsonProperty, Description("The username of the user")]
	public string Username { get; set; }

	public DatabaseUser Clone() {
		return new() {
			Username = this.Username,
			UserId   = this.UserId
		};
	}
	public bool Equals(DatabaseUser other) {
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.UserId == other.UserId && this.Username == other.Username;
	}
}
