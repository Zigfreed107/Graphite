using CadApp.Core.Snapping;
using System.Collections.Generic;
using System.Numerics;

namespace CadApp.Core.Entities;

public class LineEntity : CadEntity
{
    public Vector3 Start { get; set; }
    public Vector3 End { get; set; }

    public LineEntity(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public override IEnumerable<SnapPoint> GetSnapPoints()
    {
        yield return new SnapPoint
        {
            Position = Start,
            Type = SnapType.Endpoint
        };

        yield return new SnapPoint
        {
            Position = End,
            Type = SnapType.Endpoint
        };

        yield return new SnapPoint
        {
            Position = (Start + End) * 0.5f,
            Type = SnapType.Midpoint
        };
    }
}