namespace VoxelSandbox;

/// <summary>Writes a file of the player's settings whole, beside its name and moved over it.</summary>
/// <remarks>
/// The text is written under the file's name and <c>.new</c>, its bytes on the disk before it is
/// closed, and moved over the file, so a run ended as it writes, a full disk or a machine stopped
/// after leaves the last file written whole.
/// </remarks>
internal static class WholeFile
{
    /// <summary>Writes text over a file, telling <paramref name="writing"/> the path each write goes to first, for a test to end the write there.</summary>
    public static void Write(string path, string text, Action<string>? writing = null)
    {
        var written = path + ".new";
        writing?.Invoke(written);
        using (var file = new FileStream(written, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            file.Write(System.Text.Encoding.UTF8.GetBytes(text));
            file.Flush(flushToDisk: true);
        }
        File.Move(written, path, overwrite: true);
    }
}
