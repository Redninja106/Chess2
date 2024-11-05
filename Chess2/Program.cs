using Chess2;
using SimulationFramework;
using SimulationFramework.Drawing;

Start<Program>();

partial class Program : Simulation
{
    public static SpriteStackManager spriteManager = new();

    public static PiecePrototype RookPrototype = new()
    {
        spriteStack = spriteManager.GetOrLoad("rook"),
        movementRules = [
            new(1, 0, MovementRuleFlags.Repeat),
            new(0, 1, MovementRuleFlags.Repeat),
            new(-1, 0, MovementRuleFlags.Repeat),
            new(0, -1, MovementRuleFlags.Repeat),
            ],
    }; 
    
    public static PiecePrototype PawnPrototype = new()
    {
        spriteStack = spriteManager.GetOrLoad("pawn"),
        movementRules = [
            new(0, 1, MovementRuleFlags.CannotAttack),
            new(0, 2, MovementRuleFlags.CannotHaveMoved),
            new(1, 1, MovementRuleFlags.MustAttack),
            new(-1, 1, MovementRuleFlags.MustAttack),
            ],
    };


    World world = new();
    Team white = new() { color = ColorF.Lerp(ColorF.White, ColorF.Blue, .1f), orientation = 0 };
    Team black = new() { color = ColorF.Lerp(ColorF.DarkGray, ColorF.DarkRed, .1f), orientation = 2 };

    public override void OnInitialize()
    {
        world.camera = new FreeCamera();

        world.mover.playerTeam = white;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                world.board.SetTile(x, y, new SolidTile(world, (x + y) % 2 == 0 ? ColorF.White : ColorF.Black) { height = Random.Shared.NextSingle() * .05f });
            }
        }

        world.board.AddPiece(0, 0, RookPrototype, white);
        world.board.AddPiece(7, 0, RookPrototype, white);

        for (int i = 0; i < 8; i++)
        {
            world.board.AddPiece(i, 1, PawnPrototype, white);
        }

        world.board.AddPiece(0, 7, RookPrototype, black);
        world.board.AddPiece(7, 7, RookPrototype, black);

        for (int i = 0; i < 8; i++)
        {
            world.board.AddPiece(i, 6, PawnPrototype, black);
        }
    }

    public override void OnRender(ICanvas canvas)
    {
        SpriteStackViewer.Layout();
        SpriteStackEditor.Layout();

        world.Update(canvas.Width, canvas.Height);

        world.Render(canvas);

    }
}
