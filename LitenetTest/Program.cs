using LiteNetLib;
using LiteNetLib.Utils;
using LitenetTestCommon;

EventBasedNetListener listener = new();
NetManager server = new(listener);
server.Start(9050 /* port */);

listener.ConnectionRequestEvent += request =>
{
    request.Data.Get(out CustomKey key);
    Console.WriteLine("Got connection request with key: {0} {1}", key.A, key.B);
    if (server.ConnectedPeersCount < 10 /* max connections */)
    {
        if (key.A == 1 && key.B == 2)
            request.Accept();
        else
            request.Reject();
    }
    else
        request.Reject();
};

listener.PeerConnectedEvent += peer =>
{
    Console.WriteLine("Got connection: {0}", peer);  // Show peer ip
    NetDataWriter writer = new NetDataWriter();         // Create writer class
    writer.Put("Hello client!");                        // Put some string
    peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
};

// public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
listener.PeerDisconnectedEvent += (fromPeer, disconnectInfo) =>
{
    Console.WriteLine("Peer disconnected: {0}, Reason: {1}", fromPeer, disconnectInfo.Reason);
    if (disconnectInfo.Reason == DisconnectReason.RemoteConnectionClose)
    {
        try
        {
            disconnectInfo.AdditionalData.Get(out CustomKey customKey);
            Console.WriteLine("Custom key: {0} {1}", customKey.A, customKey.B);
            if (disconnectInfo.AdditionalData.TryGetString(out string? message))
                Console.WriteLine("Message: {0}", message);
            disconnectInfo.AdditionalData.Recycle();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get CustomKey from AdditionalData: {0}", ex.Message);
        }
    }
};

listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
{
    Console.WriteLine("Got: {0} on channel: {1}", dataReader.GetString(100 /* max length of string */), channel);
    dataReader.Recycle();
};

while (!Console.KeyAvailable)
{
    server.PollEvents();
    Thread.Sleep(15);
}
server.Stop();