using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;
internal class SpriteStack
{
    public ITexture texture;
    public int layerWidth, layerHeight;
    public int layers;

    public SpriteStack(ITexture texture, int layerWidth, int layerHeight)
    {
        this.texture = texture;
        this.layerWidth = layerWidth;
        this.layerHeight = layerHeight;
        layers = texture.Width / layerWidth;
        texture.WrapModeX = texture.WrapModeY = WrapMode.None;
    }

    public void RenderLayer(ICanvas canvas, Camera camera, float z, ColorF tint)
    {
        if (z < layers)
        {
            canvas.DrawTexture(
                    texture,
                    GetLayerSourceRect((int)z),
                    new Rectangle(0, 0, layerWidth / 16f, layerHeight / 16f, Alignment.Center),
                    tint
                    );
        }
    }

    public Rectangle GetLayerSourceRect(int layer)
    {
        return new Rectangle(layerWidth * layer, 0, layerWidth, layerHeight);
    }
}
