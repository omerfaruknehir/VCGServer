using System.Threading;
using VCG_Library;
using VCG_Objects;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace VCG_Server
{
    public static class ProgramEntry
    {
        public static WebSocketServer wssv;

        public static Dictionary<int, Room> roomsWithkeys = new Dictionary<int, Room>();
        public static Dictionary<int, Room> publicRoomsWithkeys = new Dictionary<int, Room>();
        public static List<Room> rooms = new List<Room>();

        public static Dictionary<string, Player> playersByIDs = new Dictionary<string, Player>();

        public static void Main(string[] args)
        {
            try
            {
                wssv = new WebSocketServer("ws://127.0.0.1:99");
                wssv.AddWebSocketService<Program>("/VCG_Main");
                ServerLib.RoomList = ServerLib.ListOfRooms(20);
                wssv.Start();
                Debug.LogWarning("Server Started! Press \"CTRL+C\" to stop server.");
                Console.CancelKeyPress += (sender, eventArgs) => {
                    wssv.Stop();
                    Debug.LogWarning("Server Stopped!");
                    Environment.Exit(0);
                };
                while (wssv.IsListening)
                {
                    Console.ReadKey(true);
                }
            }
            catch (WebSocketException e)
            {
                Debug.LogError("WebSocketException: " + e);
            }
            finally
            {

            }
        }
    }
}