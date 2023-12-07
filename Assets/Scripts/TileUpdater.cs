using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileUpdater : MonoBehaviour
{
    public static TileUpdater instance;

    public TileData[,] tiles;
    public Chunk[,] chunks;

    public int mapSize;
    public int chunkSize;

    public Tilemap map; 
    public Tilemap boundaryMap;
    public Tilemap debugMap;

    public int brushSize;

    [HideInInspector]
    public TileData currentTile;

    public TileData sand;
    public TileData water;
    public TileData air;

    private void Awake() {
        instance = this;

        tiles = new TileData[mapSize,mapSize];
        chunks = new Chunk[mapSize/chunkSize, mapSize/chunkSize];

        //Generate Chunks
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                chunks[x,y] = new Chunk();
                chunks[x,y].chunkTiles = new TileData[chunkSize, chunkSize];

                chunks[x,y].chunkOrigin = new Vector3Int(x*chunkSize,y*chunkSize,0);
                chunks[x,y].chunkIsActive = false;

                //Draw tile at the bottom left corner of each chunk
                debugMap.SetTile(chunks[x,y].chunkOrigin, sand.tile);
            }
        }

        currentTile = sand;
    }

    private void FixedUpdate() {
        foreach (var chunk in chunks)
        {
            if(chunk.chunkIsActive){
                bool stayActive = false;
                for (int x = 0; x < chunk.chunkTiles.GetLength(0); x++)
                {
                    for (int y = 0; y < chunk.chunkTiles.GetLength(1); y++)
                    {
                        debugMap.SetTile(new Vector3Int(x + chunk.chunkOrigin.x,y+ chunk.chunkOrigin.y,0), sand.tile);
                        TileData tile = chunk.chunkTiles[x,y];
                        if(tile != null){
                            stayActive = true;

                            switch (tile.state)
                            {
                                case TileHandler.State.Powder:
                                    UpdatePowder(tile);
                                    break;
                                case TileHandler.State.Liquid:
                                    UpdateLiquid(tile);
                                    break;
                                case TileHandler.State.Gas:
                                    UpdateGas(tile);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                //Disables chunk and its debug boundaries
                if(stayActive == false){
                    chunk.chunkIsActive = false;

                    for (int x1 = 0; x1 < chunk.chunkTiles.GetLength(0); x1++)
                    {
                        for (int y1 = 0; y1 < chunk.chunkTiles.GetLength(1); y1++)
                        {
                            debugMap.SetTile(new Vector3Int(x1 + chunk.chunkOrigin.x,y1+ chunk.chunkOrigin.y,0), null);
                        }
                    }
                }
            }
        }
    }

    //STATE UPDATING CODE
    public void UpdatePowder(TileData tile){
        
        List<Vector3Int> diagonaldirections = new List<Vector3Int>();

        diagonaldirections.Add(Vector3Int.left + Vector3Int.down);
        diagonaldirections.Add(Vector3Int.right + Vector3Int.down);
        diagonaldirections.Add(Vector3Int.down);

        while (true)
        {
            if(diagonaldirections.Count == 0){
                break;
            }
            int rand = Random.Range(0, diagonaldirections.Count);

            if(!tile.TileInDir(diagonaldirections[rand])){
                tile.MoveTile(diagonaldirections[rand]);
                return;
            }
            else{
                Vector3Int pos = diagonaldirections[rand] + tile.position;
                if(map.GetTile(pos) != null){
                    if(diagonaldirections[rand].y == -1 && pos.GetTileAtPos().mass < tile.mass){
                        tile.Displace(pos.GetTileAtPos());
                        return;
                    }
                }
                diagonaldirections.Remove(diagonaldirections[rand]);
            }
        }
    }
    public void UpdateLiquid(TileData tile){
        List<Vector3Int> diagonaldirections = new List<Vector3Int>();

        diagonaldirections.Add(Vector3Int.left + Vector3Int.down);
        diagonaldirections.Add(Vector3Int.right + Vector3Int.down);
        diagonaldirections.Add(Vector3Int.down);

        while (true)
        {
            if(diagonaldirections.Count == 0){
                break;
            }
            int rand = Random.Range(0, diagonaldirections.Count);

            if(!tile.TileInDir(diagonaldirections[rand])){
                tile.MoveTile(diagonaldirections[rand]);
                return;
            }
            else{
                Vector3Int pos = diagonaldirections[rand] + tile.position;
                if(map.GetTile(pos) != null){
                    if(diagonaldirections[rand].y == -1 && pos.GetTileAtPos().mass < tile.mass){
                        tile.Displace(pos.GetTileAtPos());
                        return;
                    }
                }
                diagonaldirections.Remove(diagonaldirections[rand]);
            }
        }


        List<Vector3Int> sidedirections = new List<Vector3Int>();

        sidedirections.Add(Vector3Int.left);
        sidedirections.Add(Vector3Int.right);

        while (true)
        {
            if(sidedirections.Count == 0){
                break;
            }
            int rand = Random.Range(0, sidedirections.Count);

            if(!tile.TileInDir(sidedirections[rand])){
                tile.MoveTile(sidedirections[rand]);
                return;
            }
            else{
                Vector3Int pos = sidedirections[rand] + tile.position;
                if(map.GetTile(pos) != null){
                    if(pos.GetTileAtPos().mass < tile.mass){
                        tile.Displace(pos.GetTileAtPos());
                        return;
                    }
                }
                sidedirections.Remove(sidedirections[rand]);
            }
        }
    }
    public void UpdateGas(TileData tile){
        List<Vector3Int> directions = new List<Vector3Int>();

        directions.Add(Vector3Int.up);
        directions.Add(Vector3Int.up + Vector3Int.left);
        directions.Add(Vector3Int.up + Vector3Int.right);
        directions.Add(Vector3Int.left);
        directions.Add(Vector3Int.right);
        directions.Add(Vector3Int.down);
        directions.Add(Vector3Int.down + Vector3Int.left);
        directions.Add(Vector3Int.down + Vector3Int.right);

        while (true)
        {
            if(directions.Count == 0){
                return;
            }
            int rand = Random.Range(0, directions.Count);

            if(!tile.TileInDir(directions[rand])){
                tile.MoveTile(directions[rand]);
                return;
            }
            else{
                 Vector3Int pos = directions[rand] + tile.position;
                if(map.GetTile(pos) != null){
                    if(directions[rand].y == -1 && pos.GetTileAtPos().mass < tile.mass){
                        tile.Displace(pos.GetTileAtPos());
                        return;
                    }
                }
                directions.Remove(directions[rand]);
            }
        }
    }

    //RUNS AT THE CURRENT FPS, COULD BE MORE RESPONSIVE BUT ANYTHING NEEDING TIME IS VARIABLE
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Minus)){
            brushSize--;
        }
        if(Input.GetKeyDown(KeyCode.Equals)){
            brushSize++;
        }


        if(Input.GetKeyDown(KeyCode.Alpha1)){
            currentTile = sand;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            currentTile = water;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            currentTile = air;
        }

        //Generate 3x3  square around mouse position for use in placing and breaking
        Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
        List<Vector3Int> positions = new List<Vector3Int>();
    
        Vector3Int pos = map.WorldToCell(point);

        for (int x = 0; x < brushSize; x++)
        {
            for (int y = 0; y < brushSize; y++)
            {
                positions.Add(new Vector3Int(pos.x + x, pos.y + y, 0));
            }
        }

        //Placing
        if(Input.GetMouseButton(0)){
            foreach (var item in positions)
            {
                if(boundaryMap.GetTile(item) == null){
                    currentTile.PlaceTile(item);
                }
            }
        }
        //Breaking
        else if(Input.GetMouseButton(1)){
            foreach (var item in positions)
            {
                if(boundaryMap.GetTile(item) == null){
                    tiles[item.x, item.y] = null;
                    Vector2Int index = item.ConvertPositionToChunk();
                    TileUpdater.instance.chunks[index.x, index.y].chunkTiles[item.x - TileUpdater.instance.chunks[index.x, index.y].chunkOrigin.x, item.y - TileUpdater.instance.chunks[index.x, index.y].chunkOrigin.y] = null;

                    map.SetTile(item, null);
                }
            }
        }
    }

}
