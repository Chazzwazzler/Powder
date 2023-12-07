using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileExtensions
{
    public static void PlaceTile(this TileData tile, Vector3Int position){
        TileData newTile = new TileData();
        newTile.tile = tile.tile;
        newTile.mass = tile.mass;
        newTile.state = tile.state;
        newTile.position = position;
        newTile.preset = tile;

        TileUpdater.instance.map.SetTile(position, tile.tile);
        TileUpdater.instance.tiles[position.x, position.y] = newTile; 

        Vector2Int index = position.ConvertPositionToChunk();
        TileUpdater.instance.chunks[index.x, index.y].chunkTiles[position.x - TileUpdater.instance.chunks[index.x, index.y].chunkOrigin.x, position.y - TileUpdater.instance.chunks[index.x, index.y].chunkOrigin.y] = newTile;
        newTile.chunksIndex = index;

        TileUpdater.instance.chunks[index.x, index.y].chunkIsActive = true;
    }

    //Find the chunk a position is in
    public static Vector2Int ConvertPositionToChunk(this Vector3Int position){
        TileUpdater updater = TileUpdater.instance;

        for (int x = 0; x < updater.chunks.GetLength(0); x++)
        {
            for (int y = 0; y < updater.chunks.GetLength(1); y++)
            {
                if(position.x - updater.chunks[x,y].chunkOrigin.x >= 0 && position.x - updater.chunks[x,y].chunkOrigin.x < updater.chunkSize){
                    if(position.y - updater.chunks[x,y].chunkOrigin.y >= 0 && position.y - updater.chunks[x,y].chunkOrigin.y < updater.chunkSize){
                        return new Vector2Int(x, y);
                    }
                }
            }
        }
        return Vector2Int.zero;
    }

    //Check if a position is in a chunk
    public static bool PositionInChunk(this Vector3Int position, Vector2Int index){
        TileUpdater updater = TileUpdater.instance;

        if(position.x - updater.chunks[index.x,index.y].chunkOrigin.x >= 0 && position.x - updater.chunks[index.x,index.y].chunkOrigin.x < updater.chunkSize){
            if(position.y - updater.chunks[index.x,index.y].chunkOrigin.y >= 0 && position.y - updater.chunks[index.x,index.y].chunkOrigin.y < updater.chunkSize){
                return true;
            }
        }

        return false;
    }

    //Convert a position and a chunk index to the index within the chunk
    public static Vector2Int ConvertPositionToInChunk(this Vector3Int position, Vector2Int index){
        TileUpdater updater = TileUpdater.instance;
        return new Vector2Int(position.x - updater.chunks[index.x, index.y].chunkOrigin.x, position.y - updater.chunks[index.x, index.y].chunkOrigin.y);
    }

    public static bool TileInDir(this TileData tile, Vector3Int direction){
        Tilemap map = TileUpdater.instance.map;
        Tilemap boundaryMap = TileUpdater.instance.boundaryMap;

        if(boundaryMap.GetTile(tile.position + direction) == null){
            return map.GetTile(tile.position + direction) != null;
        }
        return true;
    }

    public static void MoveTile(this TileData tile, Vector3Int direction){
        TileUpdater updater = TileUpdater.instance;

        TileData currentTile = tile;

        updater.tiles[tile.position.x, tile.position.y] = null;
        updater.chunks[tile.chunksIndex.x, tile.chunksIndex.y].chunkTiles[tile.position.x - updater.chunks[tile.chunksIndex.x, tile.chunksIndex.y].chunkOrigin.x, tile.position.y - updater.chunks[tile.chunksIndex.x, tile.chunksIndex.y].chunkOrigin.y] = null;
        updater.map.SetTile(tile.position, null);

        Vector3Int prevPos = currentTile.position;
        currentTile.position += direction;
        currentTile.chunksIndex = currentTile.position.ConvertPositionToChunk();

        updater.tiles[currentTile.position.x, currentTile.position.y] = currentTile;
        updater.chunks[currentTile.chunksIndex.x, currentTile.chunksIndex.y].chunkTiles[currentTile.position.x - updater.chunks[currentTile.chunksIndex.x, currentTile.chunksIndex.y].chunkOrigin.x, currentTile.position.y - updater.chunks[currentTile.chunksIndex.x, currentTile.chunksIndex.y].chunkOrigin.y] = currentTile;
        updater.map.SetTile(currentTile.position, currentTile.tile);

        updater.chunks[currentTile.chunksIndex.x, currentTile.chunksIndex.y].chunkIsActive = true;
        currentTile.updated = true;
    }

    public static TileData GetTileAtPos(this Vector3Int position){
        if(TileUpdater.instance.map.GetTile(position) != null){
            return TileUpdater.instance.tiles[position.x, position.y];
        }
        return null;
    }

    public static void Displace(this TileData tile, TileData displaced){
        TileData currentTile = tile;
        TileData displaceThis = displaced;

        Vector3Int tileIntendedDir = tile.position - displaced.position;
        Vector3Int displacedIntendedDir = displaced.position - tile.position;

        currentTile.PlaceTile(displaceThis.position);
        displaceThis.PlaceTile(currentTile.position);
    }
}
