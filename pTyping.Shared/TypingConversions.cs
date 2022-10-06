namespace pTyping.Shared;

public static class TypingConversions {
	public enum ConversionType {
		StandardHiragana,
		StandardEsperanto,
		StandardLatin,
		StandardRussian
	}

	public static readonly Dictionary<ConversionType, Dictionary<string, List<string>>> Conversions = new Dictionary<ConversionType, Dictionary<string, List<string>>>();

	public static void LoadConversion() {
		#region Standard Latin

		string slConversion = @"a	a
b	b
c	c
d	d
e	e
f	f
g	g
h	h
i	i
j	j
k	k
l	l
m	m
n	n
o	o
p	p
q	q
r	r
s	s
t	t
u	u
v	v
w	w
x	x
y	y
z	z
A	A
B	B
C	C
D	D
E	E
F	F
G	G
H	H
I	I
J	J
K	K
L	L
M	M
N	N
O	O
P	P
Q	Q
R	R
S	S
T	T
U	U
V	V
W	W
X	X
Y	Y
Z	Z
1	1
2	2
3	3
4	4
5	5
6	6
7	7
8	8
9	9
0	0
-	-
^	^
!	!
""	""
#	#
$	$
%	%
&	&
'	'
(	(
)	)
=	=
~	~
|	|
@	@
[	[
`	`
{	{
;	;
:	:
]	]
+	+
*	*
}	}
,	,
.	.
/	/
\	\
<	<
>	>
?	?
_	_
A	Ａ
B	Ｂ
C	Ｃ
D	Ｄ
E	Ｅ
F	Ｆ
G	Ｇ
H	Ｈ
I	Ｉ
J	Ｊ
K	Ｋ
L	Ｌ
M	Ｍ
N	Ｎ
O	Ｏ
P	Ｐ
Q	Ｑ
R	Ｒ
S	Ｓ
T	Ｔ
U	Ｕ
V	Ｖ
W	Ｗ
X	Ｘ
Y	Ｙ
Z	Ｚ
a	ａ
b	ｂ
c	ｃ
d	ｄ
e	ｅ
f	ｆ
g	ｇ
h	ｈ
i	ｉ
j	ｊ
k	ｋ
l	ｌ
m	ｍ
n	ｎ
o	ｏ
p	ｐ
q	ｑ
r	ｒ
s	ｓ
t	ｔ
u	ｕ
v	ｖ
w	ｗ
x	ｘ
y	ｙ
z	ｚ
1	１
2	２
3	３
4	４
5	５
6	６
7	７
8	８
9	９
0	０
-	ー
^	＾
!	！
""	”
#	＃
$	＄
%	％
&	＆
'	’
(	（
)	）
=	＝
~	～
~	〜
|	｜
@	＠
[	「
`	‘
{	｛
;	；
:	：
]	」
+	＋
*	＊
}	｝
,	、
.	。
/	・
\	￥
<	＜
>	＞
?	？
_	＿
///	…
-	－
[	［
]	］
,	，
.	．
/	／
";

		#endregion

		#region Japanese Hiragana

		string jpConversion = $@"{slConversion}
a	あ
i	い
yi	い
u	う
wu	う
whu	う
e	え
o	お
la	ぁ
xa	ぁ
li	ぃ
xi	ぃ
lyi	ぃ
xyi	ぃ
lu	ぅ
xu	ぅ
le	ぇ
xe	ぇ
lye	ぇ
xye	ぇ
lo	ぉ
xo	ぉ
ye	いぇ
wha	わ
whi	うぃ
wi	うぃ
whe	うぇ
we	うぇ
who	うぉ
ka	か
ca	か
ki	き
ku	く
cu	く
qu	く
ke	け
ko	こ
co	こ
lka	ヵ
xka	ヵ
lke	ヶ
xke	ヶ
ga	が
gi	ぎ
gu	ぐ
ge	げ
go	ご
kya	きゃ
kyi	きぃ
kyu	きゅ
kye	きぇ
kyo	きょ
qya	くゃ
qyu	くゅ
qyo	くょ
qwa	くぁ
qa	くぁ
kwa	くぁ
qwi	くぃ
qi	くぃ
qyi	くぃ
qwu	くぅ
qwe	くぇ
qe	くぇ
qye	くぇ
qwo	くぉ
qo	くぉ
gya	ぎゃ
gyi	ぎぃ
gyu	ぎゅ
gye	ぎぇ
gyo	ぎょ
gwa	ぐぁ
gwi	ぐぃ
gwu	ぐぅ
gwe	ぐぇ
gwo	ぐぉ
sa	さ
si	し
ci	し
shi	し
su	す
se	せ
ce	せ
so	そ
za	ざ
zi	じ
ji	じ
zu	ず
ze	ぜ
zo	ぞ
sya	しゃ
sha	しゃ
syi	しぃ
syu	しゅ
shu	しゅ
sye	しぇ
she	しぇ
syo	しょ
sho	しょ
swa	すぁ
swi	すぃ
swu	すぅ
swe	すぇ
swo	すぉ
zya	じゃ
ja	じゃ
jya	じゃ
zyi	じぃ
jyi	じぃ
zyu	じゅ
ju	じゅ
jyu	じゅ
zye	じぇ
je	じぇ
jye	じぇ
zyo	じょ
jo	じょ
jyo	じょ
ta	た
ti	ち
chi	ち
tu	つ
tsu	つ
te	て
to	と
ltu	っ
xtu	っ
ltsu	っ
da	だ
di	ぢ
du	づ
de	で
do	ど
tya	ちゃ
cha	ちゃ
cya	ちゃ
tyi	ちぃ
cyi	ちぃ
tyu	ちゅ
chu	ちゅ
cyu	ちゅ
tye	ちぇ
che	ちぇ
cye	ちぇ
tyo	ちょ
cho	ちょ
cyo	ちょ
tsa	つぁ
tsi	つぃ
tse	つぇ
tso	つぉ
tha	てゃ
thi	てぃ
thu	てゅ
the	てぇ
tho	てょ
twa	とぁ
twi	とぃ
twu	とぅ
twe	とぇ
two	とぉ
dya	ぢゃ
dyi	ぢぃ
dyu	ぢゅ
dye	ぢぇ
dyo	ぢょ
dha	でゃ
dhi	でぃ
dhu	でゅ
dhe	でぇ
dho	でょ
dwa	どぁ
dwi	どぃ
dwu	どぅ
dwe	どぇ
dwo	どぉ
na	な
ni	に
nu	ぬ
ne	ね
no	の
nya	にゃ
nyi	にぃ
nyu	にゅ
nye	にぇ
nyo	にょ
ha	は
hi	ひ
hu	ふ
fu	ふ
he	へ
ho	ほ
ba	ば
bi	び
bu	ぶ
be	べ
bo	ぼ
pa	ぱ
pi	ぴ
pu	ぷ
pe	ぺ
po	ぽ
hya	ひゃ
hyi	ひぃ
hyu	ひゅ
hye	ひぇ
hyo	ひょ
fya	ふゃ
fyu	ふゅ
fyo	ふょ
fwa	ふぁ
fa	ふぁ
fwi	ふぃ
fi	ふぃ
fyi	ふぃ
fwu	ふぅ
fwe	ふぇ
fe	ふぇ
fye	ふぇ
fwo	ふぉ
fo	ふぉ
bya	びゃ
byi	びぃ
byu	びゅ
bye	びぇ
byo	びょ
va	ヴぁ
vi	ヴぃ
vu	ヴ
ve	ヴぇ
vo	ヴぉ
vya	ヴゃ
vyi	ヴぃ
vyu	ヴゅ
vye	ヴぇ
vyo	ヴょ
ma	ま
mi	み
mu	む
me	め
mo	も
mya	みゃ
myi	みぃ
myu	みゅ
mye	みぇ
myo	みょ
ya	や
yu	ゆ
yo	よ
lya	ゃ
xya	ゃ
lyu	ゅ
xyu	ゅ
lyo	ょ
xyo	ょ
ra	ら
ri	り
ru	る
re	れ
ro	ろ
rya	りゃ
ryi	りぃ
ryu	りゅ
rye	りぇ
ryo	りょ
wa	わ
wo	を
n	ん
nn	ん
n'	ん
xn	ん
lwa	ゎ
xwa	ゎ
yyi	っい
wwu	っう
wwhu	っう
lla	っぁ
xxa	っぁ
lli	っぃ
xxi	っぃ
llyi	っぃ
xxyi	っぃ
llu	っぅ
xxu	っぅ
lle	っぇ
xxe	っぇ
llye	っぇ
xxye	っぇ
llo	っぉ
xxo	っぉ
yye	っいぇ
wwha	っわ
wwhi	っうぃ
wwi	っうぃ
wwhe	っうぇ
wwe	っうぇ
wwho	っうぉ
kka	っか
cca	っか
kki	っき
kku	っく
ccu	っく
qqu	っく
kke	っけ
kko	っこ
cco	っこ
llka	っヵ
xxka	っヵ
llke	っヶ
xxke	っヶ
gga	っが
ggi	っぎ
ggu	っぐ
gge	っげ
ggo	っご
kkya	っきゃ
kkyi	っきぃ
kkyu	っきゅ
kkye	っきぇ
kkyo	っきょ
qqya	っくゃ
qqyu	っくゅ
qqyo	っくょ
qqwa	っくぁ
qqa	っくぁ
kkwa	っくぁ
qqwi	っくぃ
qqi	っくぃ
qqyi	っくぃ
qqwu	っくぅ
qqwe	っくぇ
qqe	っくぇ
qqye	っくぇ
qqwo	っくぉ
qqo	っくぉ
ggya	っぎゃ
ggyi	っぎぃ
ggyu	っぎゅ
ggye	っぎぇ
ggyo	っぎょ
ggwa	っぐぁ
ggwi	っぐぃ
ggwu	っぐぅ
ggwe	っぐぇ
ggwo	っぐぉ
ssa	っさ
ssi	っし
cci	っし
sshi	っし
ssu	っす
sse	っせ
cce	っせ
sso	っそ
zza	っざ
zzi	っじ
jji	っじ
zzu	っず
zze	っぜ
zzo	っぞ
ssya	っしゃ
ssha	っしゃ
ssyi	っしぃ
ssyu	っしゅ
sshu	っしゅ
ssye	っしぇ
sshe	っしぇ
ssyo	っしょ
ssho	っしょ
sswa	っすぁ
sswi	っすぃ
sswu	っすぅ
sswe	っすぇ
sswo	っすぉ
zzya	っじゃ
jja	っじゃ
jjya	っじゃ
zzyi	っじぃ
jjyi	っじぃ
zzyu	っじゅ
jju	っじゅ
jjyu	っじゅ
zzye	っじぇ
jje	っじぇ
jjye	っじぇ
zzyo	っじょ
jjo	っじょ
jjyo	っじょ
tta	った
tti	っち
cchi	っち
ttu	っつ
ttsu	っつ
tte	って
tto	っと
lltu	っっ
xxtu	っっ
dda	っだ
ddi	っぢ
ddu	っづ
dde	っで
ddo	っど
ttya	っちゃ
ccha	っちゃ
ccya	っちゃ
ttyi	っちぃ
ccyi	っちぃ
ttyu	っちゅ
cchu	っちゅ
ccyu	っちゅ
ttye	っちぇ
cche	っちぇ
ccye	っちぇ
ttyo	っちょ
ccho	っちょ
ccyo	っちょ
ttsa	っつぁ
ttsi	っつぃ
ttse	っつぇ
ttso	っつぉ
ttha	ってゃ
tthi	ってぃ
tthu	ってゅ
tthe	ってぇ
ttho	ってょ
ttwa	っとぁ
ttwi	っとぃ
ttwu	っとぅ
ttwe	っとぇ
ttwo	っとぉ
ddya	っぢゃ
ddyi	っぢぃ
ddyu	っぢゅ
ddye	っぢぇ
ddyo	っぢょ
ddha	っでゃ
ddhi	っでぃ
ddhu	っでゅ
ddhe	っでぇ
ddho	っでょ
ddwa	っどぁ
ddwi	っどぃ
ddwu	っどぅ
ddwe	っどぇ
ddwo	っどぉ
hha	っは
hhi	っひ
hhu	っふ
ffu	っふ
hhe	っへ
hho	っほ
bba	っば
bbi	っび
bbu	っぶ
bbe	っべ
bbo	っぼ
ppa	っぱ
ppi	っぴ
ppu	っぷ
ppe	っぺ
ppo	っぽ
hhya	っひゃ
hhyi	っひぃ
hhyu	っひゅ
hhye	っひぇ
hhyo	っひょ
ffya	っふゃ
ffyu	っふゅ
ffyo	っふょ
ffwa	っふぁ
ffa	っふぁ
ffwi	っふぃ
ffi	っふぃ
ffyi	っふぃ
ffwu	っふぅ
ffwe	っふぇ
ffe	っふぇ
ffye	っふぇ
ffwo	っふぉ
ffo	っふぉ
bbya	っびゃ
bbyi	っびぃ
bbyu	っびゅ
bbye	っびぇ
bbyo	っびょ
vva	っヴぁ
vvi	っヴぃ
vvu	っヴ
vve	っヴぇ
vvo	っヴぉ
vvya	っヴゃ
vvyi	っヴぃ
vvyu	っヴゅ
vvye	っヴぇ
vvyo	っヴょ
mma	っま
mmi	っみ
mmu	っむ
mme	っめ
mmo	っも
mmya	っみゃ
mmyi	っみぃ
mmyu	っみゅ
mmye	っみぇ
mmyo	っみょ
yya	っや
yyu	っゆ
nb	ん	b
nc	ん	c
nd	ん	d
nf	ん	f
ng	ん	g
nh	ん	h
nj	ん	j
nk	ん	k
nl	ん	l
nm	ん	m
np	ん	p
nq	ん	q
nr	ん	r
ns	ん	s
nt	ん	t
nv	ん	v
nw	ん	w
nx	ん	x
nz	ん	z
n1	ん	1
n2	ん	2
n3	ん	3
n4	ん	4
n5	ん	5
n6	ん	6
n7	ん	7
n8	ん	8
n9	ん	9
n0	ん	0
n-	ん	-
n^	ん	^
n!	ん	!
n""	ん	""
n#	ん	#
n$	ん	$
n%	ん	%
n&	ん	&
n(	ん	(
n)	ん	)
n=	ん	=
n~	ん	~
n|	ん	|
n@	ん	@
n[	ん	[
n`	ん	`
n{{	ん	{{
n;	ん	;
n:	ん	:
n]	ん	]
n+	ん	+
n*	ん	*
n}}	ん	}}
n,	ん	,
n.	ん	.
n/	ん	/
n\	ん	\
n<	ん	<
n>	ん	>
n?	ん	?
n_	ん	_
";

		#endregion

		#region Esperanto

		string eoConversion = $@"{slConversion}
cx	ĉ
gx	ĝ
hx	ĥ
jx	ĵ
sx	ŝ
ux	ŭ
Ĉ	Cx
Ĝ	Gx
Ĥ	Hx
Ĵ	Jx
Ŝ	Sx
Ŭ	Ux
Ĉ	CX
Ĝ	GX
Ĥ	HX
Ĵ	JX
Ŝ	SX
Ŭ	UX
";

		#endregion

		#region Russian

		string rsConversion = @"A	А
B	Б
V	В
YA	Я
G	Г
D	Д
YE	Е
YO	Ё
ZH	Ж
Z	З
I	И
J	Й
Y	Й
K	К
L	Л
M	М
N	Н
O	О
P	П
R	Р
S	С
T	Т
U	У
F	Ф
H	Х
KH	Х
TS	Ц
CH	Ч
SH	Ш
SCH	Щ
OO	Ы
Y	Ы
'	Ь
""	Ь
E	Э
YU	Ю
-	—
-	–
X	×
";

		rsConversion = $@"{slConversion}
{rsConversion.ToLower()}
{rsConversion}";

		#endregion
		
		ReadConversionDatabase(slConversion, ConversionType.StandardLatin);
		ReadConversionDatabase(jpConversion, ConversionType.StandardHiragana);
		ReadConversionDatabase(eoConversion, ConversionType.StandardEsperanto);
		ReadConversionDatabase(rsConversion, ConversionType.StandardRussian);
	}

	public static void ReadConversionDatabase(string database, ConversionType type) {
		using StringReader reader = new StringReader(database);
		string             line;

		Dictionary<string, List<string>> conversionDict = new Dictionary<string, List<string>>();

		do {
			line = reader.ReadLine();

			//Only parse lines with actual content
			if (string.IsNullOrWhiteSpace(line))
				continue;

			string[] splitLine = line.Split("\t");

			string romaji   = splitLine[0];
			string hiragana = splitLine[1];

			if (splitLine.Length > 2) continue;

			if (conversionDict.TryGetValue(hiragana, out List<string> currentRomaji))
				currentRomaji.Add(romaji);
			else
				conversionDict.Add(
					hiragana,
					new List<string> {
						romaji
					}
				);
		} while (line != null);

		//Set the Japanese conversion to the one we just loaded from the default string
		Conversions[type] = conversionDict;
	}
}
