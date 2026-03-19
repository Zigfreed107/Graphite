using System.Numerics;

namespace CadApp.Core.Snapping;

public enum SnapType
{
    Endpoint,
    Midpoint,
    Center
}

public class SnapPoint
{
    public Vector3 Position { get; set; }
    public SnapType Type { get; set; }
}