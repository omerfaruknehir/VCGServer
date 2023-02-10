using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using VCG_Library;

namespace VCG_Objects
{
    public class CardType
    {
        public string TypeName { get; private set; }
        public int FigureNumber { get; private set; }

        public CardType(string typeName, int figureNumber)
        {
            TypeName = typeName;
            FigureNumber = figureNumber;
        }

        public void AddToCards()
        {
            Card.CardTypes[TypeName] = this;
        }

        public Card CreateCard(int figureNumber)
        {
            if (figureNumber >= 0 && figureNumber <= FigureNumber)
            {
                return new Card(TypeName, FigureNumber);
            }
            throw new ArgumentException(String.Format("\"figureNumber\" parameter must be higher than {0} or equal to {0} and lower than {1}", 0, FigureNumber));
        }
    }

    public class Card
    {
        public string Type { get; set; }
        public int Figure { get; set; }

        public static readonly string[] Types = new string[] { "blue", "green", "orange", "yellow", "powered" };

        public static Dictionary<string, CardType> CardTypes  = new Dictionary<string, CardType>();

        public static readonly int PoweredFigures = 4;
        public static readonly int ColoredFigures = 13;

        public readonly static Random random = new Random();

        public Card(string type, int figure)
        {
            this.Type = type;
            this.Figure = figure;
        }

        public static Card Parse(string data)
        {
            var type = Reflectives.GetCard(data.Split(":")[0]);
            var constructor = type.GetConstructor(new Type[] { typeof(int) });
            Card result = constructor.Invoke(new object[] { 3 }) as Card;
            return result;
        }

        //public static Card Random()
        //{
        //    int typeNum = random.Next(0, Types.Length - 1);
        //    return new Card(Types[typeNum], typeNum == 4 ? random.Next(0, PoweredFigures + 1) : random.Next(0, ColoredFigures + 1));
        //}

        public static Card Random(List<Card> UnusedCards)
        {
            int index = random.Next(UnusedCards.Count);
            Card res = UnusedCards[index];
            UnusedCards.RemoveAt(index);
            return res;
        }

        public static Card RandomPowered()
        {
            var types = Reflectives.GetCards();
            foreach (var type in types)
            {
                var constructor = type.GetConstructor(new Type[] { typeof(int) });
                Card result = constructor.Invoke(new object[] { 3 }) as Card;
            }
            return new Card(Types[4], random.Next(0, PoweredFigures));
        }

        public static Card RandomUnpowered()
        {
            return new Card(Types[random.Next(0, Types.Length - 1)], random.Next(0, ColoredFigures));
        }

        public override string ToString()
        {
            return Type + ":" + Figure;
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public static implicit operator string(Card card)
        {
            return card.ToString();
        }
        public static implicit operator Card(string text)
        {
            return Parse(text);
        }
    }
}
