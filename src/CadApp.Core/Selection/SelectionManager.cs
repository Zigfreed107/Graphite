using CadApp.Core.Entities;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages selection state for all selectable entities.
/// This class is part of the domain layer and contains no rendering logic.
/// </summary>
public class SelectionManager
{
    /// <summary>
    /// Currently selected entities.
    /// </summary>
    private readonly HashSet<Guid> _selectedEntityIds = new HashSet<Guid>();

    /// <summary>
    /// Fired when selection changes.
    /// Provides delta changes for efficient rendering updates.
    /// </summary>
    public event Action<IEnumerable<Guid>, IEnumerable<Guid>>? SelectionChanged;

    /// <summary>
    /// Select a single entity (clears previous selection).
    /// </summary>
    public void SelectSingle(CadEntity entity)
    {
        List<Guid> removed = new List<Guid>(_selectedEntityIds);
        List<Guid> added = new List<Guid>();

        _selectedEntityIds.Clear();

        if (!_selectedEntityIds.Contains(entity.CadEntityId))
        {
            _selectedEntityIds.Add(entity.CadEntityId);
            added.Add(entity.CadEntityId);
        }

        SelectionChanged?.Invoke(added, removed);
    }

    /// <summary>
    /// Add entity to selection (for multi-select later).
    /// </summary>
    public void AddToSelection(CadEntity entity)
    {
        _selectedEntityIds.Add(entity.CadEntityId);

        SelectionChanged?.Invoke(_selectedEntityIds, new List<Guid>());
    }

    /// <summary>
    /// Deselect all entities.
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedEntityIds.Count == 0)
            return;

        List<Guid> removed = new List<Guid>(_selectedEntityIds);

        _selectedEntityIds.Clear();

        SelectionChanged?.Invoke(new List<Guid>(), removed);
    }

    /// <summary>
    /// Check if an entity is selected.
    /// </summary>
    public bool IsSelected(Guid entityId)
    {
        return _selectedEntityIds.Contains(entityId);
    }
}