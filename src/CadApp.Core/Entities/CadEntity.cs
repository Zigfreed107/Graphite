using CadApp.Core.Snapping;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CadApp.Core.Entities;

public abstract class CadEntity: ISelectable
{
    public Guid Id { get; protected set; }
    public abstract (Vector3 Min, Vector3 Max) GetBounds();

    public virtual IEnumerable<SnapPoint> GetSnapPoints()
    {
        yield break;
    }

    protected CadEntity()
    {
            Id = Guid.NewGuid();
    }
}