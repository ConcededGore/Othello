using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : AIScript {

    BoardSpace[][] board;

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currBoard) {
        board = currBoard;

        //HMinimizeOpponentsMoves(availableMoves);

        return availableMoves[HMinimizeOpponentsMoves(availableMoves)];
    }

    private int HMinimizeOpponentsMoves(List<KeyValuePair<int, int>> availableMoves) {

        List<Vector2> enemyMoves = new List<Vector2>();

        for (int i = 0; i < availableMoves.Count; i++) {
            KeyValuePair<int, int> temp = availableMoves[i];
            int tempKey = temp.Key;
            int tempValue = temp.Value;
            BoardSpace[][] tempBoard = board;

            //Debug.Log(BoardScript.GetTurnNumber());
            tempBoard[tempKey][tempValue] = getColor(BoardScript.GetTurnNumber());
            enemyMoves.Add(new Vector2(i, BoardScript.GetValidMoves(tempBoard, BoardScript.GetTurnNumber() + 1).Count));
            Debug.Log(enemyMoves[i].x + " " + enemyMoves[i].y);
            tempBoard[tempKey][tempValue] = BoardSpace.EMPTY;
        }

        int retval = 0;
        int retvalVal = int.MaxValue;
        for (int i = 0; i < enemyMoves.Count; i++) {
            if (enemyMoves[i].y < retvalVal) {
                retval = (int)enemyMoves[i].x;
                retvalVal = (int)enemyMoves[i].y;
            }
        }

        return retval;
    }

    private BoardSpace getColor(uint turn) {
        if (turn % 2 == 0) {
            return BoardSpace.BLACK;
        }
        return BoardSpace.WHITE;
    }
}
