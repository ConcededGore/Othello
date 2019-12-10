﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaiveBehavior : AIScript {

    BoardSpace[][] board;
    int ai = 0; // 0 attempts to minimize the opponents moves, 1 attempts to pick the best board spaces

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currBoard) {
        board = currBoard;

        //testCheckSameColor();
        /*
        if (availableMoves.Count == 0) {
            Debug.Log("Make move has been passed an empty available moves");
            return new KeyValuePair<int, int>(0, 0);
        }

        if (ai == 0) {
            return availableMoves[HMinimizeOpponentsMoves(availableMoves)];
        } else if (ai == 1) {
            int highScore = rateMoveSelect(availableMoves[0]);
            List<KeyValuePair<int, int>> potentialMoves = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < availableMoves.Count; i++) {
                if (rateMoveSelect(availableMoves[i]) > highScore) {
                    potentialMoves.Clear();
                    potentialMoves.Add(availableMoves[i]);
                } else if (rateMoveSelect(availableMoves[i]) == highScore) {
                    potentialMoves.Add(availableMoves[i]);
                }
            }

            int rand = Mathf.FloorToInt(Random.Range(0, potentialMoves.Count));

            return potentialMoves[rand];
        } else if (ai == 2) {
            int highScore = rateMoveSelect(availableMoves[0]);
            List<KeyValuePair<int, int>> potentialMoves = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < availableMoves.Count; i++) {

                //int score = rateMoveSelect

                if (rateMoveSelect(availableMoves[i]) > highScore) {
                    potentialMoves.Clear();
                    potentialMoves.Add(availableMoves[i]);
                }
                else if (rateMoveSelect(availableMoves[i]) == highScore) {
                    potentialMoves.Add(availableMoves[i]);
                }
            }

            int rand = Mathf.FloorToInt(Random.Range(0, potentialMoves.Count));

        }
        */
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

        int temp = (100 * score / highMoves);

        temp = 100 - temp;

        retval += temp;

        return retval;
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
        } else if (KVP.Key == 7 && (KVP.Value == 0 || KVP.Value == 7)) {
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
                } else {
                    doubleEdge = 2;
                }
            } else {
                edge = 1;
                if (KVP.Value == 0) {
                    doubleEdge = 0;
                }
                else {
                    doubleEdge = 2;
                }
            }
        } else {
            if (KVP.Key == 0) {
                edge = 3;
            } else if (KVP.Key == 7) {
                edge = 1;
            } else if (KVP.Value == 0) {
                edge = 0;
            } else if (KVP.Value == 7) {
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
                        } else {
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
