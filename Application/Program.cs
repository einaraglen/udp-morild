using System.Text;

class Program
{
    public static void Main()
    {
        UDPSocket server = new UDPSocket();
        server.Server("127.0.0.1", 8062);

        server.Callback = (byte[] bytes) =>
        {
            Console.WriteLine("RECV: {0}", Encoding.ASCII.GetString(bytes, 0, bytes.Length));
        };

        UDPSocket client = new UDPSocket();
        client.Client("127.0.0.1", 8062);
        client.Send("$SkipTravel,1");
        client.Send("$SkipTravel,2");

        Console.ReadKey();
        server.Dispose();
        client.Dispose();
    }
}
