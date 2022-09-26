using System;
using System.Numerics;
using System.Text;
using Furball.Vixie.Backends.Shared;

namespace pTyping.Graphics.UiMaker;

public class IndentedStringBuilder {
	private readonly StringBuilder StringBuilder;

	public int Indentation;

	public IndentedStringBuilder() {
		this.StringBuilder = new StringBuilder();
	}

	public string GetIndentation() {
		string str = "";

		for (int i = 0; i < this.Indentation; i++)
			str += "    ";

		return str;
	}

	public void AppendLine(string str = "") {
		this.StringBuilder.AppendLine(this.GetIndentation() + str);
	}

	public override string ToString() {
		return this.StringBuilder.ToString();
	}
}

public static class UiMakerCodeGen {
	private static string ElementTypeToClassName(UiMakerElementType type) {
		return type switch {
			UiMakerElementType.Text    => "TextDrawable",
			UiMakerElementType.Texture => "TexturedDrawable",
			UiMakerElementType.Button  => "DrawableButton",
			_                          => throw new ArgumentOutOfRangeException(nameof (type), type, null)
		};
	}

	private static void PrintFieldsForElements(IndentedStringBuilder builder, UiMakerElementContainer container) {
		foreach (UiMakerElement element in container.Elements)
			builder.AppendLine($"public {ElementTypeToClassName(element.Type)} {element.Identifier};");
	}

	private static string FormatVector2(Vector2 vector) {
		return $"new Vector2({FormatFloat(vector.X)}, {FormatFloat(vector.Y)})";
	}
	private static string FormatColor(Color color) {
		return $"new Color({FormatFloat(color.Rf)}, {FormatFloat(color.Gf)}, {FormatFloat(color.Bf)}, {FormatFloat(color.Af)})";
	}
	private static string FormatFloat(float f) {
		return $"{f:0.############}f";
	}

	private static void PrintConstructor(IndentedStringBuilder builder, UiMakerElementContainer container) {
		builder.AppendLine($"public {container.Name}() {{");
		builder.Indentation++;

		foreach (UiMakerElement element in container.Elements) {
			string args = element.Type switch {
				UiMakerElementType.Text    => $"{FormatVector2(element.Position)}, pTyping.pTypingGame.JapaneseFontStroked, \"{element.Text}\", {element.FontSize}",
				UiMakerElementType.Texture => $"ContentManager.LoadTextureFromFileCached(\"{element.Texture}\"), {FormatVector2(element.Position)}",
				UiMakerElementType.Button =>
					$"{FormatVector2(element.Position)}, pTypingGame.JapaneseFontStroked, {element.FontSize}, \"{element.Text}\", {FormatColor(element.ButtonColor)}, {FormatColor(element.Color)}, {FormatColor(element.ButtonOutlineColor)}, {FormatVector2(element.ButtonSize)}",
				_ => throw new Exception("Unknown element type!")
			};

			builder.AppendLine($"this.Drawables.Add(this.{element.Identifier} = new({args}));");
			builder.AppendLine($"this.{element.Identifier}.Rotation       = {FormatFloat(element.Rotation)};");
			builder.AppendLine($"this.{element.Identifier}.Depth          = {FormatFloat(element.Depth)};");
			builder.AppendLine($"this.{element.Identifier}.OriginType     = OriginType.{element.OriginType};");
			if (element.Type != UiMakerElementType.Button) //the button constructor handles this
				builder.AppendLine($"this.{element.Identifier}.ColorOverride  = {FormatColor(element.Color)};");
			builder.AppendLine($"this.{element.Identifier}.Scale          = {FormatVector2(element.Scale)};");
			builder.AppendLine($"this.{element.Identifier}.RotationOrigin = {FormatVector2(element.RotationOrigin)};");

			builder.AppendLine();

			if (element.OnClickFuncName?.Trim().Length != 0)
				builder.AppendLine($"this.{element.Identifier}.OnClick     += this.{element.OnClickFuncName}");
			if (element.OnClickUpFuncName?.Trim().Length != 0)
				builder.AppendLine($"this.{element.Identifier}.OnClickUp   += this.{element.OnClickUpFuncName}");
			if (element.OnHoverFuncName?.Trim().Length != 0)
				builder.AppendLine($"this.{element.Identifier}.OnHover     += this.{element.OnHoverFuncName}");
			if (element.OnHoverLostFuncName?.Trim().Length != 0)
				builder.AppendLine($"this.{element.Identifier}.OnHoverLost += this.{element.OnHoverLostFuncName}");
		}

		builder.Indentation--;
		builder.AppendLine("}");
	}

	public static string GenerateClass(UiMakerElementContainer container) {
		IndentedStringBuilder builder = new IndentedStringBuilder();

		builder.AppendLine("using System.Numerics;");
		builder.AppendLine("using Furball.Vixie.Backends.Shared;");
		builder.AppendLine("using Furball.Engine.Engine.Graphics;");
		builder.AppendLine("using Furball.Engine.Engine.Graphics.Drawables;");
		builder.AppendLine("using Furball.Engine.Engine.Graphics.Drawables.UiElements;");
		builder.AppendLine();

		builder.AppendLine("namespace pTyping;");
		builder.AppendLine();

		builder.AppendLine($"public partial class {container.Name} : CompositeDrawable {{");
		builder.Indentation++;

		{
			PrintFieldsForElements(builder, container);
			builder.AppendLine();

			PrintConstructor(builder, container);
		}

		builder.Indentation--;
		builder.AppendLine("}");

		return builder.ToString();
	}
}
