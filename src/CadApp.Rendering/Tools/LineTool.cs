using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Rendering.Math;
using CadApp.Rendering.Scene;
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

    private Vector3? _startPoint;

    public LineTool(CadDocument document,
                    ProjectionService projection,
                    SceneManager scene)
    {
        _document = document;
        _projection = projection;
        _scene = scene;
    }

    public void OnMouseDown(MouseButtonEventArgs e, Viewport3DX viewport)
    {
        var pos = e.GetPosition(viewport);

        if (!_projection.TryGetWorldPoint(pos, out var world))
            return;

        if (_startPoint == null)
        {
            _startPoint = world;
        }
        else
        {
            var line = new LineEntity(_startPoint.Value, world);

            _document.Entities.Add(line);

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

        _scene.ShowPreviewLine(_startPoint.Value, world);
    }
}