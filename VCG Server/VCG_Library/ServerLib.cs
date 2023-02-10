using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

using VCG_Objects;
using VCG_Server;

namespace VCG_Library
{
    public static class ServerLib
    {
        public static string RoomList;

        private static string sessionIDChars = "qwertypsdghjklzxcvbnm";
        public static Random random = new Random();

        public static string RandomSessionID()
        {
            while (true)
            {
                string res = "";
                for (int i = 0; i < 2; i++)
                {
                    for (int n = 0; n < 5; n++)
                    {
                        res += sessionIDChars[random.Next(sessionIDChars.Length)];
                    }
                    res += "-";
                }
                res = res.Remove(res.Length - 1);
                if (!ProgramEntry.playersByIDs.ContainsKey(res))
                {
                    return res;
                }
            }
        }

        public static string GetCookie(WebSocketContext Context, dynamic[] args)
        {
            if (Context.CookieCollection[args[0]] != null)
            {
                return ("GetCookie<<" + args[0] + "," + Context.CookieCollection[args[0]].Value);
            }
            else
            {
                return ("GetCookie<<Fail");
            }
        }

        public static string Quit(WebSocketContext Context, dynamic[] args)
        {
            if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
            {
                string playerID = Context.CookieCollection["sessionID"].Value;
                string playerName = Context.QueryString["name"];
                if (ProgramEntry.playersByIDs.ContainsKey(playerID) && args.Length == 1 && args[0] is string)
                {
                    if (args[0] == "Room")
                    {
                        return ServerLib.QuitRoom(Context);
                    }
                    else
                    {
                        ServerLib.QuitRoom(Context);
                        return "Quit<<Success";
                    }
                }
                else
                {
                    return "Quit<<Success(But you don't have any pass card! I caught you!)";
                }
            }
            else
            {
                Context.WebSocket.Close(CloseStatusCode.NoStatus, "ClientQuit, Unauthorized");
                return "Quit<<Success";
            }
        }

        public static string ListOfRooms(int max = 0)
        {
            string resp = "ListRooms<<";
            int i = 0;
            foreach (int roomKey in ProgramEntry.publicRoomsWithkeys.Keys)
            {
                Room room = ProgramEntry.publicRoomsWithkeys[roomKey];
                if (room is not null && ProgramEntry.roomsWithkeys.ContainsKey(room.Key))
                {
                    i++;
                    resp += room.Key + ":" + room.Name + ":" + room.PlayerNum + ":" + room.MaxPlayers + ",";
                    if (i == max)
                    {
                        break;
                    }
                }
            }
            return i == 0 ? resp : resp.Remove(resp.Length - 1);
        }

        public static string ListRooms(WebSocketContext Context, dynamic[] args)
        {
            if (args.Length == 1 && args[0] is int)
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                return ListOfRooms(args[0]);
            }
            return "Error<<ArgumentException:\"!!ATTENTION!! Worst arguments ever!\". Btw don't try hack to the server! (It's 4 the players, not hackers)";
        }

        public static string JoinRoom(WebSocketContext Context, dynamic[] args)
        {
            if (args.Length == 1 && args[0] is int)
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                int roomKey = args[0];
                if (!ProgramEntry.roomsWithkeys.ContainsKey(roomKey))
                {
                    return "Error<<A:Huge, Space";
                }
                Room room = ProgramEntry.roomsWithkeys[roomKey];
                Player player = ProgramEntry.playersByIDs[sessionID];
                int joinStatus = room.AddPlayer(player);

                if (joinStatus == 1)
                {
                    Context.CookieCollection.Add(new Cookie("room", room.Key.ToString()));
                    Context.CookieCollection.Add(new Cookie("sessionID", player.ID, "Room_" + room.Key));

                    return "ConnectService<<Room_" + room.Key;
                }
                else if (joinStatus == -1)
                    return "Error<<JoinRoomException:This room is brimful, get away before it explodes!" + room.Key;
                else if (joinStatus == -2)
                    return "Error<<JoinRoomException:Sorry man, you're late. The train has just left!" + room.Key;
                else if (joinStatus == -3)
                    return "Error<<JoinRoomException:You musn't try to join this room when already you in this room!" + room.Key;
                return "Error<<JoinRoomException(but unknown):I can't solve this error. Can you solve it?" + room.Key;
                //return resp.Remove(0, 1);
            }
            return "Error<<ArgumentException:\"!!ATTENTION!! Worst arguments ever!\". Btw don't try hack 2 the server!";
        }

        public static string QuitRoom(WebSocketContext Context)
        {
            if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                string playerName = Context.QueryString["name"];
                if (!ProgramEntry.playersByIDs.ContainsKey(sessionID) || ProgramEntry.playersByIDs[sessionID].Name != playerName)
                {
                    return "Error<<QuitRoomException<<Hey you! Gimme your pass card!";
                }
                Player player = ProgramEntry.playersByIDs[sessionID];
                if (player.Room is null)
                {
                    return "Error<<QuitRoomException:You're not joined this room yet! Please before join for quitting.";
                }
                if (player.Room.Players.Contains(player))
                {
                    return "Error<<QuitRoomException:An error was encountered! Please keep your cool and try reconnecting to the server.";
                }
                int quitStatus = player.Room.RemovePlayer(player);

                if (quitStatus == 1)
                {
                    return "QuitRoom<<Room_" + player.Room.Key;
                }
                else if (quitStatus == -1)
                    return "Error<<QuitRoomException:You're not joined this room yet! Please before join for quitting.";
                return "Error<<JoinRoomException(but unknown):I can't solve this error. Can you solve it?";
                //return resp.Remove(0, 1);
            }
            return "Error<<ArgumentException:\"!!ATTENTION!! Worst arguments ever!\". Btw don't try hack to the server!";
        }

        //Room createRoom(string roomName, int maxPlayer, bool isPublic)
        //{
        //    return new Room(roomName, maxPlayer, isPublic, Program.roomsWithkeys, Program.publicRoomsWithkeys);
        //}

        public static string CreateRoom(WebSocketContext Context, dynamic[] args)
        {
            if (args.Length == 3 && args[0] is string && args[1] is bool && args[2] is int)
            {
                string playerID = Context.CookieCollection["sessionID"].Value;
                string roomName = args[0];
                bool isPublic = args[1];
                int maxPlayer = args[2];

                Player player = ProgramEntry.playersByIDs[playerID];

                if (player.Room is not null)
                {
                    return "Error<<CreateRoomException:I caught you!, Please don't try spam the server with creating room!";
                }

                Room room = new Room(roomName, maxPlayer, isPublic);
                ProgramEntry.wssv.AddWebSocketService<RoomListener>("/Room_" + room.Key, () => new RoomListener(room));
                lock (ProgramEntry.roomsWithkeys)
                {
                    ProgramEntry.roomsWithkeys[room.Key] = room;
                }
                if (isPublic)
                {
                    lock (ProgramEntry.publicRoomsWithkeys)
                    {
                        ProgramEntry.publicRoomsWithkeys[room.Key] = room;
                        RoomList = ListOfRooms(20);
                    }
                }
                Debug.Log("Room created " + room + ". Total room number:" + ProgramEntry.roomsWithkeys.Count);
                lock (room)
                {
                    //player.Room = room;
                    room.AddPlayer(player);

                    Context.CookieCollection.Add(new Cookie("room", room.Key.ToString()));
                    Context.CookieCollection.Add(new Cookie("sessionID", player.ID, "Room_" + room.Key));

                    return "ConnectService<<Room_" + room.Key;
                }
            }
            return "Error<<ArgumentException:\"!!ATTENTION!! Worst arguments ever!\". Btw don't try hacking the server!";
        }
    }
}
