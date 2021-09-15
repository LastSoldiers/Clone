using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField]public int width; //grid alanı
    [SerializeField]public int height;

    public GameObject[] dots;//oluşturalacak assetler 

    public GameObject tilePrefab;

    private BackgroundTile[,] allTiles; //gridler

    public GameObject[,] allDots;


    // Start is called before the first frame update
    void Start()
    {
        allTiles = new BackgroundTile[width, height];  //grid oluşturma
        allDots = new GameObject[width, height];
        SetUp(); //instantiate fonksiyon
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";

                //kullanılacak assetler,şekerler
                int dotToUse = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }

    }



    // Update is called once per frame
    void Update()
    {

    }
}
