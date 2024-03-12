using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] GameObject groundPrefab, wallPrefab,sectionLinePrefab;
    public int mapWidth, mapHeight,sectionMargin,roomConerMargin;
    RougeGenerator mapGenerator;
    int[,] map;
    float tileSize;
    Vector2 mapCenterPos;
    void Start()
    {
        mapGenerator = new RougeGenerator(mapWidth, mapHeight);
        map = mapGenerator.GenerateMap();
        PlaceTiles();
    }
    
    void PlaceTiles()
    {
        
        tileSize = groundPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        mapCenterPos = new Vector2(mapWidth * tileSize / 2, mapHeight * tileSize / 2);
        for(int y = 0;y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                int tileType = map[x, y];
                Vector2 pos = GetWorldPositionFromTile(x, y);
                
                if(tileType == 0)
                {
                    Instantiate(groundPrefab, pos, Quaternion.Euler(0, 0, 0f));
                }
                else if(tileType == 2)
                {
                    Instantiate(sectionLinePrefab, pos, Quaternion.Euler(0, 0, 0f));
                }
                else if (tileType == 1)
                {
                    Instantiate(wallPrefab, pos, Quaternion.Euler(0, 0, 0f));
                }
            }
        }
    }

    Vector2 GetWorldPositionFromTile(int x,int y)
    {
        return new Vector2(x * tileSize, (mapHeight - y) * tileSize) - mapCenterPos;
    }
}