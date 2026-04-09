using CadApp.Core.Entities;
using CadApp.Core.Snapping;
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// Represents a straight line entity defined by a start and end point in 3D space. Contains core CAD logic, but not rendering.
/// </summary>
public class LineEntity : CadEntity, ISnapProvider
{
    public Vector3 Start { get; }
    public Vector3 End { get; }

    public LineEntity(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public void GetSnapPoints(List<SnapPoint> snapPoints)
    {
        // Endpoints
        snapPoints.Add(new SnapPoint(Start, SnapType.Endpoint));
        snapPoints.Add(new SnapPoint(End, SnapType.Endpoint));

        // Midpoint
        var mid = (Start + End) * 0.5f;
        snapPoints.Add(new SnapPoint(mid, SnapType.Midpoint));
    }

    public override (Vector3 Min, Vector3 Max) GetBounds()
    {
        Vector3 min = Vector3.Min(Start, End);
        Vector3 max = Vector3.Max(Start, End);
        return (min, max);
    }
}