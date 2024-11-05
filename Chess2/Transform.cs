using ImGuiNET;
using SimulationFramework;
using SimulationFramework.Drawing;
using System.Numerics;

public struct Transform
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public Vector2 Forward => Angle.ToVector(Rotation);

    public static readonly Transform Default = new();

    public Transform()
    {
    }

    public Matrix3x2 CreateWorldToLocalMatrix()
    {
        return
            Matrix3x2.CreateTranslation(-Position) *
            Matrix3x2.CreateRotation(-Rotation) *
            Matrix3x2.CreateScale(1f / Scale.X, 1f / Scale.Y);
    }

    public Matrix3x2 CreateLocalToWorldMatrix()
    {
        return
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateRotation(Rotation) *
            Matrix3x2.CreateTranslation(Position);
    }

    public void Layout()
    {
        ImGui.DragFloat2("Position", ref Position);
        ImGui.SliderAngle("Rotation", ref Rotation);
        ImGui.DragFloat2("Scale", ref Scale);
    }

    public void ApplyTo(ICanvas canvas)
    {
        canvas.Transform(this.CreateLocalToWorldMatrix());
    }

    public Vector2 WorldToLocal(Vector2 point)
    {
        return Vector2.Transform(point, CreateWorldToLocalMatrix());
    }

    public Vector2 LocalToWorld(Vector2 point)
    {
        return Vector2.Transform(point, CreateLocalToWorldMatrix());
    }

    public Transform Rotated(float angle)
    {
        return this with { Rotation = Rotation + angle };
    }

    public Transform Translated(Vector2 translation)
    {
        return this with { Position = Position + translation };
    }

    public Transform Scaled(float scale)
    {
        return this with { Scale = Scale * scale };
    }

    public float Distance(Transform transform)
    {
        return Vector2.Distance(this.Position, transform.Position);
    }
}