using System;

public class RecvBuffer : RingBuffer
{
    public RecvBuffer(int bufferSize) : base(bufferSize)
    {
    }

    // 데이터 읽기
    public bool Read(byte[] dest, int offset, int size)
    {
        return Dequeue(dest, offset, size);
    }

    // 쓰기 완료
    public void OnWrite(int size)
    {
        if (size > 0)
        {
            MoveWritePos(size);
        }
    }

}
