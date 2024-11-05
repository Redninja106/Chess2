using SimulationFramework;
using System.Numerics;
using SimulationFramework.Drawing;

namespace Chess2;

internal class Camera
{
    public Transform SmoothTransform = Transform.Default;
    public Transform Transform = Transform.Default;
    public float tilt = MathF.PI / 4f;

    public float YFactor => MathF.Sin(tilt);
    public float ZFactor => MathF.Cos(tilt);

    public int DisplayWidth { get; private set; }
    public int DisplayHeight { get; private set; }

    public float VerticalSize { get; set; } = 10;
    public float SmoothVerticalSize = 10;

    public float InterpolationFactor { get; set; } = 0.000001f;

    public float AspectRatio => DisplayWidth / DisplayHeight;

    public virtual void Update(int width, int height)
    {
        DisplayWidth = width;
        DisplayHeight = height;

        SmoothTransform.Position = Vector2.Lerp(SmoothTransform.Position, Transform.Position, 1f - MathF.Pow(InterpolationFactor, Time.DeltaTime));
        SmoothVerticalSize = float.Lerp(SmoothVerticalSize, VerticalSize, 1f - MathF.Pow(InterpolationFactor, Time.DeltaTime));
    }

    public void RenderSetup(ICanvas canvas)
    {
        canvas.Transform(CreateWorldToScreenMatrix());
        canvas.Clear(Color.FromHSV(0, 0, .05f));
    }

    public Matrix3x2 CreateWorldToScreenMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = CreateLocalToScreenMatrix(interpolated) * result;
        result = (interpolated ? SmoothTransform : Transform).CreateWorldToLocalMatrix() * result;
        return result;
    }

    public Matrix3x2 CreateScreenToWorldMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = (interpolated ? SmoothTransform : Transform).CreateLocalToWorldMatrix() * result;
        result = CreateScreenToLocalMatrix(interpolated) * result;
        return result;
    }

    public Matrix3x2 CreateLocalToScreenMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = Matrix3x2.CreateTranslation(DisplayWidth / 2f, DisplayHeight / 2f) * result;
        result = Matrix3x2.CreateScale(DisplayHeight / (interpolated ? SmoothVerticalSize : VerticalSize)) * result;
        return result;
    }

    public Matrix3x2 CreateScreenToLocalMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = Matrix3x2.CreateScale((interpolated ? SmoothVerticalSize : VerticalSize) / DisplayHeight) * result;
        result = Matrix3x2.CreateTranslation(-DisplayWidth / 2f, -DisplayHeight / 2f) * result;
        return result;
    }

    public Vector2 ScreenToWorld(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, CreateScreenToWorldMatrix(interpolated));
    }

    public Vector2 ScreenToLocal(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, CreateScreenToLocalMatrix(interpolated));
    }

    public Vector2 WorldToScreen(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, CreateWorldToScreenMatrix(interpolated));
    }

    public Vector2 LocalToScreen(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, CreateLocalToScreenMatrix(interpolated));
    }

    public float ScreenDistanceToWorldDistance(float distance)
    {
        return Vector2.Distance(ScreenToWorld(Vector2.Zero), ScreenToWorld(new(distance, 0)));
    }

    public Matrix3x2 CreatePlaneToLocalMatrix(float z)
    {
        Matrix3x2 matrix;
        matrix = Matrix3x2.CreateTranslation(0, z * ZFactor);
        matrix = Matrix3x2.CreateScale(1, YFactor) * matrix;
        matrix = Matrix3x2.CreateRotation(-Transform.Rotation) * matrix;
        matrix = Matrix3x2.CreateTranslation(-SmoothTransform.Position.X, -SmoothTransform.Position.Y) * matrix;
        return matrix;
    }

    public Matrix3x2 CreateLocalToPlaneMatrix(float z)
    {
        Matrix3x2 matrix;
        matrix = Matrix3x2.CreateTranslation(SmoothTransform.Position.X, SmoothTransform.Position.Y);
        matrix = Matrix3x2.CreateRotation(Transform.Rotation) * matrix;
        matrix = Matrix3x2.CreateScale(1, 1f / YFactor) * matrix;
        matrix = Matrix3x2.CreateTranslation(0, -z * ZFactor) * matrix;
        return matrix;
    }
}
