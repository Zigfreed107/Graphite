using CadApp.Core.Entities;
using System;

namespace CadApp.Core.Selection;

public class SelectionManager
{

    public CadEntity? SelectedEntity { get; private set; }

    public event Action<CadEntity?>? SelectionChanged;

    public void Select(CadEntity? entity)
    {
        SelectedEntity = entity;
        SelectionChanged?.Invoke(entity);
    }

    public void Clear()
    {
        Select(null);
    }
}