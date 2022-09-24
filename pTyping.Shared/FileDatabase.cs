using System.Collections.Concurrent;
using System.Diagnostics;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;

namespace pTyping.Shared;

public class FileDatabase {
	public const string DATA_PATH = "data";

	public static readonly string FileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DATA_PATH, "f");

	private static ConcurrentDictionary<string, WeakReference<byte[]>> _Cache;

	public FileDatabase() {
		//Ensures the folder exists
		if (!Directory.Exists(FileFolderPath))
			Directory.CreateDirectory(FileFolderPath);

		_Cache = new ConcurrentDictionary<string, WeakReference<byte[]>>();
	}

	[Pure, NotNull]
	private static string FolderForHash(string hash) {
		return hash[..4];
	}

	[Pure, NotNull]
	private static string PathForHash(string hash) {
		return Path.Combine(FileFolderPath, FolderForHash(hash), hash);
	}

	public async Task AddFile(byte[] file) {
		string hash = CryptoHelper.GetMd5(file);

		string path = PathForHash(hash);

		if (File.Exists(path))
			return;

		Directory.CreateDirectory(Path.Combine(FileFolderPath, FolderForHash(hash)));

		await using FileStream stream = File.OpenWrite(path);

		await stream.WriteAsync(file);

		_Cache[hash] = new WeakReference<byte[]>(file);
	}

	public byte[] GetFile(string hash) {
		string path = PathForHash(hash);

		if (!File.Exists(path))
			throw new FileNotFoundException(hash);

		using FileStream   stream = File.OpenRead(path);
		using BinaryReader reader = new(stream);

		byte[] arr = reader.ReadBytes((int)stream.Length);

		Debug.Assert(arr.Length == stream.Length, "arr.Length == stream.Length");

		if (_Cache.TryGetValue(hash, out WeakReference<byte[]> dataRef))
			if (dataRef.TryGetTarget(out byte[] refArr))
				return refArr;
			else
				_Cache.Remove(hash, out _);

		_Cache[hash] = new WeakReference<byte[]>(arr);

		return arr;
	}

	public async Task<byte[]> GetFileAsync(string hash) {
		string path = PathForHash(hash);

		if (!File.Exists(path))
			throw new FileNotFoundException(hash);

		await using FileStream stream = File.OpenRead(path);

		byte[] arr = new byte[stream.Length];

		int readBytes = await stream.ReadAsync(arr.AsMemory(0, (int)stream.Length));

		Debug.Assert(readBytes == stream.Length, "readBytes == stream.Length");

		if (_Cache.TryGetValue(hash, out WeakReference<byte[]> dataRef))
			if (dataRef.TryGetTarget(out arr))
				return arr;
			else
				_Cache.Remove(hash, out _);

		_Cache[hash] = new WeakReference<byte[]>(arr);

		return arr;
	}
}
