namespace pTyping.Shared;

public class ReaderWriterLockedObject <T> {
	public T Object;

	public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	public ReaderWriterLockedObject(T obj) {
		this.Object = obj;
	}
}
