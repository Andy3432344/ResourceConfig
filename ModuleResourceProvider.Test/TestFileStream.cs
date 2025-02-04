namespace ModuleResourceProvider.Test;

public class TestFileStream : Stream
{
    private readonly List<byte> bytes = new();

    public TestFileStream(byte[] bytes)
    {
        this.bytes = bytes.ToList();
    }

    private int index = 0;

    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = false;
    public override bool CanWrite { get; } = false;
    public override long Length => bytes.Count;
    public override long Position { get; set; }

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        try
        {

            if (count + index > bytes.Count)
                count = bytes.Count - index;

            bytes.CopyTo(index, buffer, offset, count);
            index += count;
        }
        catch (Exception)
        {
            return 0;
        }
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return -1;
    }

    public override void SetLength(long value)
    {

    }

    public override void Write(byte[] buffer, int offset, int count)
    {
    }
}