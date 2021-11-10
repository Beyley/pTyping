using System.Threading.Tasks;
using WebSocketSharp;

namespace pTyping.Online.Taiko_rs {
    public static class WebSocketExtensions {
        public static async Task SendRealAsync(this WebSocket socket, byte[] data) {
            if (!(pTypingGame.OnlineManager as TaikoRsOnlineManager).IsAlive) {
                await pTypingGame.OnlineManager.Logout();
                return;
            }

            await Task.Run(() => socket.Send(data));
        }
    }
}
