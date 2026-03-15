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
}