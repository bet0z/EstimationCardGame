using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai.common
{
    /**
    * An abstract class representing a move made by a player. 
    */
    public class Move
    {
        private Player player;
        private Card card;

        public Move(Player player, Card card)
        {
            this.player = player;
            this.card = card;
        }

        public Move(Move move)
        {
            this.player = new Player(move.player);
            this.card = new Card(move.card);
        }

        public String toString()
        {
            return "Pid:" + this.player.id + ", PCard:" + this.card.toString();
        }

        public Card GetCard()
        {
            return this.card;
        }

        public Player GetPlayer()
        {
            return this.player;
        }

        /**
         * Creates a deep copy of this move.
         * @return the newly created copy
         */
        public Move copy()
        {
            return new Move(this.player, this.card);
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
            if (GetType() != obj.GetType())
            {
                return false;
            }
            Move other = (Move)obj;

            return (this.player.id == other.player.id) && (this.card.equals(other.card));
        }

    }
}
