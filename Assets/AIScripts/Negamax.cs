using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Negamax : AIScript
{

    public int maxDepth = 5;
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

    }

    private KeyValuePair<int, int> negamax(KeyValuePair<int,int> node, int depth, int color)
    {

    }
}
