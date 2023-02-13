using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Server;

using VCG_Library;
using VCG_Server;
using WebSocketSharp.Net.WebSockets;
using Windows.Media.PlayTo;

namespace VCG_Objects
{
    public class Room : IDisposable
    {
        public string Name;
        public int Key;
        public int MaxPlayers;
        public List<Player> Players { get; private set; } = new List<Player>();
        public List<Player> PlayersWithGameOrder { get; private set; } = new List<Player>();
        public Player Admin { get => Players[0]; }
        public bool IsPublic;
        public bool IsStarted;
        public List<Card> UnusedCards;

        private Thread roomLoop;

        private Thread lobbyLoop;
        private Thread gameLoop;

        public Card LastPlayerCard { get; set; }
        public Player PlayerWillPlay { get; private set; }

        public List<Card> DiscardPile = new List<Card>();

        private protected bool Disposed { get; private set; } = false;

        public int PlayerNum
        {
            get => Players.Count;
        }

        public Player players(string sessionID)
        {
            lock (Players)
            {
                foreach (Player player in Players)
                {
                    if (player.ID == sessionID)
                    {
                        return player;
                    }
                }
                return null;
            }
        }

        int RandomKey()
        {
            int key = new Random().Next(100000, 999999);
            while (ProgramEntry.roomsWithkeys.ContainsKey(key))
            {
                key = new Random().Next(100000, 999999);
            }
            return key;
        }

        public Room(string name, int maxPlayers, bool isPublic)
        {
            Name = name;
            MaxPlayers = maxPlayers;
            IsPublic = isPublic;
            Key = RandomKey();

            roomLoop = new Thread(RoomLoop);
            roomLoop.Start();

            lobbyLoop = new Thread(LobbyLoop);
            lobbyLoop.Start();

            gameLoop = new Thread(GameLoop);
        }

        private void RoomLoop()
        {
            Thread.Sleep(2000);
            while (!Disposed)
            {
                Thread.Sleep(1000);

                if (Disposed)
                {
                    return;
                }
                if (PlayerNum == 0)
                {
                    this.Dispose();
                    return;
                }
            }
        }

        private void LobbyLoop()
        {
            //Player[] lastPlayers = Players.ToArray();
            //while (!IsStarted && !Disposed)
            //{
            //    if (lastPlayers != Players.ToArray())
            //    {
            //        ListPlayers();
            //        lastPlayers = Players.ToArray();
            //    }
            //    Thread.Sleep(1000);
            //}
        }

        private void GameLoop()
        {
            while (!Disposed && IsStarted)
            {
                if (Disposed)
                {
                    return;
                }
                int ind = 0;
                while (ind < PlayersWithGameOrder.Count)
                {
                    Player player = PlayersWithGameOrder[ind];
                    if (player.RoomSocket == null)
                    {
                        PlayersWithGameOrder.Remove(player);
                        if (PlayerNum <= 1)
                        {
                            this.Dispose();
                        }
                        continue;
                    }
                    ind++;
                    if (Disposed)
                    {
                        return;
                    }
                    PlayerWillPlay = player;

                    bool bl = false;

                    if (DiscardPile.Count != 0)
                    {
                        foreach (Card card in player.Deck)
                        {
                            if (card.CanPlayableOn(DiscardPile.Last()))
                            {
                                bl = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bl = true;
                    }

                    if (!bl)
                    {
                        var rand = Card.Random(UnusedCards);
                        player.Deck.Add(rand);
                        player.Socket.Send("SetCards<<" + ServerLib.DeckToString(player.Deck));
                        if (!rand.CanPlayableOn(DiscardPile.Last()))
                        {
                            player.Socket.Send("Pass<<Round");
                            continue;
                        }
                    }

                    player.Socket.Send("Play<<Round");
                    int i = 0;
                    while (LastPlayerCard == null && i < 100)
                    {
                        Thread.Sleep(100);
                        i++;
                    }

                    if (i >= 100)
                    {
                        player.Socket.Send("Pass<<Round");
                        continue;
                    }

                    //if (LastPlayerCard.Type == "Powered")
                    //{
                    //    player.RoomSocket.Send("SelectColor<<");
                    //}
                    //while (LastSelectedColor == null)
                    //{
                    //    Thread.Sleep(100);
                    //}

                    Debug.Log(player.Name + $" played card({LastPlayerCard.Type}, {LastPlayerCard.Figure})");
                    DiscardPile.Add(LastPlayerCard);
                    SendAll("CardPlayed<<" + LastPlayerCard);
                    LastPlayerCard = null;
                }
            }
        }

        public void StartGame()
        {
            IsStarted = true;
            if (IsPublic)
            {
                ProgramEntry.publicRoomsWithkeys.Remove(this.Key);
                ServerLib.RoomList = ServerLib.ListOfRooms(20);
            }

            Card[] cards = new Card[PlayerNum * 32];
            for (int p = 0; p < PlayerNum; p++)
            {
                for (int c = 0; c < 30; c++)
                {
                    cards[(p * 32) + c] = Card.RandomUnpowered();
                }
                cards[(p * 32) + 30] = Card.RandomPowered();
                cards[(p * 32) + 31] = Card.RandomPowered();
            }
            UnusedCards = cards.ToList();

            foreach (Player player in Players)
            {
                string str = "";
                lock (player.Deck)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        Card card = Card.Random(UnusedCards);
                        player.Deck.Add(card);
                        str += card + ",";
                    }
                }
                player.Socket.Send("SetCards<<" + str.Remove(str.Length - 1));
            }

            PlayersWithGameOrder = Players.Shuffle();

            gameLoop.Start();
            SendAll("StartRoom<<NOW!");
        }

        public void SendAll(string str)
        {
            foreach (Player player in Players)
            {
                if (player.RoomSocket != null && player.RoomSocket.IsAlive)
                {
                    player.RoomSocket.Send(str);
                }
            }
        }

        public void ListPlayers()
        {
            if (Disposed)
            {
                return;
            }
            lock (Players)
            {
                SendAll(ListOfPlayers());
            }
        }

        public string ListOfPlayers()
        {
            string str = "ListPlayers<<";
            foreach (var player in Players)
            {
                str += player.RoomName + ",";
            }
            return str.Remove(str.Length - 1);
        }

        public bool IsPlayerNameExist(Player player)
        {
            foreach (Player checkPlayer in Players)
            {
                if (checkPlayer.RoomName == player.RoomName)
                {
                    return true;
                }
            }
            return false;
        }

        public int AddPlayer(Player player)
        {
            if (IsStarted)
            {
                return -2;
            }
            else if (PlayerNum == MaxPlayers)
            {
                return -1;
            }
            else if (player.Room == this)
            {
                return -3;
            }
            else if (!(player.Room is null))
            {
                player.Room.RemovePlayer(player);
            }
            player.RoomName = player.Name;
            while (IsPlayerNameExist(player))
            {
                int Out;
                int len = player.RoomName.ParseEnd(out Out);
                if (len != 0)
                {
                    player.RoomName = player.RoomName.Remove(player.RoomName.Length - (len), len) + (Out + 1);
                }
                else
                {
                    player.RoomName += " 1";
                }
            }
            Players.Add(player);
            player.Room = this;
            ListPlayers();
            return 1;
        }

        public int RemovePlayer(Player player)
        {
            if (!Players.Contains(player))
                return -1;

            player.Room = null;
            player.RoomSocket = null;
            if (Players.Contains(player))
                Players.Remove(player);
            ListPlayers();
            return 1;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    Player player = Players[i];
                    if (player is not null)
                    {
                        player.Room = null;
                        player.RoomSocket.Close();
                    }
                }
            }

            ProgramEntry.wssv.RemoveWebSocketService("/Room_" + this.Key);

            this.Disposed = true;

            ProgramEntry.roomsWithkeys.Remove(this.Key);

            Debug.Log("Room removed " + this + ". Total room number:" + ProgramEntry.roomsWithkeys.Count);

            if (IsPublic)
            {
                ProgramEntry.publicRoomsWithkeys.Remove(this.Key);
                ServerLib.RoomList = ServerLib.ListOfRooms(20);
            }

            this.Key = 0;
            this.MaxPlayers = 0;

            this.IsPublic = false;
            this.IsStarted = false;

            this.Name = null;
            this.UnusedCards = null;
            this.roomLoop = null;
            this.lobbyLoop = null;
            this.gameLoop = null;

            this.Players = null;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (!(obj as Room is Room)) return false;
            return Key == ((Room)obj).Key;
        }

        public override string ToString()
        {
            return "Room{ Name:" + this.Name + ", Key:" + this.Key + ", isPublic:" + this.IsPublic + " }";
        }

        public override int GetHashCode()
        {
            return this.Key;
        }

        public static implicit operator string(Room room)
        {
            return room.ToString();
        }

        public static bool operator ==(Room a, object b) => a.Equals(b);
        public static bool operator !=(Room a, object b) => !a.Equals(b);

        public static bool operator ==(object a, Room b) => b.Equals(a);
        public static bool operator !=(object a, Room b) => !a.Equals(b);

        public static bool operator ==(Room a, Room b) => (a is null && b is null) ? true : (a is null || b is null) ? false : a.Equals(b);
        public static bool operator !=(Room a, Room b) => (a is null && b is null) ? false : (a is null || b is null) ? true : !a.Equals(b);

        public static Room operator +(Room room, Player player)
        {
            room.AddPlayer(player);
            return room;
        }
    }
}
