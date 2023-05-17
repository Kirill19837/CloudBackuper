namespace DbBackuper.Core.Extensions;

public static class GlobalExtensions
{
    public static string FormatBytes(this long bytes, bool useUnit = true)
    {
        string[] Suffix = { " B", " kB", " MB", " GB", " TB" };
        double dblSByte = bytes;
        int i;
        for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }
        return $"{dblSByte:0.##}{(useUnit ? Suffix[i] : null)}";
    }
}
