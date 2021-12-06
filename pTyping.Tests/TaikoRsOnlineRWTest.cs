using System.IO;
using pTyping.Online.Taiko_rs;
using Xunit;

namespace pTyping.Tests {
    public class TaikoRsOnlineRWTest {
        public const string BASICSTRING = "Hello World!";
        public const string UNICODESTRING =
            "༿㹚Ңᮾц憲䲊㇎ߤ渘ភ⋵琌∓ᝪ來攀户Ꮇᐲώ澑婀晹ᡬ扞毩ၱ⊐䟶獗㚜㧤䅖䯒吀ⳟ㖒ˤҟࠀ䮥壟献屖㽓䓪姱◚仴怽綏䙥䘳㰵俄妚氹稦◰䠥叮塆撞㉍ୗ朊ᆼᖟ抩ܑḞ倹ߖ᧭嵗晦湵㿈濾汅劾ྴẴ4෰劂哎㜁呟烷㌑糐暌䫎毮嫍䲥⚋纞㖫䀜殧清䔴ⷦ⡌叓勂峮狮ៀ狮兰篑㳌扇涢檋嫓ᴍ墉哼檔⧣爴潍孢也ࠗ懼撹⼆᱉哑ℬ㈢㉞玍ᓐ⛓Ԓ㶃䆣吚㵩၄滫௧ゃᰈ⭍澀癮䝹䏖⺿碝࿏䫲䀍泎姩╾♥᛹曎ྈᄞ簽⛮㰂焬枛ᱽ忏甴庾აล඾平干䒈Ʋ࢕砟䝄摗勱ᒦᡚ䶃ⶄ掵ᦵ偤纚䝵⡆ᯖ瞠א炘༼㺳ᴗ孭⚚⁷ᠯ๶ᠾ噼づ粽⸐㧍缆唁燨⧧渍癕禟凇ઙ淙歱噬湋ຢݏő晫橫䑐擫㮼㝊嗶䔇窚⋷嫝烆篋翩儰ܦ䩂ω爜३ڳ不㆓ḕᮻ㖟ᄎ᪶ɦᓂ㪖ᗮ㮙㹘䇭乲痿巄⽲࿾博䛬䧱⟋⻯㩝楍ԍ敾倲Ƕ⡽棪瑞ᘙᾱ参ͅ䃂ሥⴆ缌⻠੣";

        [Fact]
        public void BasicStringTest() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write(BASICSTRING);

            writer.Flush();
            writer.Close();

            MemoryStream  readString = new(stream.ToArray());
            TaikoRsReader reader     = new(readString);

            Assert.True(reader.ReadString() == BASICSTRING, "Reading/Writing a basic string failed!");
        }

        [Fact]
        public void UnicodeStringTest() {
            MemoryStream  stream = new();
            TaikoRsWriter writer = new(stream);

            writer.Write(UNICODESTRING);

            writer.Flush();
            writer.Close();

            MemoryStream  readString = new(stream.ToArray());
            TaikoRsReader reader     = new(readString);

            Assert.True(reader.ReadString() == UNICODESTRING, "Reading/Writing a unicode string failed!");
        }
    }
}