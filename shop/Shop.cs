public partial class ShopManager : Node2D
{
    public static ShopManager Instance { get; private set; } = null!;

    public ShopManager()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        HideShop();
        GamePhaseManager.Instance.PhaseChanged += OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            ShowShop();
        }
        else
        {
            HideShop();
        }
    }

    private void ShowShop()
    {
        Visible = true;
    }

    private void HideShop()
    {
        Visible = false;
    }
}