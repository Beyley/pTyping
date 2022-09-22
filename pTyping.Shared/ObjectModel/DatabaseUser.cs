using System.ComponentModel;
using Realms;

namespace pTyping.Shared.ObjectModel;

public class DatabaseUser : RealmObject, IClonable<DatabaseUser> {
    [Description("The online ID of the user")]
    public long UserId { get; set; }//NOTE: the reason it is a long and not a uint is because Realm does not support unsigned integers

    [Description("The username of the user")]
    public string Username { get; set; }

    public DatabaseUser Clone() => new() {
        Username = this.Username,
        UserId   = this.UserId
    };
}
