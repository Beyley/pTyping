namespace pTyping.Shared.ObjectModel;

public interface ICopyable <in T> {
	/// <summary>
	///     Copies the contents of one object into another, without rewriting the object itself
	/// </summary>
	/// <param name="into"></param>
	public void CopyInto(T into);
}
