using RoR2;
using WebSocketSharp;


namespace Aerolt.Social
{
    public static class WebSocketClient {
        
        public static string UsernameText;
        public static string UserCountText;
        public static string MessageText;

        public static string ip = "aerolt.lodington.dev";
        public static string port = "5000";
        
        public static readonly WebSocket Disconnect = new($"ws://{ip}:{port}/Disconnect");
        public static readonly WebSocket Message = new($"ws://{ip}:{port}/Message");
        public static readonly WebSocket UserCount = new($"ws://{ip}:{port}/UserCount");
        public static readonly WebSocket Usernames = new($"ws://{ip}:{port}/Usernames");

        public static string _username;
        static WebSocketClient()
        {
            Usernames.OnMessage += (sender, e) =>
                UsernameText = e.Data + "\n";
            UserCount.OnMessage += (sender, e) =>
                UserCountText = e.Data;
            Message.OnMessage += (sender, e) =>
                MessageText += e.Data + "\n"; // Retrieve input from all clients
        }
        
        public static void ConnectClient()
        { // Set name variable

            _username = RoR2Application.GetBestUserName();

            // Connect to Usernames SocketBehaviour
            Usernames.Connect();
            Usernames.Send(_username);

            UserCount.Connect();
            UserCount.Send(_username);

            // Connect to Message SocketBehaviour
            Message.Connect();
        }

        public static void DisconnectClient()
        {
            Disconnect.Connect();
            
            // Send username of client disconnecting
            Disconnect.Send(_username);

            // Close all sockets and exit application
            Message.Close();
            Usernames.Close();
            Disconnect.Close();
        }
    }
}