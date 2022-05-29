using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai.common
{
    public class Player
    {
        private int nId;
        private List<Card> lstHandInfo;
        private Stack<Card> lstDiscardPile;

        public BidValue bidValue;

        public List<Card> hand { 
            get { return lstHandInfo; } 
            set{ lstHandInfo = new List<Card>(); foreach(Card card in value) lstHandInfo.Add(new Card(card));}
        }
        
        public string HandInfo2String()
        {
            string strText = "";
            foreach (Card card in this.lstHandInfo)
                strText += card.toString() + "|";
            return strText;
        }

        public void addToHand(Card card)
        {
            this.lstHandInfo.Add(card);
        }

        public Stack<Card> discardPile { get { return lstDiscardPile; }}

        public int id { get { return nId; } }


        /// <summary>
        /// Constructor used to create a new player.
        /// </summary>
        /// <param name="id">the unique id of this player, integer 1 or 2</param>
        public Player(int id)
        {
            this.nId = id;
            this.lstHandInfo = new List<Card>();
            this.lstDiscardPile = new Stack<Card>();
            this.bidValue = null;
        }

       /// <summary>
       /// Constructor used to clone a player.
       /// </summary>
       /// <param name="player">the player to be cloned</param>
        public Player(Player player)
        {
            this.nId = player.id;
            this.lstHandInfo = new List<Card>();
            this.lstDiscardPile = new Stack<Card>();
            this.bidValue = player.bidValue;
            foreach (Card card in player.hand)
                this.lstHandInfo.Add(new Card(card));
            foreach (Card card in player.discardPile)
                this.lstDiscardPile.Push(new Card(card));
        }

        public void initPlayerInfo()
        {
            this.lstHandInfo = new List<Card>();
            this.lstDiscardPile = new Stack<Card>();
            this.bidValue = null;
        }

        public void discard(Card card)
        {
            int index = lstHandInfo.FindIndex(finder => finder.toString() == card.toString());
            if(index == -1)
            {
                Console.WriteLine("Discard Warning:: There are not " + card.toString() + " in UserID = " + id);
                return;
            }
            this.lstHandInfo.RemoveAt(index);
            this.lstDiscardPile.Push(card);
        }

        public void setHand(Card[] hand)
        {
            List<Card> clone = hand.ToList().ConvertAll(card => new Card(card));
            this.hand = clone.ToList();
        }


        /// <summary>
        /// Organizes the hand of this player. Suits are prioritized first, then the ranks.
        /// </summary>
        public void organizeHand()
        {
            for (int i = 0; i < this.lstHandInfo.Count(); i++)
            {
                for (int j = 0; j < this.lstHandInfo.Count(); j++)
                {
                    if ((int)this.lstHandInfo[i].getSuit() < (int)this.lstHandInfo[j].getSuit())
                    {
                        Card helper = new Card(this.lstHandInfo[i]);
                        this.lstHandInfo[i] = new Card(this.lstHandInfo[j]);
                        this.lstHandInfo[j] = helper;
                    }
                    else if (this.lstHandInfo[i].getSuit() == this.lstHandInfo[j].getSuit())
                    {
                        if (this.lstHandInfo[i].getRank() < this.lstHandInfo[j].getRank())
                        {
                            Card helper = new Card(this.lstHandInfo[i]);
                            this.lstHandInfo[i] = new Card(this.lstHandInfo[j]);
                            this.lstHandInfo[j] = helper;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Have you some cards in player hand
        /// </summary>
        /// <param name="suit">Suit type of target cards</param>
        /// <param name="values">values array of target cards</param>
        /// <returns>if the exist : true or not return false</returns>
        public bool ContainCard(Suit suit, int[] values)
        {
            List<Card> lstFinds = lstHandInfo.FindAll(card => card.getSuit() == suit);
            
            int nSameCount = 0;
            foreach (int cardVal in values)
            {
                int index = lstFinds.FindIndex(card => card.getRank() == cardVal);
                if (index != -1)
                    nSameCount++;
            }
            return nSameCount == values.Length;
        }
    }
}
