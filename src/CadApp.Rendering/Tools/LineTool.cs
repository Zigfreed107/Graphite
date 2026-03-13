using CadApp.Core.Document;
using CadApp.Core.Entities;
using CadApp.Core.Tools;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CadApp.Rendering.Math;

namespace CadApp.Rendering.Tools;

public class LineTool : ITool
{
    private readonly HelixViewport3D _viewport;
    private readonly CadDocument _document;

    private bool _isDrawing;
    private Vector3 _startPoint;

    private LinesVisual3D? _previewLine;

    public LineTool(HelixViewport3D viewport, CadDocument document)
    {
        _viewport = viewport;
        _document = document;
    }

    public void OnMouseDown(double x, double y)
    {

        if (!TryGetWorldPoint(x, y, out var point))
            return;

        if (!_isDrawing)
        {
            _startPoint = point;
            _isDrawing = true;

            _previewLine = new LinesVisual3D
            {
                Color = Colors.Red,
                Thickness = 2
            };

            _viewport.Children.Add(_previewLine);
        }
        else
        {
            var line = new LineEntity
            {
                Start = _startPoint,
                End = point
            };

            _document.Entities.Add(line);

            if (_previewLine != null)
                _viewport.Children.Remove(_previewLine);

            _previewLine = null;
            _isDrawing = false;
        }
    }

    public void OnMouseMove(double x, double y)
    {
        if (!_isDrawing || _previewLine == null)
            return;

        if (!TryGetWorldPoint(x, y, out var point))
            return;

        _previewLine.Points.Clear();

        _previewLine.Points.Add(new Point3D(_startPoint.X, _startPoint.Y, _startPoint.Z));
        _previewLine.Points.Add(new Point3D(point.X, point.Y, point.Z));
    }

    public void OnMouseUp(double x, double y) { }

    //private bool TryGetWorldPoint(double x, double y, out Vector3 point)
    //{
    //    IList<PointHitResult> hits = _viewport.Viewport.FindHits(new System.Windows.Point(x, y));

    //    if (hits.Count > 0)
    //    {
    //        var p = hits[0].Position;

    //        point = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
    //        return true;
    //    }

    //    point = default;
    //    return false;
    //}

    private bool TryGetWorldPoint(double x, double y, out Vector3 point)
    {
        return Workplane.TryGetPointOnPlane(_viewport, x, y, out point);
    }
}