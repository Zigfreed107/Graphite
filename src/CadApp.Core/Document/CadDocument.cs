// CadDocument.cs
// Owns the current CAD entity collection and document-level mutation helpers used by tools, rendering, and file workflows.
using CadApp.Core.Entities;
using CadApp.Core.Spatial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CadApp.Core.Document;

/// <summary>
/// Represents the editable CAD document that tools modify and renderers observe.
/// </summary>
public class CadDocument
{
    //TODO Spatial grid size is a const value. Make it specified in a config file somewhere?
    public ObservableCollection<CadEntity> Entities { get; } = new();
    public SpatialGrid SpatialGrid { get; }

    public CadDocument()
    {
        SpatialGrid = new SpatialGrid(1.0f);
    }

    /// <summary>
    /// Removes all entities while preserving per-entity collection notifications for renderers.
    /// </summary>
    public void ClearEntities()
    {
        for (int i = Entities.Count - 1; i >= 0; i--)
        {
            Entities.RemoveAt(i);
        }
    }

    /// <summary>
    /// Replaces the document contents while preserving collection notifications for scene synchronization.
    /// </summary>
    public void ReplaceEntities(IEnumerable<CadEntity> entities)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        List<CadEntity> replacementEntities = new List<CadEntity>(entities);

        ClearEntities();

        foreach (CadEntity entity in replacementEntities)
        {
            Entities.Add(entity);
        }
    }
}
