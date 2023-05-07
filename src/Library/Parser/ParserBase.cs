namespace Library.Parser;

public abstract class ParserBase<T>
    where T : notnull
{
    public abstract T? Parse(ref State state);

    /// <summary>
    /// Fill buffer with new data. If the buffer is full or the input is empty this return false.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>true if more data could be read</returns>
    protected bool ReadMoreData(ref State state)
    {
        // shift data back to start if the offset is more than halfway through
        if ((state.offset << 1) >= state.buffer.Length)
        {
            state.buffer[state.offset..].CopyTo(state.buffer);
            state.offset = 0;
        }
        // read more data
        if (state.fileFinished)
            return false;
        var moreRead = state.reader.ReadBlock(state.buffer[(state.read + state.offset)..]);
        state.read += moreRead;
        state.fileFinished = moreRead == 0;
        return !state.fileFinished;
    }

    /// <summary>
    /// Reads the line from the current position to the end. The end line
    /// terminator is also consumed but not returned as a token.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>the line from input or null if the file is finished</returns>
    protected string? ReadLine(ref State state)
    {
        var sb = new System.Text.StringBuilder();
        if (state.read == 0 && !ReadMoreData(ref state))
        {
            return null;
        }
        while (!state.EOF)
        {
            var index = state.Data.IndexOf('\n');
            if (index == -1)
            {
                if (state.read == 0)
                    break;
                sb.Append(state.Data);
                state.Shift(state.read);
                ReadMoreData(ref state);
                continue;
            }
            sb.Append(state.Data[.. index]);
            state.Shift(index + 1); // skip the newline terminator
            break;
        }
        if (sb.Length == 0)
            return string.Empty;
        if (sb[sb.Length - 1] == '\r')
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    /// <summary>
    /// Reads a positive or negative 32bit integer number. This uses any
    /// character which isn't a number as final delimiter.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>the number or null if no number was found or the file is finished</returns>
    protected int? ReadInt32(ref State state)
    {
        // ensure that there is enough chars read to fit the largest int
        if (state.read < 11)
        {
            ReadMoreData(ref state);
        }
        // read the int
        int value = 0;
        var data = state.Data;
        if (data.Length == 0)
            return null;
        var offset = 0;
        if (data[0] == '-')
            offset++;
        while (offset < data.Length && char.IsNumber(data[offset]))
        {
            value = value * 10 + (data[offset] - '0');
            offset++;
        }
        // convert number and setup state
        if (offset == 0)
            return null;
        if (data[0] == '-')
            value = -value;
        state.Shift(offset);
        return value;
    }

    /// <summary>
    /// Reads a double precision floating point number. This uses a whitespace
    /// character as a delimiter.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>the number or null if no number was found or the file is finished</returns>
    protected double? ReadDouble(ref State state)
    {
        // ensure there is enough characters to read
        if (state.read < 100)
        {
            ReadMoreData(ref state);
        }
        // read the double
        var data = state.Data;
        if (data.Length == 0)
            return null;
        var offset = 0;
        while (offset < data.Length && !char.IsWhiteSpace(data[offset]))
            offset++;
        // convert number and setup state
        if (offset == 0 || !double.TryParse(data[..offset], System.Globalization.CultureInfo.InvariantCulture, out double value))
            return null;
        state.Shift(offset);
        return value;
    }

    /// <summary>
    /// Reads the next characters until the next whitespace. This wont consume the whitespace.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>the found string or null if the state is at a whitespace or the file has been finished</returns>
    protected string? ReadKeyword(ref State state)
    {
        var sb = new System.Text.StringBuilder();
        if (state.read == 0 && !ReadMoreData(ref state))
        {
            return null;
        }
        while (!state.EOF)
        {
            var data = state.Data;
            if (data.Length == 0)
                break;
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (!char.IsLetterOrDigit(data[offset]) && data[offset] != '_')
                {
                    sb.Append(data[..offset]);
                    state.Shift(offset);
                    goto after_loop;
                }
            }
            sb.Append(data);
            state.Shift(data.Length);
            ReadMoreData(ref state);
        }
        after_loop:
        if (sb.Length == 0)
            return null;
        return sb.ToString();
    }

    /// <summary>
    /// Skip all characters until a newline terminator was found. This will also
    /// consume the newline terminator itself.
    /// </summary>
    /// <param name="state">the common reader state</param>
    protected void SkipUntilEndOfLine(ref State state)
    {
        if (state.read == 0 && !ReadMoreData(ref state))
            return;
        while (!state.EOF)
        {
            var index = state.Data.IndexOf('\n');
            if (index == -1)
            {
                if (state.read == 0)
                    return;
                state.Shift(state.read);
                ReadMoreData(ref state);
                continue;
            }
            state.Shift(index + 1); // skip the newline terminator
            return;
        }
    }

    /// <summary>
    /// Skip all whitespace characters (excluding newline terminator).
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <returns>true if any whitespace character was skipped</returns>
    protected bool SkipWhitespace(ref State state)
    {
        if (state.read == 0 && !ReadMoreData(ref state))
            return false;
        bool consumed = false;
        while (!state.EOF)
        {
            var data = state.Data;
            if (data.Length == 0)
            {
                if (!ReadMoreData(ref state))
                    return consumed;
                else continue;
            }
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == '\n' || !char.IsWhiteSpace(data[offset]))
                {
                    state.Shift(offset);
                    return consumed || offset > 0;
                }
            }
            state.Shift(data.Length);
            consumed = true;
        }
        return consumed;
    }

    /// <summary>
    /// Check if the keyword exists at the given position. This will also
    /// consume the keyword itself. This won't check any characters after the
    /// keyword.
    /// </summary>
    /// <param name="state">the common reader state</param>
    /// <param name="keyword">the keyword to look for</param>
    /// <returns>true if the keyword exists at the current position</returns>
    protected bool CheckKeyword(ref State state, ReadOnlySpan<char> keyword)
    {
        // check if enough characters are available
        if (state.read < keyword.Length && !ReadMoreData(ref state))
            return false;
        // check for equality
        if (!keyword.StartsWith(state.Data[..keyword.Length]))
            return false;
        state.Shift(keyword.Length);
        return true;
    }
}
