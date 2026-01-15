public partial class ShopModuleButton : Control
{
    
    private Button _button = null!;
    private TextureRect _buttonUnhovered = null!;
    private TextureRect _buttonHovered = null!;
    
    public override void _Ready()
    {
        _button = GetNode<Button>("Button");
        _buttonUnhovered = GetNode<TextureRect>("ButtonUnhovered");
        _buttonHovered = GetNode<TextureRect>("ButtonHovered");
    }
}
