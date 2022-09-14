using System.Diagnostics;
using Furball.Engine.Engine.Helpers;
using JetBrains.Annotations;

namespace pTyping.Shared;

public class FileDatabase {
    public const string DATA_PATH = "data";

    public static readonly string FileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DATA_PATH, "f");

    public FileDatabase() {
        //Ensures the folder exists
        if (!Directory.Exists(FileFolderPath))
            Directory.CreateDirectory(FileFolderPath);
    }

    [Pure, NotNull]
    private static string FolderForHash(string hash) => hash[..4];

    [Pure, NotNull]
    private static string PathForHash(string hash) => Path.Combine(FileFolderPath, FolderForHash(hash));

    public async Task AddFile(byte[] file) {
        string hash = CryptoHelper.GetMd5(file);

        string path = PathForHash(hash);

        if (File.Exists(path))
            return;

        await using FileStream stream = File.OpenWrite(path);

        await stream.WriteAsync(file);
    }

    public async Task<byte[]> GetFile(string hash) {
        string path = PathForHash(hash);

        if (!File.Exists(path))
            throw new FileNotFoundException(hash);

        await using FileStream stream = File.OpenRead(path);

        byte[] arr = new byte[stream.Length];

        int readBytes = await stream.ReadAsync(arr.AsMemory(0, (int)stream.Length));

        Debug.Assert(readBytes == stream.Length, "readBytes == stream.Length");

        return arr;
    }
}
