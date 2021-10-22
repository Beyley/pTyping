using System.Text;

namespace pTyping {
    internal class Program {
        private static void Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using pTypingGame game = new();
            game.Run();
        }
    }
}
