using LiteNetLib.Utils;

namespace LitenetTestCommon;
public struct CustomKey : INetSerializable
{
    public int A;
    public int B;
    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(A);
        writer.Put(B);
    }
    public void Deserialize(NetDataReader reader)
    {
        if (!reader.TryGetInt(out A) || !reader.TryGetInt(out B))
        {
            throw new InvalidOperationException("Failed to deserialize CustomKey.");
        }
    }
}