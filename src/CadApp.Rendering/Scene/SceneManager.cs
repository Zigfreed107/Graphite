using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Rendering.EntityRenderers;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Media.Media3D;

namespace CadApp.Rendering.Scene;

public class SceneManager
{
    private readonly HelixViewport3D _viewport;
    private readonly CadDocument _document;
    private readonly Dictionary<Visual3D, CadEntity> _visualToEntity = new();
    public SceneManager(HelixViewport3D viewport, CadDocument document)
    {
        _viewport = viewport;
        _document = document;

        _document.Entities.CollectionChanged += OnEntitiesChanged;

        RenderAll();
    }

    private void OnEntitiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenderAll();
    }

    private void RenderAll()
    {
        _viewport.Children.Clear();
        _visualToEntity.Clear();

        _viewport.Children.Add(new SunLight());

        foreach (var entity in _document.Entities)
        {
            var visual = CreateVisual(entity);

            if (visual != null)
            {
                _viewport.Children.Add(visual);
                _visualToEntity[visual] = entity;
            }
        }
    }

    public CadEntity? GetEntityFromVisual(Visual3D visual)
    {
        if (_visualToEntity.TryGetValue(visual, out var entity))
            return entity;

        return null;
    }

    private Visual3D? CreateVisual(CadEntity entity)
    {
        if (entity is LineEntity line)
            return LineRenderer.Create(line);

        return null;
    }
}