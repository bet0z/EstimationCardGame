using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai.common
{
    /**
    * Represents a card. Every card is unique in the game.
    */
    public class Card
    {
        private Suit suit;
        private int rank;

        public Card(Card clone)
        {
            this.suit = clone.suit;
            this.rank = clone.rank;
        }

        public Card(Suit suit, int rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        public Suit getSuit()
        {
            return this.suit;
        }

        public int getRank()
        {
            return this.rank;
        }

        public String toString()
        {
            return  this.suit + "^" + this.rank;
        }

        public int hashCode()
        {
            int hash = 7;
            hash = 13 * hash + (int)this.suit;
            hash = 13 * hash + this.rank;
            return hash;
        }

        public bool equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            Card other = (Card)obj;
            if (this.rank != other.rank)
            {
                return false;
            }
            if (this.suit != other.suit)
            {
                return false;
            }
            return true;
        }
    }
}
