using CadApp.Core.Snapping;
using System;
using System.Collections.Generic;

namespace CadApp.Core.Entities;

public abstract class CadEntity
{
    public Guid Id { get; } = Guid.NewGuid();

    public virtual IEnumerable<SnapPoint> GetSnapPoints()
    {
        yield break;
    }
}