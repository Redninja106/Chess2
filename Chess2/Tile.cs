namespace Chess2;

abstract class Tile
{
    public float height;
    public Piece? piece;

    public Tile(World world)
    {
    }

    public abstract void Render(ICanvas canvas);
}
