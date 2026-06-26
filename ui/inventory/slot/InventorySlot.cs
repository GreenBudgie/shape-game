public partial class InventorySlot : TextureButton
{

    public const float Inradius = 87;
    public const float InradiusSq = Inradius * Inradius;
    public const float Circumradius = 97;

    private const float GlowStartDistance = 600f;
    private const float GlowStopDistance = 100f;
    private const float GlowStartDistanceSq = GlowStartDistance * GlowStartDistance;
    private const float GlowMinStrength = 0.6f;
    private const float GlowMaxStrength = 1f;
    private const float GlowMaxRadius = 50f;

    private const float GlowHoverStrength = 1.5f;
    private const float GlowHoverRadius = 80f;
    private const float GlowHoverTweenDuration = 0.1f;
    private const float GlowUnhoverTweenDuration = 0.4f;
    private const float ButtonDownTweenDuration = 0.1f;
    private const float ButtonUpTweenDuration = 0.1f;
    private const float ButtonDownGlowStrength = 1.5f;
    private const float ButtonDownSize = 0.9f;
    private const float HoverSize = 1.15f;

    private static readonly AudioStream HoverSound = GD.Load<AudioStream>("uid://djdfrb1kcmfle");
    private static readonly AudioStream ClickSound = GD.Load<AudioStream>("uid://cry6gufmuccda");
    private static readonly AudioStream ButtonUpSound = GD.Load<AudioStream>("uid://cpqn2k806hyjd");

    public ModuleInventory Inventory { get; private set; } = null!;
    public HexCoordinates Coordinates { get; private set; }
    
    private InventoryModule? _module;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dilsv34jaqcrd");

    public static InventorySlot Create(ModuleInventory inventory, HexCoordinates coordinates)
    {
        var node = Scene.Instantiate<InventorySlot>();
        node.Inventory = inventory;
        node.Coordinates = coordinates;
        return node;
    }

    public InventoryModule? GetModule()
    {
        return _module;
    }

    public bool HasModule()
    {
        return GetModule() != null;
    }

    public Vector2 GetCenterPosition()
    {
        return GetGlobalRect().GetCenter();
    }

}