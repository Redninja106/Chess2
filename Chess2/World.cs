using SimulationFramework.Drawing;
using SimulationFramework.Input;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Chess2;

class World
{
    public Board board;

    public Vector2 MousePosition = Vector2.Zero;

    public Camera camera;
    private IMask? mask;
    public PieceMover mover;
    public TurnProcessor turnProcessor;

    int layers = 32;

    public World()
    {
        mover = new(this);
        turnProcessor = new();
        board = new(this);
    }

    public void Render(ICanvas canvas)
    {
        if (mask is null || mask.Width != canvas.Width || mask.Height != canvas.Height)
        {
            mask?.Dispose();
            mask = Graphics.CreateMask(canvas.Width, canvas.Height);
        }

        canvas.Clear(Color.FromHSV(0, 0, .1f));
        mask.Clear(true);

        canvas.Transform(camera.CreateLocalToScreenMatrix());

        canvas.Mask(mask);
        canvas.WriteMask(mask, false);

        for (int i = layers - 1; i >= 0; i--)
        {
            canvas.PushState();

            int sublayers = Math.Clamp((int)(16 * camera.ZFactor), 1, 16);
            float step = 1f / sublayers;
            for (int j = sublayers - 1; j >= 0; j--)
            {
                canvas.PushState();
                float z = (-i + -j * step) * 1 / 16f;
                canvas.Transform(camera.CreatePlaneToLocalMatrix(z));
                RenderLayer(canvas, camera, i, z);
                canvas.PopState();
            }
            canvas.PopState();
        }

        // foreach (var (pt, tile) in tiles)
        // {
        //     canvas.PushState();
        //     canvas.Translate(pt.x, pt.y);
        //     tile.Render(canvas);
        //     canvas.PopState();
        // }
        // foreach (var piece in Pieces)
        // {
        //     piece.Render(canvas);
        // }
    }

    private void RenderLayer(ICanvas canvas, Camera camera, int layer, float z)
    {
        foreach (var (pt, tile) in board.tiles)
        {
            if (-z <= tile.height)
            {
                canvas.PushState();
                canvas.Translate(pt.x, pt.y);

                tile.Render(canvas);

                canvas.PopState();
            }
        }
        foreach (var piece in board.pieces)
        {

            canvas.PushState();
            canvas.Translate(piece.LocationX, piece.LocationY);
            piece.prototype.spriteStack.RenderLayer(canvas, camera, layer, piece.team.color);
            canvas.PopState();
        }
        mover.RenderLayer(canvas, camera, layer);
    }

    public void Update(int displayWidth, int displayHeight)
    {
        camera.Update(displayWidth, displayHeight);

        mover.Update();

        turnProcessor.FinishTurn();

        foreach (var piece in board.pieces)
        {
            piece.Update();
        }
    }
}
