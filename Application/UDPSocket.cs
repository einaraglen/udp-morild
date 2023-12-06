using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPSocket : IDisposable
{
    private Socket? socket;
    private const int bufSize = 8 * 1024;
    private State state = new State();
    private EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback? receiver = null;
    public Action<byte[]>? Callback { get; set; }

    public class State
    {
        public byte[] buffer = new byte[bufSize];
    }

    public void Server(string address, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        Receive();
    }

    public void Client(string address, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(IPAddress.Parse(address), port);
        Receive();
    }

    public void Send(string text)
    {
        byte[] data = Encoding.ASCII.GetBytes(text);
        socket!.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            State so = (State)ar.AsyncState!;
            int bytes = socket.EndSend(ar);
        }, state);
    }

    private void Receive()
    {
        socket!.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref endpoint, receiver = (ar) =>
        {
            try
            {
                State so = (State)ar.AsyncState!;
                int bytes = socket.EndReceiveFrom(ar, ref endpoint);
                socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref endpoint, receiver, so);
                Callback!.Invoke(so.buffer);
            }
            catch { }

        }, state);
    }

    public void Dispose()
    {
        socket!.Close();
    }
}