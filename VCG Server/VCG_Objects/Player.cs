using WebSocketSharp;

namespace VCG_Objects
{
    public class Player
    {
        public string Name { get; set; }
        public string roomName;
        public string RoomName { get => roomName is null ? Name : roomName; set => roomName = value; }
        public string ID { get; set; }
        public WebSocket Socket { get; set; }
        public List<Card> Deck { get; set; }
        public Room Room { get; set; }
        public WebSocket RoomSocket { get; set; }

        public Player(string name, string ID, WebSocket socket)
        {
            this.Name = name;
            this.ID = ID;
            this.Socket = socket;
            this.Deck = new List<Card>();
        }

        public override bool Equals(object? obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public static bool operator ==(Player a, Player b)
        {
            if (a is null && b is null) return true;
            else if (a is null || b is null) return false;
            else return a.Equals(b);
        }

        public static bool operator !=(Player a, Player b)
        {
            if (a is null && b is null) return false;
            else if (a is null || b is null) return true;
            else return !a.Equals(b);
        }

        public override string ToString()
        {
            return "Player{Name:" + Name + ",ID:" + ID + "}";
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
