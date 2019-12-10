using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Negamax : AIScript
{

    public uint maxDepth = 5;
    private int colorNum;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard)
    {
        if (color == BoardSpace.BLACK)
            colorNum = 1;
        else
            colorNum = -1;
        KeyValuePair<int, int> best;
        int bestScore = -100000000;
        foreach(KeyValuePair<int,int> n in availableMoves)
        {
            BoardSpace[][] nodeCopy = (BoardSpace[][])currentBoard.Clone();
            if (colorNum == 1)
                nodeCopy[n.Key][n.Value] = BoardSpace.BLACK;
            else
                nodeCopy[n.Key][n.Value] = BoardSpace.WHITE;

            //simulate the changes the move would result in
            List<KeyValuePair<int, int>> simulatedChanges = BoardScript.GetPointsChangedFromMove(nodeCopy, BoardScript.GetTurnNumber() + depth, n.Key, n.Value);
            foreach (KeyValuePair<int, int> spot in simulatedChanges)
            {
                if (nodeCopy[spot.Key][spot.Value] == BoardSpace.BLACK)
                    nodeCopy[spot.Key][spot.Value] = BoardSpace.WHITE;
                else
                    nodeCopy[spot.Key][spot.Value] = BoardSpace.BLACK;
            }
            int value = negamax(nodeCopy, maxDepth, colorNum);
            if (value >= bestScore)
                best = n;
        }
        return best;
    }

    private int negamax(BoardSpace[][] node, uint depth, int color)
    {
        if(depth == maxDepth/* || node.Count == 1*/)
        {
            //return SEF(node)
            return 1;
        }
        /*else if(BoardScript.GetValidMoves(node, BoardScript.GetTurnNumber()).Count == 0)//if list is empty, bail
        {
            //return very negative number
            return -1000000;
        }*/
        else
        {
            int value = -100000;
            //go through each valid move for this board state
            foreach (KeyValuePair<int,int> n in BoardScript.GetValidMoves(node, BoardScript.GetTurnNumber()))
            {
                BoardSpace[][] nodeCopy = (BoardSpace[][])node.Clone();
                if (color == 1)
                    nodeCopy[n.Key][n.Value] = BoardSpace.BLACK;
                else
                    nodeCopy[n.Key][n.Value] = BoardSpace.WHITE;

                //simulate the changes each move would result in
                List<KeyValuePair<int, int>> simulatedChanges = BoardScript.GetPointsChangedFromMove(nodeCopy, BoardScript.GetTurnNumber() + depth, n.Key, n.Value);
                foreach(KeyValuePair<int,int> spot in simulatedChanges)
                {
                    if (nodeCopy[spot.Key][spot.Value] == BoardSpace.BLACK)
                        nodeCopy[spot.Key][spot.Value] = BoardSpace.WHITE;
                    else
                        nodeCopy[spot.Key][spot.Value] = BoardSpace.BLACK;
                }
                //recurse
                value = Mathf.Max(value, -1 * negamax(nodeCopy, depth + 1, -1 * color));
            }
            return value;
        }
    }

    
}
