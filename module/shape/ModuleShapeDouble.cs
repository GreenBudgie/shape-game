using System.Collections.Generic;

[GlobalClass]
[Icon(OutlineTexturePath)]
public partial class ModuleShapeDouble : ModuleShape
{
    private const string OutlineTexturePath = "uid://ow22ikgpvcsx";
    private const string FillTexturePath = "uid://i744v83kyulk";

    public override Texture2D OutlineTexture => GD.Load<Texture2D>(OutlineTexturePath);
    public override Texture2D FillTexture => GD.Load<Texture2D>(FillTexturePath);

    public override List<HexCoordinates> Hexes => [HexCoordinates.Zero, HexCoordinates.Right];
}