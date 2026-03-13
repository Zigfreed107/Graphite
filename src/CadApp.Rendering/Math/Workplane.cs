
using System.Numerics;
using System.Windows;
using HelixToolkit.Wpf;

namespace CadApp.Rendering.Math;

public static class Workplane
{
    public static bool TryGetPointOnPlane(
        HelixViewport3D viewport,
        double x,
        double y,
        out Vector3 point)
    {
        var ray = viewport.Viewport.UnProject(new Point(x, y));

        var rayOrigin = ray.Origin;
        var rayDirection = ray.Direction;

        // XY plane (Z = 0)
        double planeZ = 0;

        if (System.Math.Abs(rayDirection.Z) < 0.0001)
        {
            point = default;
            return false;
        }

        double t = (planeZ - rayOrigin.Z) / rayDirection.Z;

        var hit = rayOrigin + rayDirection * t;

        point = new Vector3(
            (float)hit.X,
            (float)hit.Y,
            (float)hit.Z);

        return true;
    }
}