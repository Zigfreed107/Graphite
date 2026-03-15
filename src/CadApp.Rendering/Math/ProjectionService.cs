using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Numerics;
using System.Windows;

namespace CadApp.Rendering.Math;

public class ProjectionService
{
    private readonly Viewport3DX _viewport;
    private readonly Workplane _workplane = new();

    public ProjectionService(Viewport3DX viewport)
    {
        _viewport = viewport;
    }

    public bool TryGetWorldPoint(Point mousePos, out Vector3 worldPoint)
    {
        if (!_viewport.GetMouseRay(mousePos, out var rayOrigin, out var rayDir))
        {
            worldPoint = Vector3.Zero;
            return false;
        }

        return _workplane.IntersectRay(rayOrigin, rayDir, out worldPoint);
    }
}