using estimation.ai.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai
{
    /**
    * Represents a node in the search tree.
    */
    public class Node
    {
        private Node parent; // the parent of this node, null for root
        private AIArrayList<Node> children; // the child nodes of this node (one for every possible move from this node)

        private Move move; // move used to get to this node, null for root
        private Player player; // player who is doing the move to get to this node, the starting player for root
        private double totalScore; // sum of all simulation results from this node
        private int visits; // times this node has been selected
        private int considerations; // times this node has been considered for selection

        public Node(Node parent, Move move, Player player)
        {
            this.parent = parent;
            this.children = new AIArrayList<Node>();

            this.move = move;
            this.player = player;
            this.totalScore = 0.0;
            this.visits = 0;
            this.considerations = 1;
        }

        public Node getParent()
        {
            return this.parent;
        }

        public void setParent(Node parent)
        {
            this.parent = parent;
        }

        public AIArrayList<Node> getChildren()
        {
            return this.children;
        }

        public void setChildren(AIArrayList<Node> children)
        {
            this.children = children;
        }

        public Move getMove()
        {
            return this.move;
        }

        public void setMove(Move move)
        {
            this.move = move;
        }

        public Player getPlayer()
        {
            return this.player;
        }

        public void setPlayer(Player player)
        {
            this.player = player;
        }

        public double getTotalScore()
        {
            return this.totalScore;
        }

        public void setTotalScore(double totalScore)
        {
            this.totalScore = totalScore;
        }

        public int getVisits()
        {
            return this.visits;
        }

        public void setVisits(int visits)
        {
            this.visits = visits;
        }

        public int getConsiderations()
        {
            return this.considerations;
        }

        public void setConsiderations(int considerations)
        {
            this.considerations = considerations;
        }


        /// <summary>
        /// Returns all untried moves by filtering out all currently possible moves that have already been tried at least once.
        /// </summary>
        /// <param name="possibleMoves"> an AIArrayList of all possible moves</param>
        /// <returns>an AIArrayList of untried moves</returns>
        public AIArrayList<Move> getUntriedMoves(AIArrayList<Move> possibleMoves)
        {
            AIArrayList<Move> triedMoves = new AIArrayList<Move>();
            for (int i = 0; i < this.children.Count(); i++)
            {
                triedMoves.add(this.children.get(i).getMove());
            }

            AIArrayList<Move> untriedMoves = new AIArrayList<Move>();

            for (int i = 0; i < possibleMoves.Count(); i++)
            {
                bool tried = false;
                for (int j = 0; j < triedMoves.Count(); j++)
                {
                    if (possibleMoves.get(i).equals(triedMoves.get(j)))
                    {
                        tried = true;
                        break;
                    }
                }

                if (!tried)
                {
                    untriedMoves.add(possibleMoves.get(i));
                }
            }

            return untriedMoves;
        }

        /// <summary>
        /// Returns the most promising child of this node (the child with the highest UCT score).
        /// </summary>
        /// <param name="possibleMoves">an AIArrayList of all possible moves</param>
        /// <param name="exploration">the exploration factor for the UCT formula</param>
        /// <returns>the most promising child node</returns>
        public Node selectChild(AIArrayList<Move> possibleMoves, double exploration)
        {
            Node selection = null;
            double selectionScore = -1.0;

            for (int i = 0; i < this.children.Count(); i++)
            {
                Node child = this.children.get(i);
                bool contains = false;
                for (int j = 0; j < possibleMoves.Count(); j++)
                {
                    if (possibleMoves.get(j).equals(child.move))
                    {
                        contains = true;
                        break;
                    }
                }

                if (contains)
                {
                    double currentScore = calculateUCTScore(child, exploration);

                    if (currentScore > selectionScore)
                    {
                        selection = child;
                        selectionScore = currentScore;
                    }
                    child.setConsiderations(child.getConsiderations() + 1);
                }
            }

            return selection;
        }

        // 
        /**
         * Adds a new child node for this node and returns it.
         * @param move the move that was used to get to the state of the game represented by the child
         * @param player the player who did the move
         * @return the newly created child node
         */
        public Node addChild(Move move, Player player)
        {
            Node newNode = new Node(this, move.copy(), player);
            this.children.add(newNode);
            return newNode;
        }

        /**
         * Updates visits and total score for this node. Used when backpropagating the simulation result.
         * @param winner the winner of the simulation
         * @param result the simulation result from the winners perspective
         */
        public void update(int nTrickCount)
        {
            this.visits++;
            double nDiffValue = 1/(double)(10 + Math.Abs(this.player.bidValue.value - nTrickCount));
            this.totalScore += nDiffValue;
            /*
            if (winner.id == this.player.id)
            {
                this.totalScore += nDiffValue;
            }
            else
            {
                this.totalScore += 1 - nDiffValue;
            }
            */
        }

        /**
         * Recursively prints the whole tree from this node down to the root. Used for debugging.
         * @param indent the Count of the indentation when printing this node (0 when called the first time)
         * @return the string representing the tree
         */
        public String treeToString(int indent)
        {
            String s = indentString(indent) + this.toString();
            for (int i = 0; i < this.children.Count(); i++)
            {
                s += this.children.get(i).treeToString(indent + 1);
            }
            return s;
        }

        /**
         * Manages the printing indentation for different child levels. Used for debugging.
         * @param indent the Count of the indentation when printing this node
         * @return the string representing the indentation
         */
        private String indentString(int indent)
        {
            String s = "\n";
            for (int i = 0; i < indent + 1; i++)
            {
                s += "| ";
            }
            return s;
        }

        public String toString()
        {
            if (this.move == null)
            {
                return this.player.id + " ROOT, SCORE: " + this.totalScore + ", VISITS: " + this.visits + ", CONSIDERATIONS: " + this.considerations;
            }
            else if (this.parent.move == null)
            {
                return this.player.id + " MOVE: " + this.move.toString() + ", SCORE: " + this.totalScore + ", VISITS: " + this.visits + ", CONSIDERATIONS: " + this.considerations + ", PARENT: ROOT";
            }
            return this.player.id + " MOVE: " + this.move.toString() + ", SCORE: " + this.totalScore + ", VISITS: " + this.visits + ", CONSIDERATIONS: " + this.considerations;
        }

        /**
         * Basic UCT formula used for calculating the next node selection.
         */
        private double calculateUCTScore(Node node, double exploration)
        {
            return (node.getTotalScore() / node.getVisits()) + (exploration * Math.Sqrt(Math.Log(node.getConsiderations()) / node.getVisits()));
        }
    }
}
