using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Rendering.Math;
using CadApp.Rendering.Scene;
using CadApp.Rendering.Snapping;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Numerics;
using System.Windows.Input;

namespace CadApp.Rendering.Tools;

public class LineTool : ITool
{
    private readonly CadDocument _document;
    private readonly ProjectionService _projection;
    private readonly SceneManager _scene;
    private readonly SnapManager _snapManager;

    private Vector3? _startPoint;

    public LineTool(CadDocument document,
                    ProjectionService projection,
                    SceneManager scene,
                    SnapManager snapManager)
    {
        _document = document;
        _projection = projection;
        _scene = scene;
        _snapManager = snapManager;
    }

    public void OnMouseDown(MouseButtonEventArgs e, Viewport3DX viewport)
    {
        var pos = e.GetPosition(viewport);

        if (!_projection.TryGetWorldPoint(pos, out var world))
            return;

        if (_snapManager.TrySnap(world, 0.5f, out var snap))
        {
            world = snap.Position;
        }

        if (_startPoint == null)
        {
            _startPoint = world;
        }
        else
        {
            _document.Entities.Add(new LineEntity(_startPoint.Value, world));
            _scene.ClearPreview();
            _startPoint = null;
        }
    }

    public void OnMouseMove(MouseEventArgs e, Viewport3DX viewport)
    {
        if (_startPoint == null)
            return;

        var pos = e.GetPosition(viewport);

        if (!_projection.TryGetWorldPoint(pos, out var world))
            return;

        // SNAP
        if (_snapManager.TrySnap(world, 0.5f, out var snap))
        {
            world = snap.Position;
        }

        _scene.ShowPreviewLine(_startPoint.Value, world);
    }
}