using estimation.ai.common;
using estimation.ai.manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai
{
    public class ISMCTS
    {
        private State rootState; // The current state of the real game from which the algorithm tries to find the optimal move
        private State currentState;  // The current state of the simulated game
        private AIArrayList<Move> possibleMoves; // List of all possible moves from currentState
        private Node currentNode; // The node currently
        private int limit; // Iteration limit for searching the best move
        private Random random;

        private static double EXPL = 0.7; // Exploration factor for UCT child selection, can be adjusted to make the AI play differently

        public ISMCTS(State rootState, int limit)
        {
            this.rootState = rootState.CloneState();
            this.limit = limit;
            this.random = new Random();
        }

        public void runThread(object data)
        {
            Action<Move> callback = data as Action<Move>;
            Move nextMove = run();
            callback(nextMove);
        }

        /**
        * Runs the ISMCTS algorithm, searching for the optimal move from rootState.
        * @return the most promising move found
        */
        public Move run()
        {
            // Root node represents the current game situation
            Node rootNode = new Node(null, null, this.rootState.GetCurrentPlayer());
            
            // Main loop of the tree search
            for (int i = 0; i < limit; i++)
            {
                this.currentNode = rootNode;

                // 1. Clone and determinize the state of the game (randomize information unknown to the AI)
                this.currentState = this.rootState.CloneAndRandomizeState();

                this.possibleMoves = this.currentState.GetAvailableMoves();

                // 2. Select the most promising child node
                selectChildISMCTS();

                // 3. Expand the tree by creating a new child node for the selected node
                expandTreeISMCTS();

                // 4. Simulate by doing random moves from the expanded node until the game ends
                simulateISMCTS();

                // 5. Backpropagate the simulation result from the expanded node (in step 3) to every node along the way to the root
                backPropagateISMCTS();
            }
            //Console.WriteLine(rootNode.treeToString(0)); // uncomment to visualize the tree, useful for debugging

            // Find the best move using backpropagated results
            Node best = rootNode.getChildren().get(0);
            for (int i = 0; i < rootNode.getChildren().Count(); i++)
            {
                if (rootNode.getChildren().get(i).getVisits() > best.getVisits())
                {
                    best = rootNode.getChildren().get(i);
                }
            }
            return best.getMove();

        }

        private void selectChildISMCTS()
        {
            // While every move option has been explored and the game hasn't ended
            while ((!possibleMoves.isEmpty() && !this.currentState.roundOver()) && this.currentNode.getUntriedMoves(this.possibleMoves).isEmpty())
            {
                this.currentNode = this.currentNode.selectChild(this.possibleMoves, EXPL); // Descend the tree
                this.currentState.doMove(this.currentNode.getMove(), true); // Update the state
                this.possibleMoves = this.currentState.GetAvailableMoves(); // Possible moves change after a move so we have to redo this here
            }
        }

        private void expandTreeISMCTS()
        {
            AIArrayList<Move> untriedMoves = this.currentNode.getUntriedMoves(this.possibleMoves);
            if (!untriedMoves.isEmpty())
            { // If the game didn't end yet
                Move randomMove = untriedMoves.get(this.random.Next(untriedMoves.Count())); // Do a random move
                Player currentPlayer = this.currentState.GetCurrentPlayer(); // Store current player in case the turn ends after the move
                this.currentNode = this.currentNode.addChild(randomMove, currentPlayer); // Add a child representing the new move and descend the tree
                this.currentState.doMove(randomMove, false);
            }
        }

        private void simulateISMCTS()
        {
            this.possibleMoves = this.currentState.GetAvailableMoves();

            while (!this.possibleMoves.isEmpty() && !this.currentState.roundOver())
            {
                // Do the random move and update possible moves
                this.currentState.doMove(this.possibleMoves.get(this.random.Next(this.possibleMoves.Count())), false);
                this.possibleMoves = this.currentState.GetAvailableMoves();
            }
        }

        private void backPropagateISMCTS()
        {
            Dictionary<int, int> result = this.currentState.GetTrickResult();
            while (this.currentNode != null)
            {
                this.currentNode.update(result[this.currentNode.getPlayer().id]);
                this.currentNode = currentNode.getParent();
            }
        }

        public State getRootState()
        {
            return this.rootState;
        }

        public void setRootState(State rootState)
        {
            this.rootState = rootState.CloneState();
        }
        public int getLimit()
        {
            return this.limit;
        }

    }
}
