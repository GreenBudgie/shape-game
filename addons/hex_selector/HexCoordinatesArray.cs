using System.Collections;
using System.Collections.Generic;
using Godot.Collections;

[GlobalClass]
public partial class HexCoordinatesArray : Resource, IEnumerable<HexCoordinates>
{
    [Export] private Array<Vector3I> _vectors = [];
    private HexCoordinates[]? _cache;

    public HexCoordinatesArray()
    {
        Changed += InvalidateCache;
    }

    internal Array<Vector3I> Vectors
    {
        get => _vectors;
        set
        {
            _vectors = value;
            InvalidateCache();
        }
    }

    public int Count => _vectors.Count;

    public IReadOnlyList<HexCoordinates> Coordinates
    {
        get
        {
            if (_cache != null) return _cache;
            var arr = new HexCoordinates[_vectors.Count];
            for (var i = 0; i < _vectors.Count; i++)
            {
                var v = _vectors[i];
                arr[i] = new HexCoordinates(v.X, v.Y, v.Z);
            }
            _cache = arr;
            return arr;
        }
    }

    public bool Contains(HexCoordinates coord) =>
        _vectors.Contains(new Vector3I(coord.Q, coord.R, coord.S));

    public IEnumerator<HexCoordinates> GetEnumerator() => Coordinates.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void InvalidateCache() => _cache = null;
}
