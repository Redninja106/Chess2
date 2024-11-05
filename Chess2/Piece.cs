using Chess2;
using SimulationFramework.Drawing;

namespace Chess2;

class Piece
{
    public int LocationX, LocationY;
    public PiecePrototype prototype;
    public World world;
    public Team team;
    public bool hasMoved = false;

    public Piece(World world, PiecePrototype prototype)
    {
        this.world = world;
        this.prototype = prototype;
    }

    public void Render(ICanvas canvas)
    {
    }

    public void Update()
    {
    }
}

class PiecePrototype
{
    public SpriteStack spriteStack;
    public MovementRule[] movementRules;
}

[Flags]
enum MovementRuleFlags
{
    None = 0,
    Repeat = 1 << 0,
    CannotHaveMoved = 1 << 1,
    MustAttack = 1 << 2,
    CannotAttack = 1 << 3,
}

class MovementRule
{
    public int offsetX;
    public int offsetY;
    public MovementRuleFlags flags;

    public MovementRule(int offsetX, int offsetY, MovementRuleFlags flags)
    {
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.flags = flags;
    }

    public virtual bool AppliesTo(Piece piece)
    {
        return true;
    }

    public virtual bool AllowsMove(Piece piece, int targetX, int targetY)
    {
        if (piece.hasMoved && flags.HasFlag(MovementRuleFlags.CannotHaveMoved))
        {
            return false;
        }

        Tile? target = null;
        var (offx, offy) = GetRotatedOffset(piece.team.orientation);
        if (flags.HasFlag(MovementRuleFlags.Repeat))
        {
            int currentX = piece.LocationX;
            int currentY = piece.LocationY;
            while (true)
            {
                currentX += offx;
                currentY += offy;

                Tile? tile = piece.world.board.GetTile(currentX, currentY);

                if (targetX == currentX && targetY == currentY)
                {
                    // reached the target tile
                    target = tile;
                    break;
                }

                if (tile == null)
                {
                    // there's blank space in the way
                    return false;
                }

                if (tile.piece != null)
                {
                    // another peice in the way
                    return false;
                }
            }
        }
        else
        {
            if (piece.LocationX + offx == targetX && piece.LocationY + offy == targetY)
            {
                target = piece.world.board.GetTile(targetX, targetY);
            }
        }

        if (target == null)
        {
            // no tile at target
            return false;
        }
        else if (target.piece == null)
        {
            // target is empty tile
            return !flags.HasFlag(MovementRuleFlags.MustAttack);
        }
        else if (target.piece.team != piece.team)
        {
            // target is non-friendly piece
            return !flags.HasFlag(MovementRuleFlags.CannotAttack);
        }
        else
        {
            // cannot move on top friendly piece
            return false;
        }
    }

    private (int x, int y) GetRotatedOffset(int rotations)
    {
        int x = offsetX, y = offsetY;
        for (int i = 0; i < rotations; i++)
        {
            (x, y) = (-y, x);
        }
        return (x, y);
    }
}
