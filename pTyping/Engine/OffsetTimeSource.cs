using Furball.Engine.Engine.Timing;

namespace pTyping.Engine;

public class OffsetTimeSource : ITimeSource {
	private readonly ITimeSource _puppet;

	public double Offset;

	public OffsetTimeSource(ITimeSource puppet, double offset) {
		this._puppet = puppet;
		this.Offset  = offset;
	}

	public double GetCurrentTime() {
		return this._puppet.GetCurrentTime() - this.Offset;
	}
}
