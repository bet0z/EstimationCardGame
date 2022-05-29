using estimation.ai.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai.manager
{
    public class State
    {
        public static int DECK_SIZE = 52; // This Game uses a standard 52 card deck
        public static int MAX_HAND_SIZE = 13; // Players can have up to 13 cards in their hands after a turn ends
        public static int PLAYER_COUNT = 4; // This version is a 4 player game
        public static int MELD_SIZE = 4;

        private Card[] deck;
        private Player[] arrPlayers;
        private int nCurPlayerIndex;

        public BidValue leadBid;
        private Stack<Card> discardPile;

        private Dictionary<int, Card> melds; // Melds played this round
        private Dictionary<int, List<Suit>> dicNeverSuitInfo;
        private Dictionary<int, int> dicTrickStatus;
        /**
         * Used to start a fresh new game of Estimation.
         * @param currentPlayerId the id of the player who will start the game.
         */
        public State(int currentPlayerId)
        {
            nCurPlayerIndex = currentPlayerId;
            arrPlayers = new Player[PLAYER_COUNT];
            for (int nI = 0; nI < PLAYER_COUNT; nI++)
                arrPlayers[nI] = new Player(nI);

            this.discardPile = new Stack<Card>();
            this.dicNeverSuitInfo = new Dictionary<int, List<Suit>>();
            this.dicTrickStatus = new Dictionary<int, int>();

            this.melds = new Dictionary<int, Card>();

            leadBid = null;
            this.deal();
        }

        public State(Player[] arrPlayer, int currentPlayerId, Card[] deck, Card[] discardPile, Dictionary<int, Card> melds, Dictionary<int, List<Suit>> suitInfo, BidValue leadBid, Dictionary<int, int> trickStatus)
        {
            this.nCurPlayerIndex = currentPlayerId;


            this.deck = deck;
            this.arrPlayers = arrPlayer;

            this.discardPile = new Stack<Card>(discardPile);

            this.melds = new Dictionary<int, Card>();
            foreach (int nIndex in melds.Keys)
            {
                Card card = new Card(melds[nIndex]);
                this.melds.Add(nIndex, card);
            }

            this.dicNeverSuitInfo = new Dictionary<int, List<Suit>>();
            foreach (int index in suitInfo.Keys)
            {
                List<Suit> lstSuits = new List<Suit>();
                foreach(Suit suit in suitInfo[index])
                    lstSuits.Add(suit);
                this.dicNeverSuitInfo.Add(index, lstSuits);
            }

            this.dicTrickStatus = new Dictionary<int, int>();

            foreach(int index in trickStatus.Keys)
                this.dicTrickStatus.Add(index, trickStatus[index]);

            this.leadBid = leadBid;
        }

        public void InitCurrentPlayer()
        {
            foreach (Player player in arrPlayers)
                if (player.bidValue.type == BidType.CALL)
                {
                    nCurPlayerIndex = player.id;
                    break;
                }
        }
        public void InitCurrentPlayer(int index)
        {
            nCurPlayerIndex = index;
        }
        public void setDeck(Card[] deck)
        {
            List<Card> clone = deck.ToList().ConvertAll(card => new Card(card));
            this.deck = clone.ToArray();
        }

        public int GetTrickStatus(int index)
        {
            return dicTrickStatus[index];
        }

        public Player GetCurrentPlayer()
        {
            return arrPlayers[nCurPlayerIndex % PLAYER_COUNT];
        }

        public Player GetPlayerInfo(int nId)
        {
            return arrPlayers[nId];
        }

        public Stack<Card> GetDiscardPile()
        {
            return this.discardPile;
        }

        public void printHandState()
        {
            foreach (Player player in arrPlayers)
                Console.WriteLine((player.id) + " : " + player.HandInfo2String());
        }

        public bool IsBidFinish()
        {
            int fullBidCount = 0;

            foreach (Player player in arrPlayers)
                if (player.bidValue != null && player.bidValue.value >= 0)
                    fullBidCount++;
                else if (player.bidValue != null && player.bidValue.type == BidType.PASS)
                    fullBidCount++;
            return fullBidCount == arrPlayers.Length;
        }

        public void confirmBid(int userIndex)
        {
            if (arrPlayers[userIndex].bidValue.type == BidType.PASS)
            {
                BidValue bid = DecideBidValue(leadBid, userIndex);
                bid.type = BidType.PASS;
                arrPlayers[userIndex].bidValue = bid;
            }
            Console.WriteLine(userIndex + "'s Bid : " + arrPlayers[userIndex].bidValue.toString());
        }

        public Dictionary<int, Card> getMelds()
        {
            return this.melds;
        }

        /// <summary>
        /// Estimaion about win trick count.
        /// </summary>
        /// <param name="userIndex">User Index</param>
        public void questionBid(int userIndex)
        {
            BidValue bid = DecideBidValue(leadBid, userIndex);
            if (leadBid != null)
            {
                if (bid.value > leadBid.value)
                {
                    bid.type = BidType.CALL;
                    leadBid = bid;
                    for (int nI = 0; nI < 4; nI++)
                    {
                        if (arrPlayers[nI].bidValue!= null && arrPlayers[nI].bidValue.type == BidType.CALL)
                            arrPlayers[nI].bidValue = null;
                    }
                } else if (bid.value == leadBid.value)
                {
                    bid.type = BidType.WITH;
                }
                else
                {
                    bid.type = BidType.PASS;
                    bid.value = -1;
                }
            } else if (bid.value >= 4)
            {
                bid.type = BidType.CALL;
                leadBid = bid;
            }
            Console.WriteLine(userIndex + "'s Bid : " + bid.toString());
            arrPlayers[userIndex].bidValue = bid;
        }

        private void deal()
        {
            this.deck = shuffleDeck(createDeck());
            initializeHands();
        }

        /**
         * Creates a fresh 52-card deck (not shuffled)
         * @return a full deck with cards in order
         */
        public Card[] createDeck()
        {
            Card[] newDeck = new Card[DECK_SIZE];

            int i = 0;
            for (int nS = 0; nS < 4; nS++)
            {
                Suit suit = (Suit)nS;
                for (int rank = 1; rank <= 13; rank++)
                {
                    newDeck[i] = (new Card(suit, rank));
                    i++;
                }
            }

            return newDeck;
        }

        private Card[] shuffleDeck(Card[] deck)
        {
            Random random = new Random();
            for (int i = deck.Length - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                Card helper = new Card(deck[randomIndex]);
                deck[randomIndex] = deck[i];
                deck[i] = helper;
            }

            return deck;
        }
        // Deals cards to both players' hands and shuffles them
        private void initializeHands()
        {
            for (int nI = 0; nI < arrPlayers.Count(); nI++)
            {
                arrPlayers[nI].initPlayerInfo();
                for (int nJ = 0; nJ < MAX_HAND_SIZE; nJ++)
                {
                    arrPlayers[nI].addToHand(this.deck[nI * MAX_HAND_SIZE + nJ]);
                }
                arrPlayers[nI].organizeHand();
                dicNeverSuitInfo.Add(nI, new List<Suit>());
                dicTrickStatus.Add(nI,0);
            }
            leadBid = null;
        }

        /// <summary>
        /// Creates a "deep" Clone of this state (deep enough for the AI to consider them the same).
        /// </summary>
        /// <returns>the Cloned state</returns>
        public State CloneState()
        {
            List<Player> clonePlayerList = this.arrPlayers.ToList().ConvertAll(player => new Player(player));
            List<Card> cloneDeckList = this.deck.ToList().ConvertAll(card => new Card(card));
            List<Card> cloneDiscardPie = this.discardPile.ToList().ConvertAll(card => new Card(card));
            return new State(
                    clonePlayerList.ToArray(),
                    this.nCurPlayerIndex,
                    cloneDeckList.ToArray(),
                    cloneDiscardPie.ToArray(),
                    this.melds,
                    this.dicNeverSuitInfo,
                    this.leadBid,
                    this.dicTrickStatus
            );
        }

        /// <summary>
        /// Finds all possible melds the current player can do from their hand. Assumes that hand cards are in order!
        /// </summary>
        /// <returns>list of all possible melds for the current player</returns>
        public AIArrayList<Move> GetAvailableMoves()
        {
            AIArrayList<Move> poibleMoves = new AIArrayList<Move>();
            if (roundOver())
            {
                return poibleMoves;
            }
            Card[] posibleCards;
            Player currentPlayer = this.GetCurrentPlayer() ;
            List<Card> hand = currentPlayer.hand;
            Card leadCard = null;
            if (this.melds != null && this.melds.Count > 0)
            {
                leadCard = this.melds.First().Value;
            }

            if (leadCard == null)
            {
                posibleCards = hand.ToArray();
            }
            else
            {
                posibleCards = hand.FindAll(card => card.getSuit() == leadCard.getSuit()).ToArray();
                if(posibleCards.Count() == 0 && hand.Count() > 0)
                {
                    posibleCards = hand.ToArray();
                }
            }

            foreach (Card card in posibleCards)
            {
                Move poibleMove = new Move(this.GetCurrentPlayer(), card);
                poibleMoves.add(poibleMove);
            }
            return poibleMoves;
        }

        /// <summary>
        /// Does a move and changes this state directly.
        /// </summary>
        /// <param name="move">the move to be done</param>
        /// <param name="aiSelection">true if this method is being used by the AI's selection phase, otherwise false</param>
        public void doMove(Move move, bool aiSelection, bool printTrickWinner = false)
        {
            if(this.melds.Count > 0 && this.melds.First().Value.getSuit() != move.GetCard().getSuit())
            {
                int nIndex = this.dicNeverSuitInfo[move.GetPlayer().id].FindIndex(suit => suit == this.melds.First().Value.getSuit());
                if(nIndex == -1)
                    this.dicNeverSuitInfo[move.GetPlayer().id].Add(this.melds.First().Value.getSuit());
            }
            Card card = move.GetCard();
            this.melds.Add(move.GetPlayer().id, card);
            GetCurrentPlayer().discard(card);
            this.discardPile.Push(card);
            this.nCurPlayerIndex++;
            if (this.melds.Count == 4) {
                this.nCurPlayerIndex = GetTrickWinner();
                this.dicTrickStatus[nCurPlayerIndex] = this.dicTrickStatus[nCurPlayerIndex] + 1;
                this.melds = new Dictionary<int, Card>();
                if (printTrickWinner)
                    Console.WriteLine(move.toString() + "____________Trick Winner : " + this.nCurPlayerIndex);
            }
            else if (printTrickWinner)
            {
                Console.WriteLine(move.toString());
            }
            
        }

        /// <summary>
        /// If melds list is full, you must check trick so you can call this function and get winner index in current trick.
        /// </summary>
        /// <returns></returns>
        public int GetTrickWinner()
        {
            Card firstCard = this.melds.First().Value;

            int nLeadCardUser = -1;
            int nNormalCardUser = this.melds.First().Key;

            foreach (int nUserId in this.melds.Keys) { 
                if(this.melds[nUserId].getSuit() == this.leadBid.suit)
                {
                    if(nLeadCardUser == -1 || (nLeadCardUser != -1 && this.melds[nLeadCardUser].getRank() < this.melds[nUserId].getRank()))
                    {
                        nLeadCardUser = nUserId;
                    }
                }else if(this.melds[nUserId].getSuit() == firstCard.getSuit())
                {
                    if(this.melds[nNormalCardUser].getRank() < this.melds[nUserId].getRank())
                    {
                        nNormalCardUser = nUserId;
                    }
                }
            }
            if (nLeadCardUser != -1)
                return nLeadCardUser;
            else
                return nNormalCardUser;
        }

        /// <summary>
        /// Gets the round result from the winner's perspective. The result is linearly scaled between 0.5 and 1.0 so that 0.5 means a draw and 1.0 means a win by 100 points. This value is used by the AI.
        /// </summary>
        /// <returns>the win result between 0.505 (inclusive) and 1.0 (inclusive)</returns>
        public Dictionary<int, int> GetTrickResult()
        {
            return dicTrickStatus;
        }

        /// <summary>
        /// Calculates the points for the winner of this round. Must be called during the winning player's turn.
        /// </summary>
        /// <returns>the sum of the points awarded from this round</returns>
        public int calculateRoundPoints()
        {
            int sum = 0;
            
            return sum;
        }

        /// <summary>
        /// Clones the current state, then randomizes all hidden information of the state from the AI's perspective. Applies this randomization to the Cloned state.
        /// </summary>
        /// <returns>the newly Cloned and randomized state</returns>
        public State CloneAndRandomizeState()
        {
            State Clone = CloneState();

            Card[] knownCards = new Card[DECK_SIZE];
            int knownIndex = 0;

            // Add own hand cards to known cards
            foreach (Card card in Clone.GetCurrentPlayer().hand)
            {
                knownCards[knownIndex] = card;
                knownIndex++;
            }

            // Add discard pile to known cards
            foreach (Card card in Clone.GetDiscardPile())
            {
                knownCards[knownIndex] = card;
                knownIndex++;
            }

            int unknownIndex = 0;
            Card[] unknownCards = new Card[DECK_SIZE - knownIndex];
            Card[] fullDeck = createDeck();

            // Add every card except the known cards to unknown cards
            for (int nD = 0; nD < DECK_SIZE; nD++)
            {
                bool known = false;

                for (int j = 0; j < knownIndex; j++)
                {
                    if (fullDeck[nD].equals(knownCards[j]))
                    {
                        known = true;
                        break;
                    }
                }

                if (!known)
                {
                    unknownCards[unknownIndex] = fullDeck[nD];
                    unknownIndex++;
                }
            }

            //-------------------------------------------------------------------------
            unknownCards = shuffleDeck(unknownCards);
            Clone.setDeck(unknownCards);
            List<Card> lstUnknowCardWork = new List<Card>(unknownCards);
            for (int nI = 0; nI < arrPlayers.Count(); nI++)
            {
                if(nI != this.GetCurrentPlayer().id)
                {
                    int nHandIndex = 0;
                    int nUnknownIndex = 0;
                    int restHandSize = Clone.GetPlayerInfo(nI).hand.Count();

                    Card[] opponentHandGuess = new Card[restHandSize];
                    List<Suit> lstInposibleSuit = this.dicNeverSuitInfo[nI];
                    while (nHandIndex != restHandSize)
                    {
                        if (nUnknownIndex >= lstUnknowCardWork.Count)
                            break;
                        //If there are no in inposible suit list
                        //if (lstInposibleSuit.FindIndex(suit => suit == lstUnknowCardWork[nUnknownIndex].getSuit()) == -1)
                        {
                            opponentHandGuess[nHandIndex] = lstUnknowCardWork[nUnknownIndex];
                            nHandIndex++;
                        }
                        nUnknownIndex++;
                    }
                    foreach(Card card in opponentHandGuess)
                    {
                        lstUnknowCardWork.Remove(card);
                    }
                    Clone.GetPlayerInfo(nI).setHand(opponentHandGuess);
                }  
            }
            return Clone;
        }


        public bool roundOver()
        {
            return GetCurrentPlayer().hand.Count() == 0;
        }

        public BidValue DecideBidValue(BidValue leadBid, int userIndex)
        {
            BidValue bidValue = new BidValue();
            Player tarPalyer = arrPlayers[userIndex];
            List<Card> lstHandInfo = tarPalyer.hand;
            Suit selSuit = Suit.NONE;
            if (leadBid != null)
            {
                selSuit = leadBid.suit;
            }
            else
            {
                int maxCounter = 0;
                for (int nI = 0; nI < 4; nI++)
                {
                    int count = lstHandInfo.FindAll(card => card.getSuit() == (Suit)nI).Count;
                    if (maxCounter < count) {
                        selSuit = (Suit)nI;
                        maxCounter = count;
                    }
                    else if(maxCounter == count)
                    {
                        int pastSum = lstHandInfo.FindAll(card => card.getSuit() == selSuit).Sum(card => card.getRank());
                        int curSum = lstHandInfo.FindAll(card => card.getSuit() == (Suit)nI).Sum(card => card.getRank());
                        if (pastSum < curSum) {
                            selSuit = (Suit)nI;
                            maxCounter = count;
                        }
                    }
                }
            }
            bidValue.suit = selSuit;
            int nBidValue = 0;
            for (int nI = 0; nI < 4; nI++)
            {
                Suit suit = (Suit)nI;
                List<Card> lstCardValues = lstHandInfo.FindAll(card => card.getSuit() == suit);
                float result = 0;

                if (tarPalyer.ContainCard(suit, new int[] { 9, 10, 11, 12 }))
                    result += 4;
                else if (tarPalyer.ContainCard(suit, new int[] { 9, 10, 11 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 3;
                    else if (lstCardValues.Count >= 3)
                        result += 2;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 9, 10, 12 }))
                {
                    if (lstCardValues.Count >= 4)
                        result += 3;
                    else if (lstCardValues.Count >= 3)
                        result += 2;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 10, 11, 12 }))
                {
                    result += 3;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 9, 10 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 2;
                    else if (lstCardValues.Count >= 4)
                        result += 1;
                    else if (lstCardValues.Count >= 3)
                        result += 0.5f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 9, 11 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 2;
                    else if (lstCardValues.Count >= 4)
                        result += 1;
                    else if (lstCardValues.Count >= 2)
                        result += 0.5f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 9, 12 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 3;
                    else if (lstCardValues.Count >= 4)
                        result += 2;
                    else if (lstCardValues.Count >= 2)
                        result += 1;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 10, 11 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 3;
                    else if (lstCardValues.Count >= 4)
                        result += 2;
                    else if (lstCardValues.Count >= 2)
                        result += 0.7f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 10, 12 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 3;
                    else if (lstCardValues.Count >= 4)
                        result += 2;
                    else if (lstCardValues.Count >= 2)
                        result += 1;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 11, 12 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 3;
                    else if (lstCardValues.Count >= 2)
                        result += 2;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 9 }))
                {
                    if (lstCardValues.Count >= 5)
                        result += 1;
                    else if (lstCardValues.Count >= 4)
                        result += 0.5f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 10 }))
                {
                    if (lstCardValues.Count >= 4)
                        result += 1;
                    else if (lstCardValues.Count >= 3)
                        result += 0.5f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 11 }))
                {
                    if (lstCardValues.Count >= 3)
                        result += 1;
                    else if (lstCardValues.Count >= 2)
                        result += 0.5f;
                }
                else if (tarPalyer.ContainCard(suit, new int[] { 12 }))
                {
                    result += 1;
                }
                nBidValue += (int)result;
            }
            int nLeaderCount = lstHandInfo.FindAll(card => card.getSuit() == selSuit).Count;
            int diffCount = nLeaderCount - (int)(Math.Ceiling((double)State.MAX_HAND_SIZE - nLeaderCount) / 3);
            if (diffCount > 2)
                nBidValue += diffCount - 1;

            bidValue.value = nBidValue;
            return bidValue;
        }
    }
}
