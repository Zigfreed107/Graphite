using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Core.Spatial;
using CadApp.Rendering.EntityRenderers;
using HelixToolkit.Geometry;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SharpDX;

namespace CadApp.Rendering.Scene;

TODO the way the preview line is redrawn and has to be deleted from previewRoot by name, so as not to delete the snap marker, is a bit hacky. Consider a better way to manage preview elements, maybe by keeping references to them instead of searching by name. Also consider using a single reusable preview line visual instead of creating/removing each time for better performance.


public class SceneManager
{
    private readonly Viewport3DX _viewport;
    private readonly CadDocument _document;
    private readonly Dictionary<Element3D, CadEntity> _visualToEntity = new();
    private readonly GroupModel3D _entityRoot = new();  //Permanent CAD geometry
    private readonly GroupModel3D _previewRoot = new();
    private readonly MeshGeometryModel3D _snapMarker;
    
    public SceneManager(Viewport3DX viewport, CadDocument document)
    {
        _viewport = viewport;
        _document = document;

        var _grid = CreateGrid();
        _viewport.Items.Add(_grid);
        _viewport.Items.Add(_entityRoot);
        _viewport.Items.Add(_previewRoot);

        var builder = new MeshBuilder();
        builder.AddSphere(new Vector3(0, 0, 0), 0.2f); // make it BIG for testing

        _snapMarker = new MeshGeometryModel3D
        {
            Geometry = builder.ToMeshGeometry3D(),
            Material = new PhongMaterial
            {
                DiffuseColor = new Color4(1f, 0f, 0f, 1f), // red
                AmbientColor = new Color4(1f, 0f, 0f, 1f)

            },
            Name = "SnapMarker",
            CullMode = SharpDX.Direct3D11.CullMode.None, // IMPORTANT
            Visibility = Visibility.Visible
        };

        _previewRoot.Children.Add(_snapMarker);



        _document.Entities.CollectionChanged += OnEntitiesChanged;

        RenderAll();
    }

    private void OnEntitiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CadEntity entity in e.NewItems)
            {
                InsertEntity(entity);
            }
        }

        if (e.OldItems != null)
        {
            foreach (CadEntity entity in e.OldItems)
            {
                RemoveEntity(entity);
            }
        }

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

    //TODO : optimize by reusing a single preview line visual instead of creating/removing each time
    public void ShowPreviewLine(Vector3 start, Vector3 end)
    {
        // Find existing preview named "PreviewLine" without modifying the collection during enumeration
        Element3D? existingPreview = null;
        foreach (var child in _previewRoot.Children)
        {
            if (child is not null && child.Name == "PreviewLine")
            {
                existingPreview = child;
                break;
            }
        }

        if (existingPreview != null)
        {
            _previewRoot.Children.Remove(existingPreview);
        }

        var builder = new LineBuilder();
        builder.AddLine(start, end);

        var preview = new LineGeometryModel3D
        {
            Geometry = builder.ToLineGeometry3D(),
            Color = System.Windows.Media.Color.FromRgb(255,255,0),
            Thickness = 2,
            Name = "PreviewLine"
        };

        _previewRoot.Children.Add(preview);
    }

    public void ShowSnappingPoint(Vector3 position)
    {
        _snapMarker.Transform = new TranslateTransform3D(position.X, position.Y, position.Z);

        _snapMarker.Visibility = Visibility.Visible;
    }

    public void HideSnappingPoint()
    {
        _snapMarker.Visibility = Visibility.Hidden;
    }

    public void ClearPreview()
    {
        // Find existing preview named "PreviewLine" without modifying the collection during enumeration
        Element3D? existingPreview = null;
        foreach (var child in _previewRoot.Children)
        {
            if (child is not null && child.Name == "PreviewLine")
            {
                existingPreview = child;
                break;
            }
        }

        if (existingPreview != null)
        {
            _previewRoot.Children.Remove(existingPreview);
        }

        _snapMarker.Visibility = Visibility.Hidden;
    }

    private void InsertEntity(CadEntity entity)
    {
        if (entity is LineEntity line)
        {
            _document.SpatialGrid.Insert(line, line.Start); // temporary
        }

        // future: handle other entity types
    }

    private void RemoveEntity(CadEntity entity)
    {
        if (entity is LineEntity line)
        {
            _document.SpatialGrid.Remove(line, line.Start);
        }
    }
}
