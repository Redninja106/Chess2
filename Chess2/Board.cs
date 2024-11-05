using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;
internal class Board
{
    private World world;
    public List<Piece> pieces = [];
    public Dictionary<(int x, int y), Tile> tiles = [];

    public Board(World world)
    {
        this.world = world;
    }

    public bool AddPiece(int x, int y, PiecePrototype prototype, Team team)
    {
        if (!IsTileEmpty(x, y))
            return false;

        Piece piece = new(world, prototype);
        piece.LocationX = x;
        piece.LocationY = y;
        piece.team = team;

        pieces.Add(piece);
        tiles[(x, y)].piece = piece;

        return true;
    }

    public bool IsTileEmpty(int x, int y)
    {
        return tiles.TryGetValue((x, y), out var tile) && tile.piece == null;
    }

    public void SetTile(int x, int y, Tile? tile)
    {
        if (tile == null)
        {
            tiles.Remove((x, y));
            return;
        }

        if (tiles.TryGetValue((x, y), out var prevTile))
        {
            tile.piece = prevTile.piece;
        }

        tiles[(x, y)] = tile;
    }

    public void MovePiece(Piece piece, int targetX, int targetY)
    {
        Tile source = GetTile(piece.LocationX, piece.LocationY)!;
        Tile? target = GetTile(targetX, targetY);

        if (target?.piece != null)
        {
            // capturing
            pieces.Remove(target.piece);
        }

        tiles[(piece.LocationX, piece.LocationY)].piece = null;
        tiles[(targetX, targetY)].piece = piece;
        piece.LocationX = targetX;
        piece.LocationY = targetY;
        piece.hasMoved = true;
    }

    public Tile? GetTile(int x, int y)
    {
        return tiles.TryGetValue((x, y), out Tile? t) ? t : null;
    }
}
