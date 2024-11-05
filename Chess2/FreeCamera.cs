using SimulationFramework.Input;
using SimulationFramework;
using System.Numerics;

namespace Chess2;

internal class FreeCamera : Camera
{
    public float zoom;

    public FreeCamera()
    {
        zoom = MathF.Log(10, 1.1f);
    }

    public override void Update(int width, int height)
    {
        base.Update(width, height);

        if (Mouse.ScrollWheelDelta != 0)
        {
            zoom -= Mouse.ScrollWheelDelta;

            Vector2 zoomTarget = this.ScreenToWorld(Mouse.Position, false);

            float zoomFac = MathF.Pow(1.1f, zoom);
            VerticalSize = zoomFac;

            Vector2 newZoomTarget = this.ScreenToWorld(Mouse.Position, false);

            this.Transform.Position += zoomTarget - newZoomTarget;
        }

        if (Mouse.IsButtonDown(MouseButton.Right))
        {
            tilt += 0.005f * Mouse.DeltaPosition.Y;
            tilt = Math.Clamp(tilt, MathF.PI / 12f, MathF.PI / 2f);

            Transform.Rotation += 0.005f * Mouse.DeltaPosition.X;
        }

        Vector2 delta = Vector2.Zero;

        if (Keyboard.IsKeyDown(Key.W))
            delta -= Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.A))
            delta -= Vector2.UnitX;
        if (Keyboard.IsKeyDown(Key.S))
            delta += Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.D))
            delta += Vector2.UnitX;

        Transform.Position += MathF.Pow(1.1f, zoom) * delta.Rotated(Transform.Rotation) * Time.DeltaTime;
    }
}
