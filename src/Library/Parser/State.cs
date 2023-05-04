namespace Library.Parser;

public ref struct State
{
    public TextReader reader;

    public Span<char> buffer;

    public int read;

    public int offset;

    /// <summary>
    /// This flag is set to true if the reader can no longer provide new characters
    /// </summary>
    public bool fileFinished;

    public readonly ReadOnlySpan<char> Data => buffer[offset .. (offset + read)];

    public readonly bool EOF => read == 0 && fileFinished;

    public long TotalRead { get; private set; }

    public long TotalLine { get; private set; }

    public long LastLineStart { get; private set; }

    public void Shift(int length)
    {
        ShiftNewLine(length);
        offset += length;
        read -= length;
        TotalRead += length;
    }

    private void ShiftNewLine(int length)
    {
        var data = Data[..length];
        while (data.Length > 0)
        {
            var index = data.IndexOf('\n');
            if (index == -1)
                return;
            TotalLine++;
            data = data[(index + 1)..];
            LastLineStart = TotalRead + length - data.Length;
        }
    }
}
