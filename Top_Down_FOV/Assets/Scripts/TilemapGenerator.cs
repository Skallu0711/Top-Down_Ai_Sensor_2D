using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap walkableTilemap;
    [SerializeField] private Tilemap nonWalkableTilemap;
    
    [SerializeField] private List<Tile> floorTiles;
    [SerializeField] private Tile wallTile;
    
    [Header("Size parameters")]
    [SerializeField] private int widthInTiles = 30;
    [SerializeField] private int heightInTiles = 30;

    private void Start()
    {
        var halfWidth = (int) (widthInTiles * 0.5f);
        var halfHeight = (int) (heightInTiles * 0.5f);
        
        for (int x = 0; x < widthInTiles; x++)
        {
            for (int y = 0; y < heightInTiles; y++)
            {
                if (x == 0 || x == widthInTiles - 1 || y == 0 || y == heightInTiles - 1)
                    nonWalkableTilemap.SetTile(new Vector3Int(x - halfWidth, y - halfHeight, 0), wallTile);
                else
                    walkableTilemap.SetTile(new Vector3Int(x - halfWidth, y - halfHeight, 0), floorTiles[Random.Range(0, floorTiles.Count)]);
            }
        }
        
        // setup wall collision
        gameObject.AddComponent<CompositeCollider2D>();
        nonWalkableTilemap.gameObject.AddComponent<TilemapCollider2D>().usedByComposite = true;
    }

}