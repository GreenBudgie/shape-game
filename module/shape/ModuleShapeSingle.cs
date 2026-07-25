using System.Collections.Generic;

[GlobalClass]
[Icon(OutlineTexturePath)]
public partial class ModuleShapeSingle : ModuleShape
{
    
    private const string OutlineTexturePath = "uid://bwxthnwovlmpr";
    private const string FillTexturePath = "uid://bv0dh5lkm5vq6";
    
    public override Texture2D OutlineTexture => GD.Load<Texture2D>(OutlineTexturePath);
    public override Texture2D FillTexture => GD.Load<Texture2D>(FillTexturePath);
    
    public override List<HexCoordinates> Hexes => [HexCoordinates.Zero];
}