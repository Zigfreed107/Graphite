using System;

namespace CadApp.Core.Entities;

public abstract class CadEntity
{
    public Guid Id { get; } = Guid.NewGuid();
}