using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Rendering.EntityRenderers;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using System.Windows.Media;

namespace CadApp.Rendering.Scene;




public class SceneManager
{
    private readonly Viewport3DX _viewport;
    private readonly CadDocument _document;
    private readonly Dictionary<Element3D, CadEntity> _visualToEntity = new();
    private readonly GroupModel3D _entityRoot = new();  //Permanent CAD geometry
    private readonly GroupModel3D _previewRoot = new();

    public SceneManager(Viewport3DX viewport, CadDocument document)
    {
        _viewport = viewport;
        _document = document;

        var _grid = CreateGrid();
        _viewport.Items.Add(_grid);
        _viewport.Items.Add(_entityRoot);
        _viewport.Items.Add(_previewRoot);

        _document.Entities.CollectionChanged += OnEntitiesChanged;

        RenderAll();
    }

    private void OnEntitiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenderAll();
    }

    private void RenderAll()
    {
        _entityRoot.Children.Clear();
        _visualToEntity.Clear();

        foreach (var entity in _document.Entities)
        {
            var visual = CreateVisual(entity);

            if (visual != null)
            {
                _entityRoot.Children.Add(visual);
                _visualToEntity[visual] = entity;
            }
        }
    }

    public CadEntity? GetEntityFromVisual(Element3D visual)
    {
        if (_visualToEntity.TryGetValue(visual, out var entity))
            return entity;

        return null;
    }

    private Element3D? CreateVisual(CadEntity entity)
    {
        if (entity is LineEntity line)
            return LineRenderer.Create(line);

        return null;
    }

    private Element3D CreateGrid()
    {
        var builder = new LineBuilder();

        int size = 50;
        int step = 1;

        for (int i = -size; i <= size; i += step)
        {
            // vertical lines
            builder.AddLine(
                new Vector3(i, -size, 0),
                new Vector3(i, size, 0));

            // horizontal lines
            builder.AddLine(
                new Vector3(-size, i, 0),
                new Vector3(size, i, 0));
        }

        return new LineGeometryModel3D
        {
            Geometry = builder.ToLineGeometry3D(),
            Color = System.Windows.Media.Color.FromRgb(150,150,150),
            Thickness = 0.5f
        };
    }

    public void ShowPreviewLine(Vector3 start, Vector3 end)
    {
        _previewRoot.Children.Clear();

        var builder = new LineBuilder();
        builder.AddLine(start, end);

        var preview = new LineGeometryModel3D
        {
            Geometry = builder.ToLineGeometry3D(),
            Color = System.Windows.Media.Color.FromRgb(255,255,0),
            Thickness = 2
        };

        _previewRoot.Children.Add(preview);
    }

    public void ClearPreview()
    {
        _previewRoot.Children.Clear();
    }
}
