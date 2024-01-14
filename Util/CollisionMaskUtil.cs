using System.Linq;

namespace ShapeGame.Util;

public static class CollisionMaskUtil
{

    public static uint MaskWithLayers(params uint[] layers)
    {
        return layers.Aggregate((prevLayerBit, layerBit) => prevLayerBit + (uint) Pow(2, layerBit));
    }
    
}