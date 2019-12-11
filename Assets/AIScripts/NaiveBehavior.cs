using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaiveBehavior : AIScript {

    BoardSpace[][] board;
    int ai = 0; // 0 attempts to minimize the opponents moves, 1 attempts to pick the best board spaces
    public uint maxDepth = 5;
    int colorNum;

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currBoard) {
        board = currBoard;

        if (color == BoardSpace.BLACK)
            colorNum = -1;
        else
            colorNum = 1;
        KeyValuePair<int, int> best;
        int bestScore = -100000000;
        foreach (KeyValuePair<int, int> n in availableMoves) {
            Debug.Log("iteration start");
            BoardSpace[][] nodeCopy = new BoardSpace[8][];
            for (int x = 0; x < 8; x++) {
                nodeCopy[x] = new BoardSpace[8];
                System.Array.Copy(currBoard[x], nodeCopy[x], 8);
            }
            if (colorNum == -1)
                nodeCopy[n.Value][n.Key] = BoardSpace.BLACK;
            else
                nodeCopy[n.Value][n.Key] = BoardSpace.WHITE;

            //simulate the changes the move would result in
            List<KeyValuePair<int, int>> simulatedChanges = BoardScript.GetPointsChangedFromMove(nodeCopy, BoardScript.GetTurnNumber(), n.Key, n.Value);
            //Debug.Log(simulatedChanges);
            foreach (KeyValuePair<int, int> spot in simulatedChanges) {
                if (nodeCopy[n.Value][n.Key] == BoardSpace.BLACK)
                    nodeCopy[n.Value][n.Key] = BoardSpace.WHITE;
                else
                    nodeCopy[n.Value][n.Key] = BoardSpace.BLACK;
            }
            //Debug.Log("starting negamax traversal");
            int value = negamax(nodeCopy, 0, colorNum);
            if (value >= bestScore)
                best = n;
        }
        Debug.Log("best is x:" + best.Value + " y:" + best.Key);
        return best;
    }

    private int negamax(BoardSpace[][] node, uint depth, int color) {
        //Debug.Log("negamax function start");
        if (depth == maxDepth - 2) {
            int retVal = -1000000;
            List<KeyValuePair<int, int>> moves = BoardScript.GetValidMoves(node, BoardScript.GetTurnNumber() + maxDepth);

            foreach (KeyValuePair<int, int> n in moves) {
                int temp = NaiveRateMoveSelect(moves, n);
                if (temp > retVal)
                    retVal = temp;
            }
            return retVal;
        }
        else {
            int value = -100000;
            //go through each valid move for this board state
            foreach (KeyValuePair<int, int> n in BoardScript.GetValidMoves(node, BoardScript.GetTurnNumber())) {
                BoardSpace[][] nodeCopy = new BoardSpace[8][];
                for (int x = 0; x < 8; x++) {
                    nodeCopy[x] = new BoardSpace[8];
                    System.Array.Copy(node[x], nodeCopy[x], 8);
                }
                if (color == -1)
                    nodeCopy[n.Value][n.Key] = BoardSpace.BLACK;
                else
                    nodeCopy[n.Value][n.Key] = BoardSpace.WHITE;

                //simulate the changes each move would result in
                List<KeyValuePair<int, int>> simulatedChanges = BoardScript.GetPointsChangedFromMove(nodeCopy, BoardScript.GetTurnNumber() + depth, n.Key, n.Value);
                foreach (KeyValuePair<int, int> spot in simulatedChanges) {
                    if (nodeCopy[spot.Value][spot.Key] == BoardSpace.BLACK)
                        nodeCopy[spot.Value][spot.Key] = BoardSpace.WHITE;
                    else
                        nodeCopy[spot.Value][spot.Key] = BoardSpace.BLACK;
                }
                //recurse
                value = Mathf.Max(value, -1 * negamax(nodeCopy, depth + 1, -1 * color));
            }
            return value;
        }
    }

    public override void SetAI(int ai) {
        this.ai = ai;
    }

    private int[] HMinimizeOpponentsMoves(List<KeyValuePair<int, int>> availableMoves) {

        List<Vector2> enemyMoves = new List<Vector2>();
        int[] retval = new int[availableMoves.Count];

        for (int i = 0; i < availableMoves.Count; i++) {
            KeyValuePair<int, int> temp = availableMoves[i];
            int tempKey = temp.Key;
            int tempValue = temp.Value;
            BoardSpace[][] tempBoard = board;

            //Debug.Log(BoardScript.GetTurnNumber());
            tempBoard[tempKey][tempValue] = getColor(BoardScript.GetTurnNumber());
            enemyMoves.Add(new Vector2(i, BoardScript.GetValidMoves(tempBoard, BoardScript.GetTurnNumber() + 1).Count));
            retval[i] = enemyMoves.Count;
            //Debug.Log(enemyMoves[i].x + " " + enemyMoves[i].y);
            tempBoard[tempKey][tempValue] = BoardSpace.EMPTY;
        }

        return retval;
    }

    // Rates a specific tile from 0-100 based on the current board state
    private int rateMoveSelect(List<KeyValuePair<int, int>> availableMoves, KeyValuePair<int, int> KVP) {
        int[][] boardScores = getBoardScores();

        int[] moveScores = new int[availableMoves.Count];
        int retval = 0;
        int move = 0;

        for (int i = 0; i < availableMoves.Count; i++) {
            if (availableMoves[i].Key == KVP.Key && availableMoves[i].Value == KVP.Value) {
                move = i;
                break;
            }
        }

        if (CompletesEdge(KVP, (uint)color)) {
            retval = 100;
        }
        else {
            retval = boardScores[KVP.Key][KVP.Value] * 25;
        }

        moveScores = HMinimizeOpponentsMoves(availableMoves);

        int highMoves = 0;
        int lowMoves = int.MaxValue;

        for (int i = 0; i < moveScores.Length; i++) {
            if (moveScores[i] > highMoves) {
                highMoves = moveScores[i];
            }
            if (moveScores[i] < lowMoves) {
                lowMoves = moveScores[i];
            }
        }

        highMoves -= lowMoves;
        int score = moveScores[move] - lowMoves;

        if (highMoves == 0)
            highMoves++;
        int temp = (100 * score / highMoves);

        temp = 100 - temp;

        retval += temp;

        return retval;
    }

    private int NaiveRateMoveSelect(List<KeyValuePair<int, int>> availableMoves, KeyValuePair<int, int> KVP) {
        int[][] boardScores = getBoardScores();

        int[] moveScores = new int[availableMoves.Count];
        int retval = 0;
        int move = 0;

        for (int i = 0; i < availableMoves.Count; i++) {
            if (availableMoves[i].Key == KVP.Key && availableMoves[i].Value == KVP.Value) {
                move = i;
                break;
            }
        }

        if (CompletesEdge(KVP, (uint)color)) {
            return 100;
        }
        else {
            return boardScores[KVP.Key][KVP.Value] * 25;
        }
    }

    private int[][] getBoardScores() {

        int[][] retval = new int[8][] {
            new int[8] {4,-3,2,2,2,2,-3,4},
            new int[8] {-3,-4,-1,-1,-1,-1,-4,-3},
            new int[8] {2,-1,1,0,0,1,-1,2},
            new int[8] {2,-1,0,1,1,0,-1,2},
            new int[8] {2,-1,0,1,1,0,-1,2},
            new int[8] {2,-1,1,0,0,1,-1,2},
            new int[8] {-3,-4,-1,-1,-1,-1,-4,-3},
            new int[8] {4,-3,2,2,2,2,-3,4}
        };

        return retval;
    }
    /*
    private int[] HTileSelection(BoardSpace[][] board, List<KeyValuePair<int, int>> availableMoves) {
        int[] retval = new int[availableMoves.Count];
        for (int i = 0; i < availableMoves.Count; i++) {
            retval[i] = 0;
        }

        for (int i = 0; i < availableMoves.Count; i++) {
            if (isCorner(availableMoves[i])) {
                retval[i] = 100;
                continue;
            }
            if (isXTile(availableMoves[i])) {
                retval[i] = -100;
                continue;
            }
            if (CompletesEdge(availableMoves[i], BoardScript.GetTurnNumber())) {
                retval[i] = 100;
                continue;
            }
            if (isCTile(availableMoves[i])) {
                retval[i] = -20;
            }
            if (isSweet16(availableMoves[i])) {
                retval[i] = 20;
            }
        }

        return retval;
    }
    */
    private BoardSpace getColor(uint turn) {
        if (turn % 2 == 0) {
            return BoardSpace.BLACK;
        }
        return BoardSpace.WHITE;
    }

    private bool isCorner(KeyValuePair<int, int> KVP) {
        if (KVP.Key == 0 && (KVP.Value == 0 || KVP.Value == 7)) {
            return true;
        }
        else if (KVP.Key == 7 && (KVP.Value == 0 || KVP.Value == 7)) {
            return true;
        }
        return false;
    }
    /*
        private bool isXTile(KeyValuePair<int, int> KVP) {
            if (KVP.Key == 1 && (KVP.Value == 1 || KVP.Value == 6)) {
                return true;
            }
            else if (KVP.Key == 6 && (KVP.Value == 1 || KVP.Value == 6)) {
                return true;
            }
            return false;
        }

        private bool isCTile(KeyValuePair<int, int> KVP) {
            if (KVP.Key == 0 && (KVP.Value == 1 || KVP.Value == 6)) {
                return true;
            }
            else if (KVP.Key == 7 && (KVP.Value == 1 || KVP.Value == 6)) {
                return true;
            }
            return false;
        }

        private bool isSweet16(KeyValuePair<int, int> KVP) {
            if (KVP.Key < 2 || KVP.Key > 5) {
                return false;
            }
            if (KVP.Value < 2 || KVP.Value > 5) {
                return false;
            }
            return true;
        }
        */ // Tests that are no longer needed
           // THIS ASSUMES THAT BLACK HAS EVEN TURN # AND WHITE HAS ODD
    private bool CompletesEdge(KeyValuePair<int, int> KVP, uint turn) {
        BoardSpace color = (turn % 2 == 0) ? BoardSpace.BLACK : BoardSpace.WHITE;
        // Check if its on an edge
        if (!CheckOnEdge(KVP)) {
            return false;
        }

        // Check if the rest of the tiles are the same color
        return CheckSameColor(board, KVP, color);
    }

    private void testCheckSameColor() {
        BoardSpace[][] tempBoard = new BoardSpace[8][];

        tempBoard = getEmptyBoard();

        // Basic test if true
        for (int i = 0; i < 8; i++) {
            tempBoard[i][0] = BoardSpace.BLACK;
        }
        tempBoard[3][0] = BoardSpace.EMPTY;
        Debug.Log("test 1 should be true, is: " + CheckSameColor(tempBoard, new KeyValuePair<int, int>(3, 0), BoardSpace.BLACK));

        tempBoard = getEmptyBoard();

        // Basic test if false (empty)
        Debug.Log("test 2 should be false, is " + CheckSameColor(tempBoard, new KeyValuePair<int, int>(3, 0), BoardSpace.BLACK));

        tempBoard = getEmptyBoard();

        // Basic test if false (almost full)
        for (int i = 0; i < 8; i++) {
            tempBoard[i][0] = BoardSpace.BLACK;
        }
        tempBoard[4][0] = BoardSpace.WHITE;
        Debug.Log("test 3 should be false, is " + CheckSameColor(tempBoard, new KeyValuePair<int, int>(3, 0), BoardSpace.BLACK));

        tempBoard = getEmptyBoard();

        // Basic test if true (corner)
        for (int i = 0; i < 8; i++) {
            tempBoard[i][0] = BoardSpace.BLACK;
        }
        for (int i = 0; i < 8; i++) {
            tempBoard[0][i] = BoardSpace.BLACK;
        }
        tempBoard[0][0] = BoardSpace.EMPTY;

        Debug.Log("Test 4 should be true, is: " + CheckSameColor(tempBoard, new KeyValuePair<int, int>(0, 0), BoardSpace.BLACK));

        tempBoard = getEmptyBoard();

        // Basic test if true (one edge of corner)
        for (int i = 0; i < 8; i++) {
            tempBoard[i][0] = BoardSpace.BLACK;
        }
        for (int i = 0; i < 8; i++) {
            tempBoard[0][i] = BoardSpace.BLACK;
        }
        tempBoard[0][0] = BoardSpace.EMPTY;
        tempBoard[0][2] = BoardSpace.WHITE;

        Debug.Log("Test 5 should be true, is: " + CheckSameColor(tempBoard, new KeyValuePair<int, int>(0, 0), BoardSpace.BLACK));

    }

    private BoardSpace[][] getEmptyBoard() {
        BoardSpace[][] retval = new BoardSpace[8][];
        for (int i = 0; i < 8; i++) {
            retval[i] = new BoardSpace[8];
        }

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                retval[i][j] = BoardSpace.EMPTY;
            }
        }

        return retval;
    }

    private bool CheckOnEdge(KeyValuePair<int, int> KVP) {
        return (KVP.Key == 0 || KVP.Key == 7 || KVP.Value == 0 || KVP.Value == 7);
    }

    private bool CheckSameColor(BoardSpace[][] board, KeyValuePair<int, int> KVP, BoardSpace color) {
        int edge = -1; // -1 = NULL; 0 = SOUTH; 1 = EAST; 2 = NORTH; 3 = WEST;
        int doubleEdge = -1;
        if (isCorner(KVP)) {
            if (KVP.Key == 0) {
                edge = 3;
                if (KVP.Value == 0) {
                    doubleEdge = 0;
                }
                else {
                    doubleEdge = 2;
                }
            }
            else {
                edge = 1;
                if (KVP.Value == 0) {
                    doubleEdge = 0;
                }
                else {
                    doubleEdge = 2;
                }
            }
        }
        else {
            if (KVP.Key == 0) {
                edge = 3;
            }
            else if (KVP.Key == 7) {
                edge = 1;
            }
            else if (KVP.Value == 0) {
                edge = 0;
            }
            else if (KVP.Value == 7) {
                edge = 2;
            }
        }

        // USE THE EDGES TO CHECK IF THE KVP WOULD COMPLETE THE LINE ; very ugly but 5:00AM and it fixed a bug to look like this soooooo
        for (int j = 0; j < 8; j++) {
            switch (edge) {
                case 0:
                    if (j == KVP.Key) {
                        continue;
                    }
                    if (board[j][0] != color) {
                        if (doubleEdge > -1) {
                            break;
                        }
                        else {
                            return false;
                        }
                    }
                    break;
                case 1:
                    if (j == KVP.Value) {
                        continue;
                    }
                    if (board[7][j] != color) {
                        if (doubleEdge > -1) {
                            break;
                        }
                        else {
                            return false;
                        }
                    }
                    break;
                case 2:
                    if (j == KVP.Key) {
                        continue;
                    }
                    if (board[7][j] != color) {
                        if (doubleEdge > -1) {
                            break;
                        }
                        else {
                            return false;
                        }
                    }
                    break;
                case 3:
                    if (j == KVP.Value) {
                        continue;
                    }
                    if (board[0][j] != color) {
                        if (doubleEdge > -1) {
                            break;
                        }
                        else {
                            return false;
                        }
                    }
                    break;
                default:
                    Debug.Log("CheckSameColor switch 1 defaulting!");
                    break;
            }
        }

        if (doubleEdge > -1) {
            for (int j = 0; j < 8; j++) {
                switch (doubleEdge) {
                    case 0:
                        if (j == KVP.Key) {
                            continue;
                        }
                        if (board[j][0] != color) {
                            return false;
                        }
                        break;
                    case 1:
                        if (j == KVP.Value) {
                            continue;
                        }
                        if (board[7][j] != color) {
                            return false;
                        }
                        break;
                    case 2:
                        if (j == KVP.Key) {
                            continue;
                        }
                        if (board[7][j] != color) {
                            return false;
                        }
                        break;
                    case 3:
                        if (j == KVP.Value) {
                            continue;
                        }
                        if (board[0][j] != color) {
                            return false;
                        }
                        break;
                    default:
                        Debug.Log("CheckSameColor switch 2 defaulting!");
                        break;
                }
            }
        }

        return true;
    }
}
