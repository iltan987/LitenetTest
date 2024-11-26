using LiteNetLib;
using LiteNetLib.Utils;
using LitenetTestCommon;

EventBasedNetListener listener = new EventBasedNetListener();
NetManager client = new NetManager(listener);
client.Start();

NetDataWriter writer = new NetDataWriter();
writer.Put(new CustomKey { A = 1, B = 2 });

// Option 1: Connect with NetDataWriter
var peer = client.Connect("localhost" /* host ip or name */, 9050 /* port */, writer /* NetDataWriter */);

// Option 2: Connect with key string
//client.Connect("localhost" /* host ip or name */, 9050 /* port */, "some key string");

listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
{
    Console.WriteLine("Got: {0} on channel: {1}", dataReader.GetString(100 /* max length of string */), channel);
    dataReader.Recycle();
};

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    NetDataWriter writer = new NetDataWriter();
    //writer.Put(31);
    writer.Put(new CustomKey { A = 3, B = 4 });
    writer.Put("Bye server!");
    peer.Disconnect(writer);
    client.Stop();
    Environment.Exit(0);
};

while (true)
{
    client.PollEvents();
    if (Console.KeyAvailable)
    {
        string? message = Console.ReadLine();
        if (!string.IsNullOrEmpty(message))
        {
            NetDataWriter messageWriter = new NetDataWriter();
            messageWriter.Put(message);
            client.FirstPeer.Send(messageWriter, DeliveryMethod.ReliableOrdered);
        }
    }
    Thread.Sleep(15);
}