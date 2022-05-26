using System.Text;

namespace DiscordSDK;

public partial class LobbyManager {
    public IEnumerable<User> GetMemberUsers(long lobbyID) {
        int        memberCount = this.MemberCount(lobbyID);
        List<User> members     = new();
        for (int i = 0; i < memberCount; i++)
            members.Add(this.GetMemberUser(lobbyID, this.GetMemberUserId(lobbyID, i)));
        return members;
    }

    public void SendLobbyMessage(long lobbyID, string data, SendLobbyMessageHandler handler) {
        this.SendLobbyMessage(lobbyID, Encoding.UTF8.GetBytes(data), handler);
    }
}
