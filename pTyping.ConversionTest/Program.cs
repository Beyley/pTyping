using System.Text;
using pTyping.Shared;
TypingConversions.LoadConversion();

StringBuilder builder = new StringBuilder();

string text = @"I
Белая армия, чёрный барон
Снова готовят нам царский трон,
Но от тайги до британских морей
Красная Армия всех сильней.

Припев:
Так пусть же Красная
Сжимает властно
Свой штык мозолистой рукой,
И все должны мы
Неудержимо
Идти в последний смертный бой!

II
Красная Армия, марш марш вперёд!
Реввоенсовет нас в бой зовёт.
Ведь от тайги до британских морей
Красная Армия всех сильней!

Припев

III
Мы раздуваем пожар мировой,
Церкви и тюрьмы сравняем с землёй.
Ведь от тайги до британских морей
Красная Армия всех сильней!

Припев";

Dictionary<string, List<string>> dict = TypingConversions.Conversions[TypingConversions.ConversionType.StandardRussian];
for (int i = 0; i < text.Length; i++) {
	if (text[i] == ' ') {
		builder.Append(' ');
		continue;
	}
	if (text[i] == '\n') {
		builder.Append('\n');
		continue;
	}

	List<string> a = dict[text[i].ToString()];
	builder.Append(a[0]);
	if (a.Count > 1) {
		//Append the rest of the strings in parentheses to the builder
		builder.Append('(');
		for (int j = 1; j < a.Count; j++) {
			builder.Append(a[j]);
			if (j != a.Count - 1)
				builder.Append(", ");
		}
		builder.Append(')');
	}
}

string final = builder.ToString();

Console.WriteLine(final);
