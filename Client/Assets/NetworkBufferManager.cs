using System.Collections.Concurrent;

public class NetworkBufferManager
{
    private readonly ConcurrentQueue<byte[]> _packetQueue = new ConcurrentQueue<byte[]>();

    public void EnqueueBuffer(byte[] packet)
    {
        _packetQueue.Enqueue(packet);
    }

    public bool TryDequeue(out byte[] packet)
    {
        return _packetQueue.TryDequeue(out packet);
    }

    public void Clear()
    {
        while (_packetQueue.TryDequeue(out _)) { }
    }
}