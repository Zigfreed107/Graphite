using CadApp.Core.Entities;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CadApp.Rendering.EntityRenderers;

public static class LineRenderer
{
    public static LinesVisual3D Create(LineEntity line)
    {
        var visual = new LinesVisual3D
        {
            Color = Colors.Blue,
            Thickness = 2
        };

        visual.Points.Add(new Point3D(line.Start.X, line.Start.Y, line.Start.Z));
        visual.Points.Add(new Point3D(line.End.X, line.End.Y, line.End.Z));

        // Important: attach entity reference

        return visual;
    }
}