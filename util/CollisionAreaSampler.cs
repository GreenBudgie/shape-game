using System;
using System.Collections.Generic;

/// <summary>
/// Picks uniformly distributed random points inside the enabled collision shapes of a CollisionObject2D.
/// Supports CollisionShape2D with rectangle, circle and convex polygon shapes, and CollisionPolygon2D in
/// solids build mode (Godot decomposes it into convex polygon shapes). Disabled shapes are ignored.
/// <br/><br/>
/// Shapes are read on the first request and cached. Call <see cref="Invalidate"/> after adding, removing,
/// enabling or disabling collision shapes of the object.
/// </summary>
public sealed class CollisionAreaSampler(CollisionObject2D collisionObject)
{

    /// <summary>
    /// Number of points in the texture returned by <see cref="GetEmissionPoints"/>.
    /// </summary>
    public const int EmissionPointCount = 512;

    private readonly List<Piece> _pieces = [];
    private float _totalArea;
    private bool _isBuilt;
    private ImageTexture? _emissionPoints;

    /// <summary>
    /// The object whose collision shapes are sampled.
    /// </summary>
    public CollisionObject2D CollisionObject => collisionObject;

    /// <summary>
    /// Total area of all sampled shapes, in local coordinates of the collision object.
    /// </summary>
    public float Area
    {
        get
        {
            EnsureBuilt();
            return _totalArea;
        }
    }

    /// <summary>
    /// Returns a random point inside the collision shapes, in local coordinates of the collision object.
    /// Use it to place children that should move and rotate together with the object.
    /// Returns <see cref="Vector2.Zero"/> if the object has no shapes with an area.
    /// </summary>
    public Vector2 GetRandomLocalPoint()
    {
        EnsureBuilt();
        if (_totalArea <= 0)
        {
            return Vector2.Zero;
        }

        // Bigger pieces must be picked more often, otherwise points crowd in small pieces
        var target = GD.Randf() * _totalArea;
        foreach (var piece in _pieces)
        {
            target -= piece.Area;
            if (target <= 0)
            {
                return piece.GetRandomPoint();
            }
        }

        // Float rounding can leave a tiny remainder after the last piece
        return _pieces[^1].GetRandomPoint();
    }

    /// <summary>
    /// Returns a random point inside the collision shapes, in global coordinates.
    /// Returns the global position of the object if it has no shapes with an area.
    /// </summary>
    public Vector2 GetRandomGlobalPoint()
    {
        return collisionObject.ToGlobal(GetRandomLocalPoint());
    }

    /// <summary>
    /// Returns a texture with <see cref="EmissionPointCount"/> random points inside the collision shapes,
    /// in local coordinates of the collision object. It is meant for
    /// <see cref="ParticleProcessMaterial.EmissionPointTexture"/>: one point per pixel, X in the red channel
    /// and Y in the green channel of an RGF image, the same layout Godot editor uses for generated emission points.
    /// <br/><br/>
    /// The texture is created on the first request and cached.
    /// Returns null if the object has no shapes with an area.
    /// </summary>
    public ImageTexture? GetEmissionPoints()
    {
        if (_emissionPoints != null)
        {
            return _emissionPoints;
        }

        EnsureBuilt();
        if (_totalArea <= 0)
        {
            return null;
        }

        var coordinates = new float[EmissionPointCount * 2];
        for (var i = 0; i < EmissionPointCount; i++)
        {
            var point = GetRandomLocalPoint();
            coordinates[i * 2] = point.X;
            coordinates[i * 2 + 1] = point.Y;
        }

        var data = new byte[coordinates.Length * sizeof(float)];
        Buffer.BlockCopy(coordinates, 0, data, 0, data.Length);

        var image = Image.CreateFromData(EmissionPointCount, 1, false, Image.Format.Rgf, data);
        _emissionPoints = ImageTexture.CreateFromImage(image);
        return _emissionPoints;
    }

    /// <summary>
    /// Forces the shapes to be read again on the next request.
    /// </summary>
    public void Invalidate()
    {
        _isBuilt = false;
        _emissionPoints = null;
    }

    private void EnsureBuilt()
    {
        if (_isBuilt)
        {
            return;
        }

        Build();
        _isBuilt = true;
    }

    private void Build()
    {
        _pieces.Clear();
        _totalArea = 0;

        foreach (var shapeOwnerId in collisionObject.GetShapeOwners())
        {
            var shapeOwnerUid = (uint)shapeOwnerId;
            if (collisionObject.IsShapeOwnerDisabled(shapeOwnerUid))
            {
                continue;
            }

            // Transform of the CollisionShape2D / CollisionPolygon2D node relative to the collision object
            var transform = collisionObject.ShapeOwnerGetTransform(shapeOwnerUid);
            for (var i = 0; i < collisionObject.ShapeOwnerGetShapeCount(shapeOwnerUid); i++)
            {
                AddShape(collisionObject.ShapeOwnerGetShape(shapeOwnerUid, i), transform);
            }
        }

        foreach (var piece in _pieces)
        {
            _totalArea += piece.Area;
        }
    }

    private void AddShape(Shape2D shape, Transform2D transform)
    {
        switch (shape)
        {
            case RectangleShape2D rectangleShape:
                var rect = rectangleShape.GetRect();
                AddConvexPolygon(
                    [
                        rect.Position,
                        new Vector2(rect.End.X, rect.Position.Y),
                        rect.End,
                        new Vector2(rect.Position.X, rect.End.Y),
                    ],
                    transform
                );
                break;
            case ConvexPolygonShape2D convexShape:
                AddConvexPolygon(convexShape.Points, transform);
                break;
            case CircleShape2D circleShape:
                _pieces.Add(new CirclePiece(circleShape.Radius, transform));
                break;
            default:
                GD.PushWarning($"Shape {shape.GetType().Name} is not supported by {nameof(CollisionAreaSampler)}");
                break;
        }
    }

    /// <summary>
    /// Splits a convex polygon into a fan of triangles around its first point.
    /// </summary>
    private void AddConvexPolygon(Vector2[] points, Transform2D transform)
    {
        for (var i = 1; i < points.Length - 1; i++)
        {
            _pieces.Add(new TrianglePiece(transform * points[0], transform * points[i], transform * points[i + 1]));
        }
    }

    private abstract class Piece
    {
        public abstract float Area { get; }

        public abstract Vector2 GetRandomPoint();
    }

    private sealed class TrianglePiece(Vector2 a, Vector2 b, Vector2 c) : Piece
    {
        public override float Area { get; } = Abs((b - a).Cross(c - a)) / 2;

        public override Vector2 GetRandomPoint()
        {
            var r1 = GD.Randf();
            var r2 = GD.Randf();

            // (r1, r2) is uniform in the parallelogram built on the triangle sides.
            // Points from its other half are mirrored back into the triangle.
            if (r1 + r2 > 1)
            {
                r1 = 1 - r1;
                r2 = 1 - r2;
            }

            return a + r1 * (b - a) + r2 * (c - a);
        }
    }

    private sealed class CirclePiece(float radius, Transform2D transform) : Piece
    {
        // The transform may scale the circle, the area scales by the determinant of its basis
        public override float Area { get; } = Pi * radius * radius * Abs(transform.X.Cross(transform.Y));

        public override Vector2 GetRandomPoint()
        {
            return transform * RandomUtils.RandomPointInRadius(radius);
        }
    }

}
