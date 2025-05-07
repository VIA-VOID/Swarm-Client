public class SendBuffer : RingBuffer
{
    public SendBuffer(int bufferSize) : base(bufferSize)
    {
    }

    // 데이터 쓰기
    public bool Write(byte[] src, int offset, int size)
    {
        return Enqueue(src, offset, size);
    }

}