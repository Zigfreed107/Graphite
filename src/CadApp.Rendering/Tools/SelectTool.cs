using CadApp.Core.Selection;
using CadApp.Core.Tools;
using CadApp.Rendering.Scene;
using HelixToolkit.Wpf;

namespace CadApp.Rendering.Tools;

public class SelectTool : ITool
{
    private readonly HelixViewport3D _viewport;
    private readonly SceneManager _scene;
    private readonly SelectionManager _selection;

    public SelectTool(
        HelixViewport3D viewport,
        SceneManager scene,
        SelectionManager selection)
    {
        _viewport = viewport;
        _scene = scene;
        _selection = selection;
    }

    public void OnMouseDown(double x, double y)
    {
        var hits = _viewport.Viewport.FindHits(new System.Windows.Point(x, y));

        if (hits.Count > 0)
        {
            var entity = _scene.GetEntityFromVisual(hits[0].Visual);
            _selection.Select(entity);
        }
        else
        {
            _selection.Clear();
        }
    }

    public void OnMouseMove(double x, double y) { }

    public void OnMouseUp(double x, double y) { }
}