using Eto.Drawing;
using Color=Furball.Vixie.Backends.Shared.Color;

namespace pTyping.Engine;

public static class Helpers {
    public static Color RotateColor(Color color, float r) {
        ColorHSL temp = new(new Eto.Drawing.Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));

        temp.H = (temp.H + r) % 360;

        Eto.Drawing.Color temp2 = temp.ToColor();
        return new(temp2.Rb, temp2.Bb, temp2.Gb, temp2.Ab);
    }
}