public partial class CrystalsLabel : Label
{
    public override void _Ready()
    {
        UpdateCrystalsAmount();
        CrystalManager.Instance.CrystalAmountChanged += UpdateCrystalsAmount;
    }
    
    private void UpdateCrystalsAmount()
    {
        Text = $"{CrystalManager.Instance.Crystals}";
    }
}
