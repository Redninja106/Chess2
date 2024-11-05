using ImGuiNET;
using SimulationFramework.Desktop;
using SimulationFramework.Drawing.Shaders.Compiler.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;

internal class SpriteStackEditor : IDisposable
{
    private static SpriteStackEditor? instance;

    public static void Edit(SpriteStack spriteStack, string? knownName = null)
    {
        ArgumentNullException.ThrowIfNull(spriteStack);
        if (instance?.spriteStack != spriteStack)
        {
            instance?.Dispose();
            instance = new(spriteStack, knownName);
            ImGui.SetNextWindowFocus();
        }
    }

    public static void Layout()
    {
        instance?.LayoutInternal();
    }

    private SpriteStack spriteStack;
    private int layer;
    private ITexture rt;
    private Camera camera;
    private float zoom = 10;
    private Color color = Color.White;
    private Color altColor = Color.Transparent;
    private float zOffset;
    private const float gridRadius = 1;
    private static RadialGradient radialGradient = new(0, 0, gridRadius, ColorF.DarkGray, ColorF.DarkGray with { A = 0 });
    private Color[] copiedLayer;
    private bool showAbove = true;
    private bool showBelow = true;
    private string saveFileName = "sprite.png";
    private bool saveToProject = true;

    private SpriteStackEditor(SpriteStack spriteStack, string? knownName)
    {
        this.saveFileName = knownName ?? "";
        this.spriteStack = spriteStack;
        rt = Graphics.CreateTexture(600, 600);
        camera = new();
        camera.Transform.Rotation = MathF.PI / 4f;
        copiedLayer = new Color[spriteStack.layerWidth * spriteStack.layerHeight];
    }

    private void LayoutInternal()
    {
        if (ImGui.Begin("Sprite Stack Editor", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.MenuBar))
        {

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("save"))
                {
                    ImGui.OpenPopup("savemenu");
                }

                bool open = true;
                if (ImGui.BeginPopupModal("savemenu", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
                {
                    try
                    {
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Assets/");
                        ImGui.SameLine();
                        ImGui.InputText(".png##name", ref saveFileName, 256);

                        ImGui.Checkbox("Save To Project", ref saveToProject);
                        string name = Path.Combine("Assets/", saveFileName + ".png");
                        if (saveToProject)
                        {
                            name = "./../../../" + name;
                        }
                        else
                        {
                            name = "./" + name;
                        }
                        ImGui.Text("Will save to " + Path.GetFullPath(name));

                        if (ImGui.Button("save"))
                        {
                            spriteStack.texture.Encode(name);
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("cancel"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    ImGui.SetWindowPos(ImGui.GetMainViewport().Size * .5f - ImGui.GetWindowSize() * .5f);
                    ImGui.EndPopup();
                }

                if (ImGui.BeginMenu("layer"))
                {
                    if (ImGui.Selectable("copy"))
                    {
                        for (int y = 0; y < spriteStack.layerHeight; y++)
                        {
                            for (int x = 0; x < spriteStack.layerWidth; x++)
                            {
                                copiedLayer[y * spriteStack.layerWidth + x] = spriteStack.texture[x + layer * spriteStack.layerWidth, y];
                            }
                        }
                    }

                    if (ImGui.Selectable("paste"))
                    {
                        for (int y = 0; y < spriteStack.layerHeight; y++)
                        {
                            for (int x = 0; x < spriteStack.layerWidth; x++)
                            {
                                spriteStack.texture[x + layer * spriteStack.layerWidth, y] = copiedLayer[y * spriteStack.layerWidth + x];
                                spriteStack.texture.ApplyChanges();
                            }
                        }
                    }

                    if (ImGui.Selectable("rotate left"))
                    {
                        TransformLayer(Matrix3x2.CreateRotation(-MathF.PI / 2f, new(8, 8)));
                    }

                    if (ImGui.Selectable("rotate right"))
                    {
                        TransformLayer(Matrix3x2.CreateRotation(MathF.PI / 2f, new(8, 8)));
                    }

                    if (ImGui.Selectable("flip X"))
                    {
                        TransformLayer(Matrix3x2.CreateScale(new Vector2(-1, 1), new Vector2(8, 8)));
                    }

                    if (ImGui.Selectable("flip Y"))
                    {
                        TransformLayer(Matrix3x2.CreateScale(new Vector2(1, -1), new Vector2(8, 8)));
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("view"))
                {
                    ImGui.Checkbox("show layers above", ref showAbove);
                    ImGui.Checkbox("show layers below", ref showBelow);
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            ImGui.SliderInt("layer", ref layer, 0, spriteStack.layers - 1);
            ImGui.SameLine();

            Vector4 col = color.ToColorF().ToVector4();
            ImGui.ColorEdit4("color", ref col, ImGuiColorEditFlags.DisplayHex);
            color = new ColorF(col).ToColor();
            ImGui.SameLine();

            Vector4 altCol = altColor.ToColorF().ToVector4();
            ImGui.ColorEdit4("alt color", ref altCol, ImGuiColorEditFlags.DisplayHex);
            altColor = new ColorF(altCol).ToColor();

            Update();

            var canvas = rt.GetCanvas();
            canvas.Clear(Color.Black);
            canvas.ResetState();
            canvas.Transform(camera.CreateLocalToScreenMatrix());

            bool isAbove = false;
            for (int i = 0; i < spriteStack.layers; i++)
            {
                canvas.PushState();
                canvas.Transform(camera.CreatePlaneToLocalMatrix((layer - i) * 1/16f + zOffset));
                if (i == layer)
                {
                    spriteStack.RenderLayer(canvas, camera, i, ColorF.White);
                    canvas.Stroke(radialGradient);
                    int lines = (int)MathF.Ceiling(gridRadius) * 16;
                    for (int x = -lines; x < lines; x++)
                    {
                        canvas.DrawLine(x / 16f, -3, x / 16f, 3);
                    }
                    for (int y = -lines; y < lines; y++)
                    {
                        canvas.DrawLine(-3, y / 16f, 3, y / 16f);
                    }
                    canvas.Stroke(ColorF.Gray);

                    canvas.DrawLine(0, 0, 0, 1);
                    canvas.DrawLine(0, 0, 0, -1);
                    canvas.DrawLine(0, 0, 1, 0);
                    canvas.DrawLine(0, 0, -1, 0);

                    canvas.DrawAlignedText("+Y", .1f, 0, 1, Alignment.Center);
                    canvas.DrawAlignedText("-Y", .1f, 0, -1, Alignment.Center);
                    canvas.DrawAlignedText("+X", .1f, 1, 0, Alignment.Center);
                    canvas.DrawAlignedText("-X", .1f, -1, 0, Alignment.Center);

                    canvas.Stroke(ColorF.White);
                    canvas.StrokeWidth(0.00f);
                    canvas.DrawRect(0, 0, spriteStack.layerWidth / 16f, spriteStack.layerHeight / 16f, Alignment.Center);
                    isAbove = true;
                }
                else
                {
                    if ((isAbove && showAbove) || (!isAbove && showBelow))
                    {
                        spriteStack.RenderLayer(canvas, camera, i, ColorF.White with { A = .2f });
                    }
                }

                canvas.PopState();
            }

            canvas.Flush();

            var cursorPos = ImGui.GetCursorPos() - new Vector2(ImGui.GetStyle().WindowBorderSize);

            ImGui.SetCursorPos(cursorPos);
            ImGui.BeginDisabled();
            ImGui.InvisibleButton("button", new(rt.Width, rt.Height));
            ImGui.EndDisabled();
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(rt.GetImGuiID(), new(rt.Width, rt.Height));
        }
        ImGui.End();
    }

    private void RotateLayer(float angle)
    {
        Color[] buffer = new Color[spriteStack.layerWidth * spriteStack.layerHeight];
        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                buffer[y * spriteStack.layerWidth + x] = spriteStack.texture[x + layer * spriteStack.layerWidth, y];
            }
        }

        Vector2 center = new(spriteStack.layerWidth * .5f, spriteStack.layerHeight * .5f);

        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                Vector2 position = new Vector2(x, y) - center;
                position = position.Rotated(angle) + center;
                int newX = (int)position.X;
                int newY = (int)position.Y;
                spriteStack.texture[newX + layer * spriteStack.layerWidth, newY] = buffer[y * spriteStack.layerWidth + x];
            }
        }

        spriteStack.texture.ApplyChanges();
    }

    private void RearrangeLayer(Func<(int x, int y), (int x, int y)> coordinateMap)
    {
        Color[] buffer = new Color[spriteStack.layerWidth * spriteStack.layerHeight];

        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                var (bufferX, bufferY) = coordinateMap((x, y)); 
                buffer[bufferY * spriteStack.layerWidth + bufferX] = spriteStack.texture[x + layer * spriteStack.layerWidth, y];
            }
        }

        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                spriteStack.texture[x + layer * spriteStack.layerWidth, y] = buffer[y * spriteStack.layerWidth + x];
            }
        }
    }

    private void TransformLayer(Matrix3x2 transform)
    {
        Color[] buffer = new Color[spriteStack.layerWidth * spriteStack.layerHeight];

        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                buffer[y * spriteStack.layerWidth + x] = spriteStack.texture[x + layer * spriteStack.layerWidth, y];
            }
        }

        for (int y = 0; y < spriteStack.layerHeight; y++)
        {
            for (int x = 0; x < spriteStack.layerWidth; x++)
            {
                Vector2 newCoordinate = Vector2.Transform(new(x + .5f, y + .5f), transform);
                int newX = (int)newCoordinate.X;
                int newY = (int)newCoordinate.Y;

                spriteStack.texture[newX + layer * spriteStack.layerWidth, newY] = buffer[y * spriteStack.layerWidth + x];
            }
        }

        spriteStack.texture.ApplyChanges();
    }

    private void Update()
    {
        camera.Update(rt.Width, rt.Height);

        if (ImGui.IsWindowHovered())
        {
            var io = ImGui.GetIO();
            if (Keyboard.IsKeyDown(Key.LeftControl))
            {
                layer += (int)io.MouseWheel;
                layer = Math.Clamp(layer, 0, spriteStack.layers - 1);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                zOffset += 0.05f * io.MouseWheel;
            }
            else 
            {
                zoom -= ImGui.GetIO().MouseWheel;
                camera.VerticalSize = MathF.Pow(1.1f, zoom);
            }

            if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
            {
                camera.Transform.Rotation += 0.005f * Mouse.DeltaPosition.X;
                camera.tilt += 0.005f * Mouse.DeltaPosition.Y;
                camera.tilt = Math.Clamp(camera.tilt, Angle.ToRadians(45f), Angle.ToRadians(90f));
            }

            if (io.MouseDown[0])
            {
                var localPosition = camera.ScreenToLocal(io.MousePos - ImGui.GetCursorScreenPos());
                var matrix = camera.CreateLocalToPlaneMatrix(zOffset);
                var mousePosition = Vector2.Transform(localPosition, matrix);
                int x = (int)MathF.Floor((mousePosition.X * 16f) + (.5f * spriteStack.layerWidth));
                int y = (int)MathF.Floor((mousePosition.Y * 16f) + (.5f * spriteStack.layerHeight));
                if (x >= 0 && x < spriteStack.layerWidth && y >= 0 && y < spriteStack.layerHeight)
                {
                    if (Keyboard.IsKeyDown(Key.LeftControl))
                    {
                        Color col = spriteStack.texture[x + layer * spriteStack.layerWidth, y];
                        if (Keyboard.IsKeyDown(Key.LeftAlt))
                        {
                            altColor = col;
                        }
                        else
                        {
                            color = col;
                        }
                    }
                    else
                    {
                        Color col = Keyboard.IsKeyDown(Key.LeftAlt) ? altColor : color;
                        spriteStack.texture[x + layer * spriteStack.layerWidth, y] = col;
                        spriteStack.texture.ApplyChanges();
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        rt.Dispose();
    }
}
internal class SpriteStackManager
{
    internal Dictionary<string, SpriteStack> spriteStacks = [];


    public SpriteStack GetOrLoad(string name)
    {
        if (!spriteStacks.TryGetValue(name, out SpriteStack? value))
        {
            string path = "Assets/" + name + ".png";

#if DEBUG
            path = "./../../../" + path;
#endif

            var texture = Graphics.LoadTexture(path);
            value = new(texture, texture.Height, texture.Height);
            spriteStacks[name] = value;
        }
        return value;
    }

    public void Register(string name, SpriteStack stack)
    {
        spriteStacks.Add(name, stack);
    }

}
static class SpriteStackViewer
{
    private static bool open;
    private static string name = "";
    private static int width;
    private static int height;
    private static int layers;

    public static void Layout()
    {
        if (Keyboard.IsKeyPressed(Key.F1))
        {
            open = !open;
        }

        if (open && ImGui.Begin("Sprite List", ref open, ImGuiWindowFlags.MenuBar))
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.Selectable("Create New"))
                {
                    ImGui.OpenPopup("createmenu");
                }

                bool open = true;
                if (ImGui.BeginPopupModal("createmenu", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
                {
                    ImGui.InputText("name", ref name, 256);
                    ImGui.InputInt("width", ref width);
                    ImGui.InputInt("height", ref height);
                    ImGui.InputInt("layers", ref layers);

                    if (ImGui.Button("create"))
                    {
                        var texture = Graphics.CreateTexture(width * layers, height);
                        var spriteStack = new SpriteStack(texture, width, height);
                        Program.spriteManager.Register(name, spriteStack);
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SetWindowPos(.5f * (ImGui.GetMainViewport().Size - ImGui.GetWindowSize()));
                    ImGui.EndPopup();
                }

                ImGui.EndMenuBar();
            }

            if (ImGui.BeginListBox(""))
            {
                foreach (var (name, spriteStack) in Program.spriteManager.spriteStacks)
                {
                    if (ImGui.Selectable(name))
                    {
                        SpriteStackEditor.Edit(spriteStack, name);
                    }
                }
            }
        }
        ImGui.End();
    }
}
