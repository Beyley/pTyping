namespace pTyping.Shared.Mods.Attributes;

public class ModSettingAttribute : Attribute {
	public readonly string Name;
	public readonly string Tooltip;
	public readonly int    OrderPosition;
	public ModSettingAttribute(string name, string tooltip, int orderPosition) {
		this.Name          = name;
		this.Tooltip       = tooltip;
		this.OrderPosition = orderPosition;
	}
}
