using System;

public abstract class RingBuffer
{
    protected byte[] _buffer;
    protected int _bufferSize;
    protected int _readPos;
    protected int _writePos;

    public RingBuffer(int bufferSize)
    {
        _bufferSize = bufferSize;
        _buffer = new byte[bufferSize];
        _readPos = 0;
        _writePos = 0;
    }

    // 사용 중인 데이터 크기
    public int DataSize
    {
        get
        {
            if (_writePos >= _readPos)
            {
                return _writePos - _readPos;
            }
            else
            {
                return _bufferSize - _readPos + _writePos;
            }
        }
    }

    // 남은 공간 크기
    public int FreeSize
    {
        get { return _bufferSize - DataSize - 1; }
    }

    // 연속된 공간에 쓸 수 있는 최대 크기
    protected int DirectEnqueueSize
    {
        get
        {
            if (_readPos > _writePos)
            {
                return _readPos - _writePos - 1;
            }
            else
            {
                return (_readPos == 0) ? (_bufferSize - _writePos - 1) : (_bufferSize - _writePos);
            }
        }
    }

    // 연속된 공간에서 읽을 수 있는 최대 크기
    protected int DirectDequeueSize
    {
        get
        {
            if (_writePos >= _readPos)
            {
                return _writePos - _readPos;
            }
            else
            {
                return _bufferSize - _readPos;
            }
        }
    }

    // 읽기 위치 이동
    protected void MoveReadPos(int size)
    {
        _readPos = (_readPos + size) % _bufferSize;
        CleanPos();
    }

    // 쓰기 위치 이동
    protected void MoveWritePos(int size)
    {
        _writePos = (_writePos + size) % _bufferSize;
    }

    // 데이터 쓰기
    protected bool Enqueue(byte[] src, int offset, int size)
    {
        if (size > FreeSize)
        {
            return false;
        }

        int firstEnqueueSize = DirectEnqueueSize;
        if (size > firstEnqueueSize)
        {
            // 두 번에 나눠서 쓰기
            Buffer.BlockCopy(src, offset, _buffer, _writePos, firstEnqueueSize);
            Buffer.BlockCopy(src, offset + firstEnqueueSize, _buffer, 0, size - firstEnqueueSize);
        }
        else
        {
            // 한 번에 쓰기
            Buffer.BlockCopy(src, offset, _buffer, _writePos, size);
        }
        // 쓰기 위치 이동
        MoveWritePos(size);
        return true;
    }

    // 데이터 읽기
    protected bool Dequeue(byte[] dest, int offset, int size)
    {
        if (size > DataSize)
        {
            return false;
        }

        int firstDequeueSize = DirectDequeueSize;
        if (size > firstDequeueSize)
        {
            // 두 번에 나눠서 읽기
            Buffer.BlockCopy(_buffer, _readPos, dest, offset, firstDequeueSize);
            Buffer.BlockCopy(_buffer, 0, dest, offset + firstDequeueSize, size - firstDequeueSize);
        }
        else
        {
            // 한 번에 읽기
            Buffer.BlockCopy(_buffer, _readPos, dest, offset, size);
        }
        // 읽기 위치 이동
        MoveReadPos(size);
        return true;
    }

    // 데이터 읽기
    // Pos 이동 없음
    public bool Peek(byte[] dest, int offset, int size)
    {
        if (size > DataSize)
        {
            return false;
        }

        int firstPeekSize = DirectDequeueSize;
        if (size > firstPeekSize)
        {
            // 두 번에 나눠서 읽기
            Buffer.BlockCopy(_buffer, _readPos, dest, offset, firstPeekSize);
            Buffer.BlockCopy(_buffer, 0, dest, offset + firstPeekSize, size - firstPeekSize);
        }
        else
        {
            // 한 번에 읽기
            Buffer.BlockCopy(_buffer, _readPos, dest, offset, size);
        }

        return true;
    }

    // 읽기 / 쓰기 위치 정리
    private void CleanPos()
    {
        if (DataSize == 0)
        {
            _readPos = 0;
            _writePos = 0;
        }
    }

    // 읽기 세그먼트 얻기
    public ArraySegment<byte> GetReadSegment()
    {
        return new ArraySegment<byte>(_buffer, _readPos, DirectDequeueSize);
    }

    // 쓰기 세그먼트 얻기
    public ArraySegment<byte> GetWriteSegment()
    {
        return new ArraySegment<byte>(_buffer, _writePos, DirectEnqueueSize);
    }
}