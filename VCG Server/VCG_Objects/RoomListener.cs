using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp;

using VCG_Server;
using VCG_Library;

namespace VCG_Objects
{
    public class RoomListener : WebSocketBehavior, IDisposable
    {
        public Room room { get; private set; }

        public bool Disposed = false;

        public RoomListener(Room room)
        {
            this.room = room;
        }

        protected override void OnOpen()
        {
            if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                string playerName = Context.QueryString["name"];
                Player player = ProgramEntry.playersByIDs[sessionID];
                player.RoomSocket = Context.WebSocket;
                room.AddPlayer(player);
                Context.WebSocket.Send("ConnectionRoom<<" + player.RoomName);
                Context.WebSocket.Send(room.ListOfPlayers());
                room.ListPlayers();
            }
            else
            {
                Context.WebSocket.Close(CloseStatusCode.NoStatus, "Unauthorized");
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (this.Disposed)
                return;

            List<Player> checkPlayers = new List<Player>(room.Players);
            if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                Player player = room.players(sessionID);
                //player.RoomSocket.Close(e.Code, "ClientQuit");
                room.RemovePlayer(player);
                if (room.PlayerNum == 0)
                {
                    room.Dispose();
                    this.Dispose();
                }
                else
                {
                    room.ListPlayers();
                }
            }
            //Context.WebSocket.Close(e.Code, "ClientQuit");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (Context.CookieCollection["sessionID"] != null && Context.QueryString.Contains("name"))
            {
                string sessionID = Context.CookieCollection["sessionID"].Value;
                Player player = ProgramEntry.playersByIDs[sessionID];

                if (e.IsText && e.Data.Split("<<").Length == 2)
                {
                    string commandName = e.Data.Split("<<")[0];
                    string[] rawArgs = e.Data.Split("<<")[1].Split(",");
                    if (e.Data.Split("<<")[1] == "")
                        rawArgs = new string[0];

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

                    if (commandName == "ListPlayers")
                    {
                        Context.WebSocket.Send("Error<<StartRoomException:Sorry, This operation not available!");
                    }

                    if (commandName == "Quit")
                    {
                        Context.WebSocket.Close();
                    }

                    if (commandName == "UseCard")
                    {
                        if (room.PlayerWillPlay == player)
                        {
                            if (args.Length == 1 && args[0] is int)
                            {
                                if (args[0])
                                {
                                    room.LastPlayerCard = room.PlayerWillPlay.Deck[args[0]];
                                    room.PlayerWillPlay.Deck.RemoveAt(args[0]);
                                    Context.WebSocket.Send("UseCard<<" + args[0]);
                                    return;
                                }
                                Context.WebSocket.Send("Error<<UseCardException:Don't cheat!, I know you don't have this card");
                                return;
                            }
                            Context.WebSocket.Send("Error<<UseCardException:Sorry, I didn't understand. Try again please. (But I'd be happier if you didn't try again)");
                            return;
                        }
                        Context.WebSocket.Send("Error<<UseCardException:Don't be whine and wait your turn!");
                        return;
                    }

                    if (commandName == "StartRoom")
                    {
                        if (player == room.Admin)
                        {
                            if (room.PlayerNum > 1)
                            {
                                room.StartGame();
                                return;
                            }
                            else
                            {
                                Context.WebSocket.Send("Error<<StartRoomException:You can't start the room when room have less than 1 players!");
                                return;
                            }
                        }
                        else
                        {
                            Context.WebSocket.Send("Error<<StartRoomException:You can't start the room without begining admin!");
                            return;
                        }
                    }

                    else if (commandName == "KickPlayer")
                    {
                        if (player == room.Admin)
                        {
                            if (args[0] == room.Admin.RoomName)
                            {
                                Context.WebSocket.Send("Error<<KickPlayerException:YOU CAN'T KICK YOURSELF!");
                                return;
                            }
                            foreach (Player checkPlayer in room.Players)
                            {
                                if (checkPlayer.RoomName == args[0])
                                {
                                    if (checkPlayer.RoomSocket != null)
                                    {
                                        checkPlayer.RoomSocket.Close();
                                    }
                                    room.RemovePlayer(checkPlayer);
                                    Context.WebSocket.Send("KickPlayer<<Success");
                                    return;
                                }
                            }
                            Context.WebSocket.Send("Error<<KickPlayerException:(s)He is already gone!");
                            return;
                        }
                        else
                        {
                            Context.WebSocket.Send("Error<<KickPlayerException:You can't kick another player from the room without begining admin!");
                            return;
                        }
                    }
                }
            }
            else
            {
                Context.WebSocket.Close(CloseStatusCode.Abnormal, "Unauthorized");
            }
        }

        public void Dispose()
        {
            this.Disposed = true;
        }
    }
}
