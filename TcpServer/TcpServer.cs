namespace Core.TCPServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using tcpmvc.Migrations;
using tcpmvc.Models;

public class TcpServer
{
    private const int BufferSize = 1024;
    private Socket? serverSocket;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private static readonly ConcurrentDictionary<int, Socket> clients = new ConcurrentDictionary<int, Socket>();

    public async Task StartServer()
    {
        try
        {
            serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            
            serverSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            
            serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 13000));
            serverSocket.Listen(10);

            Console.WriteLine("TCP Server started on port 13000...");
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Socket error during initialization: {e.Message}");
            return;
        }

        while (!cts.Token.IsCancellationRequested)
        {
            Console.WriteLine("Waiting for a client...");
            Socket clientSocket = await serverSocket.AcceptAsync();
            int clientHashCode = clientSocket.GetHashCode();
            clients[clientHashCode] = clientSocket;
            Console.WriteLine($"Client connected with ID: {clientHashCode}");

            _ = HandleClientAsync(clientSocket);
        }
    }

    public void StopServer()
    {
        cts.Cancel();

        foreach (var client in clients.Values)
        {
            client.Close();
        }
        clients.Clear();

        if (serverSocket != null)
        {
            serverSocket.Close();
            serverSocket.Dispose();
            Console.WriteLine("TCP Server stopped.");
        }
    }

    private static async Task HandleClientAsync(Socket clientSocket)
    {
        int clientHashCode = clientSocket.GetHashCode();

        try
        {
            byte[] buffer = new byte[BufferSize];

            while (true)
            {
                int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0) break;

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                Device device = new Device
                {
                    Connection = clientHashCode,
                    LastMessage = receivedMessage,
                    LastMessageTime = DateTime.Now,
                    LastUpdated = DateTime.Now
                };
                using var dbContext = new DeviceDbContext();
                dbContext.Device.Add(device);
                dbContext.SaveChanges();

                Console.WriteLine($"Received from client {clientHashCode}: " + receivedMessage);
                
            }

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clients.TryRemove(clientHashCode, out _);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Socket error: {e.Message}");
            clients.TryRemove(clientHashCode, out _);
        }
    }
    public static void SendMessageToClient(int clientID, string message)
    {
        if (clients.TryGetValue(clientID, out Socket? clientSocket))
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            clientSocket.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
            Console.WriteLine($"Sent message to client {clientID}: {message}");
        }
        else
        {
            Console.WriteLine($"Client {clientID} not found.");
        }
    }

    public IEnumerable<int> GetAllClientIDs()
    {
        return clients.Keys;
    }   

}