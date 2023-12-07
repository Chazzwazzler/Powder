using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHandler
{
    public enum State
    {   
        Powder,
        Liquid,
        Gas
    }; 

}

[System.Serializable]
public class TileData : TileHandler
{
    public TileData preset;

    public Tile tile;
    public float mass;
    public State state;

    public bool updated;

    public Vector3Int position;

    public Vector2Int chunksIndex;
}

public class Chunk
{
    public TileData[,] chunkTiles;
    public bool chunkIsActive = false;

    public Vector3Int chunkOrigin;
}



