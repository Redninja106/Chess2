using SimulationFramework.Drawing;

internal class Actor
{
    private Transform transform = Transform.Default;

    public virtual ref Transform Transform => ref transform;

    public virtual void Update()
    {
    }

    public virtual void Render(ICanvas canvas)
    {
    }
}
