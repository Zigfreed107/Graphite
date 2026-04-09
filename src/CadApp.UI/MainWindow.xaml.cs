using CadApp.Core.Document;
using CadApp.Core.Snapping;
using CadApp.Core.Tools;
using CadApp.Rendering.Math;
using CadApp.Rendering.Scene;
using CadApp.Rendering.Tools;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using HitTestResult = HelixToolkit.SharpDX.HitTestResult;

namespace CadApp.UI;

public partial class MainWindow : Window
{
    private readonly CadDocument _document;
    private readonly SceneManager _scene;
    private readonly ToolManager _tools = new();
    private readonly SelectionManager _selection = new();
    private ProjectionService _projection;
    private LineTool _lineTool;
    private SnapManager _snapManager;
    
    public DefaultEffectsManager EffectsManager { get; }
    private readonly Dictionary<object, ISelectable> _modelToEntityMap = new Dictionary<object, ISelectable>();

    public MainWindow()
    {
        InitializeComponent();

        this.DataContext = this;        //QUick Fix - should be using a ViewModel

        EffectsManager = new DefaultEffectsManager();
 

        _document = new CadDocument();

        _scene = new SceneManager(Viewport, _document);

        _snapManager = new SnapManager(_document.SpatialGrid);

        _projection = new ProjectionService(Viewport);
        _lineTool = new LineTool(_document, _projection, _scene, _snapManager);
    }

    public ISelectable? GetEntityFromModel(object model)
    {
        if (_modelToEntityMap.TryGetValue(model, out ISelectable entity))
        {
            return entity;
        }

        return null;
    }

    private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _lineTool.OnMouseDown(e, Viewport);

        //TODO: Filter out entities that shouldnt be selected like preview lines, grids, etc. Possibly detect if object is Iselectable
        // Perform hit test using Helix
        var hits = this.Viewport.FindHits(e.GetPosition(this.Viewport));

        if (hits.Count > 0) 
        { 
            HitTestResult result = hits[0];

            if (result != null && result.ModelHit != null)
            {
                ISelectable? entity = _scene.GetEntityFromVisual((Element3D)result.ModelHit); 

                if (entity != null)
                {
                    this._scene.SelectionManager.SelectSingle(entity);
                }
            }
            else
            {
                // Clicked empty space → deselect
                this._scene.SelectionManager.ClearSelection();
            }
        }
    }

    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        _lineTool.OnMouseMove(e, Viewport);
    }

    private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(Viewport);
        _tools.ActiveTool?.OnMouseUp(pos.X, pos.Y);
    }
}
