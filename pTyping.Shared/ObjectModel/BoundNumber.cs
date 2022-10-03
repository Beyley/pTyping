#nullable enable

using System.Globalization;
using Furball.Vixie.Helpers.Helpers;
using Newtonsoft.Json;

namespace pTyping.Shared.ObjectModel;

[JsonObject(MemberSerialization.OptIn)]
public class BoundNumber <T> where T : struct, IComparable, IConvertible, IEquatable<T>, IComparable<T> {
	public event EventHandler<T>? Changed;

	[JsonProperty(TypeNameHandling = TypeNameHandling.All)]
	private T _value;
	[JsonProperty(TypeNameHandling = TypeNameHandling.All)]
	private T _maxValue = (T)Convert.ChangeType(int.MaxValue, typeof(T));
	[JsonProperty(TypeNameHandling = TypeNameHandling.All)]
	private T _minValue = (T)Convert.ChangeType(0, typeof(T));
	[JsonProperty(TypeNameHandling = TypeNameHandling.All)]
	private T _precision = (T)Convert.ChangeType(0.01f, typeof(T));

	public T Value {
		get => this._value;
		set {
			if (this._value.Equals(value))
				return;

			this.UpdateValue(value);
		}
	}

	public T MaxValue {
		get => this._maxValue;
		set {
			if (this._maxValue.Equals(value))
				return;

			this._maxValue = value;

			this.UpdateValue(this._value);
		}
	}

	public T MinValue {
		get => this._minValue;
		set {
			if (this._minValue.Equals(value))
				return;

			this._minValue = value;

			this.UpdateValue(this._value);
		}
	}

	public T Precision {
		get => this._precision;
		set {
			if (this._precision.Equals(value))
				return;

			if (value.ToDouble(CultureInfo.InvariantCulture) <= 0)
				throw new ArgumentException("Precision must be greater than 0!");
			
			this._precision = value;
			
			this.UpdateValue(this._value);
		}
	}

	private void UpdateValue(T value) {
		T old = this._value;

		double valD = value.ToDouble(CultureInfo.InvariantCulture);

		this._value = ((T)Convert.ChangeType(
			Math.Round(valD / this.Precision.ToDouble(CultureInfo.InvariantCulture)) * this.Precision.ToDouble(CultureInfo.InvariantCulture), typeof(T), CultureInfo.InvariantCulture))
		   .Clamp(this._minValue, this._maxValue);

		if (!old.Equals(this._value))
			this.Changed?.Invoke(this, this._value);
	}

	public static implicit operator T(BoundNumber<T> v) {
		return v._value;
	}

	public override string ToString() {
		return this._value.ToString()!;
	}
}
