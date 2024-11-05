

namespace Chess2;

class PieceMover
{
    private Piece? selectedPiece = null;

    private World world;

    public Team playerTeam;

    public PieceMover(World world)
    {
        this.world = world;
    }

    Vector2 mp;

    public void Update()
    {
        mp = Vector2.Transform(world.camera.ScreenToLocal(Mouse.Position), world.camera.CreateLocalToPlaneMatrix(0));
        int x = (int)MathF.Round(mp.X), y = (int)MathF.Round(mp.Y);

        if (Mouse.IsButtonReleased(MouseButton.Left))
        {
            Tile? t = world.board.GetTile(x, y);
            if (t != null)
            {
                if (t.piece != null && t.piece.team == playerTeam)
                {
                    selectedPiece = t.piece;
                }
                else if (selectedPiece != null)
                {
                    foreach (var rule in selectedPiece.prototype.movementRules)
                    {
                        if (rule.AppliesTo(selectedPiece) && rule.AllowsMove(selectedPiece, x, y))
                        {
                            world.turnProcessor.AddMoveForTeam(playerTeam, new Move(world, selectedPiece) { targetX = x, targetY = y });
                            selectedPiece = null;
                            break;
                        }
                    }
                }
            }
        }

    }

    internal void RenderLayer(ICanvas canvas, Camera camera, int layer)
    {
        if (layer == 0)
        {
            if (selectedPiece != null)
            {
                canvas.Stroke(Color.LightBlue);
                canvas.StrokeWidth(.05f);
                canvas.DrawRect(selectedPiece.LocationX, selectedPiece.LocationY, .8f, .8f, Alignment.Center);


            }
        }
    }
}