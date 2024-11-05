using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;
internal class Move
{
    public World world;
    public Piece piece;
    public int targetX;
    public int targetY;

    public Move(World world, Piece piece)
    {
        this.world = world;
        this.piece = piece;
    }

    public void Execute()
    {
        world.board.MovePiece(piece, targetX, targetY);
    }
}

class Turn
{

}