using System;
using WebSocketSharp;
using WebSocketSharp.Server;

using VCG_Objects;
using VCG_Library;
using WebSocketSharp.Net;
using System.Text;

namespace VCG_Server
{
    public class Program : WebSocketBehavior
    {
        public Dictionary<int, Room> roomsWithkeys { get => ProgramEntry.roomsWithkeys; set => ProgramEntry.roomsWithkeys = value; }
        public Dictionary<int, Room> publicRoomsWithkeys { get => ProgramEntry.publicRoomsWithkeys; set { SendAll(ServerLib.ListRooms(Context, new dynamic[] { 20 })); ProgramEntry.publicRoomsWithkeys = value; } }
        //public List<Room> rooms = new List<Room>();

        public Dictionary<string, Player> playersByIDs { get => ProgramEntry.playersByIDs; set => ProgramEntry.playersByIDs = value; }

        public void SendAll(string s)
        {
            Sessions.Broadcast(s);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            //e.Data

            if (e.IsText && e.Data.Split("<<").Length == 2)
            {
                string commandName = e.Data.Split("<<")[0];
                string[] rawArgs = e.Data.Split("<<")[1].Split(",");

                dynamic[] args = new dynamic[rawArgs.Length];

                int i = 0;
                foreach (string rawArg in rawArgs)
                {
                    if (rawArg == "true")
                    {
                        args[i] = true;
                    }
                    else if (rawArg == "false")
                    {
                        args[i] = false;
                    }
                    else
                    {
                        int outInt;
                        if (Int32.TryParse(rawArg, out outInt))
                        {
                            args[i] = outInt;
                        }
                        else
                        {
                            args[i] = rawArg;
                        }
                    }
                    i++;
                }

                if (commandName == "ListRooms")
                {
                    Context.WebSocket.Send(ServerLib.ListRooms(Context, args));
                }
                else if (commandName == "JoinRoom")
                {
                    Context.WebSocket.Send(ServerLib.JoinRoom(Context, args));
                }
                else if (commandName == "CreateRoom")
                {
                    Context.WebSocket.Send(ServerLib.CreateRoom(Context, args));
                }
                else if (commandName == "GetCookie")
                {
                    Context.WebSocket.Send(ServerLib.GetCookie(Context, args));
                }
                else if (commandName == "Quit")
                {
                    ServerLib.Quit(Context, args);
                }
                else if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name") && playersByIDs.ContainsKey(Context.CookieCollection["sessionID"].Value) && playersByIDs[Context.CookieCollection["sessionID"].Value].Room is not null)
                {
                    if (commandName == "")
                    {

                    }
                }
            }

            //Send(e.Data);
        }

        string RandomSessionID()
        {
            return ServerLib.RandomSessionID();
        }

        protected override void OnOpen()
        {
            if (/*Context.QueryString.Contains("ID &&*/ Context.QueryString.Contains("name"))
            {
                //string playerID = Context.QueryString["ID"];
                string playerName = Context.QueryString["name"];

                string randID = RandomSessionID();

                Context.CookieCollection.Add(new Cookie("sessionID", randID));
                string sessionID = Context.CookieCollection["sessionID"].Value;

                playersByIDs[sessionID] = new Player(playerName, sessionID, Context.WebSocket);

                Context.WebSocket.Send("Connection<<Success");
                Context.WebSocket.Send(ServerLib.RoomList);

                Debug.Log("Connection Accepted, IP: \"" + "127...." + "\", Session ID: \"" + sessionID + "\"; Total client number: " + Sessions.Count);
            }
            else
            {
                Context.WebSocket.Send("Connection<<Fail");
                Context.WebSocket.Close(CloseStatusCode.NoStatus, "Unauthorized");

                Debug.Log("Connection Refused (Unauthorized), IP: " + Context.UserEndPoint.Address);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (Context.WebSocket != null)
            {
                if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
                {
                    string sessionID = Context.CookieCollection["sessionID"].Value;
                    string playerName = Context.QueryString["name"];
                    if (playersByIDs.ContainsKey(sessionID) && playersByIDs[sessionID].Name == playerName)
                    {
                        Player player = playersByIDs[sessionID];
                        if (player.Room is not null)
                        {
                            player.Room.RemovePlayer(player);
                            player.Room = null;
                        }
                        if (player.RoomSocket is not null)
                        {
                            if (player.RoomSocket.IsAlive)
                                player.RoomSocket.Close();
                            player.RoomSocket = null;
                        }
                        playersByIDs.Remove(sessionID);
                        Debug.Log("Connection Closed (Player quit), SessionID: \"" + sessionID + "\"; Total client number: " + Sessions.Count);
                    }
                    else
                    {
                        Debug.Log("Connection Closed; SessionID: \"" + sessionID + "\"; Total client number: " + Sessions.Count);
                    }
                }
                else
                {
                    Debug.Log("Connection Closed; Total client number: " + Sessions.Count);
                }
            }
        }

        public Program()
        {
            //Room room1 = new Room("Deneme", 5, true, roomsWithkeys, publicRoomsWithkeys);
            //room1.Players.Append(new Player("Ömer", "keyke-ykeyk", null));
            //room1.StartGame();

            //return 0;
        }
    }
}