namespace pTyping {
    internal class Program {
        private static void Main(string[] args) {
            System.Text.Encoding.RegisterProvider (System.Text.CodePagesEncodingProvider.Instance);
            
            using pTypingGame game = new();
            game.Run();
        }
    }
}
