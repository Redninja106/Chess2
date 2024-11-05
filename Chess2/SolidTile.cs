using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;
internal class SolidTile : Tile
{
    ColorF color;

    public SolidTile(World world, ColorF color) : base(world)
    {
        this.color = color;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(color);
        canvas.DrawRect(0, 0, 1, 1, Alignment.Center);
    }

}
