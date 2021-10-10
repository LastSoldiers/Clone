using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;

    [SerializeField]public int width; //grid alan�
    [SerializeField]public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject breakablePrefab;
    public GameObject[] dots;//olusturalacak assetler
    public GameObject destroyEffect;
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] backgroundTiles;

    private BackgroundTile[,] allTiles; //gridler?
    private BackgroundTile[,] breakableTiles;

    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;


    // Start is called before the first frame update
    void Start()
    {
        breakableTiles = new BackgroundTile[width,height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];  //grid olu�turma
        allDots = new GameObject[width, height];
        SetUp(); //instantiate fonksiyon
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }


    public void GenerateBreakableTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Jelly" tile
            if (boardLayout[i].tileKind == TileKind.Breakable)
            {
                //Create a "Jelly" tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakablePrefab,tempPosition,Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {


                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "(" + i + "," + j + ")";

                    //kullan�lacak assetler,�ekerler

                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while (MachesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "(" + i + "," + j + ")";
                    allDots[i, j] = dot;
                }
            }
        }
    }

    private bool MachesAt(int column,int row, GameObject piece)
    {
        if (column > 1 && row >1)
        {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null)
            {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[column, row-1] != null && allDots[column , row-2] != null)
            {
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }else if(column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1] != null && allDots[column, row - 2] != null)
                {
                    if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (allDots[column-1, row] != null && allDots[column-2, row] != null)
                {
                    if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
    }

    private void CheckToMakeBombs()
    {
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Make a adjancent bomb
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    currentDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }


            }
        }
    }

    private void DestroyMatchesAt(int column, int row)//yok etme
    {
        if(allDots[column, row].GetComponent<Dot>().isMatched)
        {
            //How many elements are in the matched pieces list from findmatches
            if (findMatches.currentMatches.Count >= 4 )
            {
                CheckToMakeBombs();
            }

            //Does a tile need to break?
            if (breakableTiles[column,row] !=null)
            {
                //if does, give one damage
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column,row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }

            GameObject particle = Instantiate(destroyEffect,allDots[column,row].transform.position,Quaternion.identity);
            Destroy(particle,.5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()//yok etme
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }       
            }
        }

        findMatches.currentMatches.Clear();
        StartCoroutine(DecraseRowCo2());
    }

    private IEnumerator DecraseRowCo2()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if the current spot isn't blank or is empty...
                if (!blankSpaces[i,j] && allDots[i,j] == null)
                {
                    //loop from the space above to the top of the column
                    for (int k = j+1; k < height; k++)
                    {
                        //if a dot is found...
                        if (allDots[i,k]!=null)
                        {
                            //move that dot to this empty space
                            allDots[i, k].GetComponent<Dot>().row = j;
                            //set that spot to be null
                            allDots[i, k] = null;
                            //break out of the loop
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }


    private IEnumerator DecraseRowCo()
    {
        int nullCount = 0;
        for(int i = 0; i < width; i++)
        {
            for(int j= 0; j < height; j++)
            {
                if(allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard() //board yeniden doldurma & parcalari asagi kaydirma
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allDots[i,j] == null && !blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allDots[i,j] != null)
                {
                    if(allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }





    // Update is called once per frame
    void Update()
    {

    }
}
