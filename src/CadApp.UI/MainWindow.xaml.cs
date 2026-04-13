// MainWindow.xaml.cs
// Composes the WPF workspace shell with CAD document, scene, interaction, and lightweight dock-region behavior.
using CadApp.Core.Document;
using CadApp.Commands;
using CadApp.Core.Entities;
using CadApp.Core.Import;
using CadApp.Core.Persistence;
using CadApp.Core.Snapping;
using CadApp.Core.Tools;
using CadApp.Rendering.Math;
using CadApp.Rendering.Scene;
using CadApp.Rendering.Tools;
using CadApp.UI.Services;
using CadApp.ViewModels;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using SelectionWindowOverlayController = CadApp.UI.Overlays.SelectionWindowOverlay;

namespace CadApp.UI;

public partial class MainWindow : Window
{
    private readonly CadDocument _document;
    private readonly SceneManager _scene;
    private readonly MainViewModel _viewModel;
    private readonly ProjectionService _projection;
    private readonly ToolManager _toolManager;
    private readonly SelectTool _selectTool;
    private readonly LineTool _lineTool;
    private readonly IModelImporter _stlImporter;
    private readonly SnapManager _snapManager;
    private readonly SelectionWindowOverlayController _selectionWindowOverlay;
    private readonly DocumentFileService _documentFileService;
    private readonly CadCommandRunner _commandRunner;
    private string _activeToolStatusText = "Select tool active";

    public DefaultEffectsManager EffectsManager { get; }

    /// <summary>
    /// Creates the main application window and composes the current CAD services.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        _selectionWindowOverlay = new SelectionWindowOverlayController(this, SelectionWindowOverlay);

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        EffectsManager = new DefaultEffectsManager();
        Viewport.EffectsManager = EffectsManager; // Needed since EffectsManager is initialised AFTER Main window is initialised. Data binding needs to be updated.

        _document = new CadDocument();
        _scene = new SceneManager(Viewport, _document);
        _snapManager = new SnapManager(_document.SpatialGrid);
        _projection = new ProjectionService(Viewport);
        _toolManager = new ToolManager();
        _commandRunner = new CadCommandRunner();
        _selectTool = new SelectTool(Viewport, _document, _scene, _scene.SelectionManager);
        _lineTool = new LineTool(_document, _projection, _scene, _snapManager, _commandRunner);
        _stlImporter = new StlImporter();
        _documentFileService = new DocumentFileService(
            this,
            _document,
            _scene.SelectionManager,
            new GphDocumentSerializer(),
            CancelTransientToolState,
            _commandRunner.ClearHistory,
            ActivateSelectToolForDocumentCommand);

        WireWorkspaceState();
        _selectTool.SelectionWindowChanged += _selectionWindowOverlay.Update;
        SetActiveTool(_selectTool, "Select tool active");
    }

    /// <summary>
    /// Connects domain events to shell UI state so workspace feedback stays outside the renderer.
    /// </summary>
    private void WireWorkspaceState()
    {
        _scene.SelectionManager.SelectionChanged += OnSelectionChanged;
        _viewModel.SetStatusText("Ready");
        _viewModel.SetSelectedEntity(null);
    }

    /// <summary>
    /// Creates a new blank document after optionally saving the current document.
    /// </summary>
    private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyDocumentFileResult(_documentFileService.New());
    }

    /// <summary>
    /// Opens a saved Graphite project file and replaces the current document after confirmation.
    /// </summary>
    private void OpenProjectMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyDocumentFileResult(_documentFileService.Open());
    }

    /// <summary>
    /// Saves the current document to a Graphite project file selected by the user.
    /// </summary>
    private void SaveProjectMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ApplyDocumentFileResult(_documentFileService.Save());
    }

    /// <summary>
    /// Handles viewport clicks by routing them to the active interaction logic and hit-test based selection.
    /// </summary>
    private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        _toolManager.ActiveTool?.OnMouseDown(GetScreenPosition(e));
        Viewport.CaptureMouse();
        e.Handled = true;
    }

    /// <summary>
    /// Handles viewport mouse movement so interactive tools can update previews efficiently.
    /// </summary>
    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        _toolManager.ActiveTool?.OnMouseMove(GetScreenPosition(e));

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Routes mouse-up events to the active interaction tool.
    /// </summary>
    private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        _toolManager.ActiveTool?.OnMouseUp(GetScreenPosition(e));

        if (Viewport.IsMouseCaptured)
        {
            Viewport.ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    /// <summary>
    /// Activates the selection tool from the tool panel.
    /// </summary>
    private void SelectToolButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(_selectTool, "Select tool active");
    }

    /// <summary>
    /// Activates the line creation tool from the tool panel.
    /// </summary>
    private void LineToolButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTool(_lineTool, "Line tool active: click two points");
    }

    /// <summary>
    /// Imports one STL mesh into the document and lets the scene manager render it incrementally.
    /// </summary>
    private void ImportStlButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Import STL",
            Filter = "STL files (*.stl)|*.stl|All files (*.*)|*.*",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            CadEntity importedEntity = _stlImporter.Import(dialog.FileName);
            _commandRunner.Execute(new AddEntityCommand(_document, importedEntity));

            string fileName = Path.GetFileName(dialog.FileName);
            _viewModel.SetStatusText($"Imported {fileName}");
            _viewModel.SetToolPanelText($"Imported {fileName}");
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is ArgumentException)
        {
            _viewModel.SetStatusText("STL import failed");
            MessageBox.Show(this, ex.Message, "STL Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles workspace keyboard shortcuts that cancel transient tool state.
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        _lineTool.Cancel();
        _selectTool.Cancel();
        SetActiveTool(_selectTool, "Select tool active");
        e.Handled = true;
    }

    /// <summary>
    /// Switches the active interaction tool and updates shell guidance text.
    /// </summary>
    private void SetActiveTool(ITool tool, string statusText)
    {
        if (!ReferenceEquals(tool, _lineTool))
        {
            _lineTool.Cancel();
        }

        if (!ReferenceEquals(tool, _selectTool))
        {
            _selectTool.Cancel();
        }

        _activeToolStatusText = statusText;
        _toolManager.SetTool(tool);
        _viewModel.SetStatusText(statusText);
        _viewModel.SetToolPanelText(statusText);
    }

    /// <summary>
    /// Converts a WPF mouse event into the float screen coordinate format used by CAD tools.
    /// </summary>
    private Vector2 GetScreenPosition(MouseEventArgs e)
    {
        Point mousePosition = e.GetPosition(Viewport);
        return new Vector2((float)mousePosition.X, (float)mousePosition.Y);
    }

    /// <summary>
    /// Updates shell state when the domain selection changes.
    /// </summary>
    private void OnSelectionChanged(IEnumerable<Guid> addedIds, IEnumerable<Guid> removedIds)
    {
        _ = addedIds;
        _ = removedIds;

        if (_scene.SelectionManager.SelectedCount == 1)
        {
            Guid? selectedId = GetSingleSelectedEntityId();

            if (selectedId.HasValue)
            {
                _viewModel.SetSelectedEntity(FindEntityById(selectedId.Value));
            }

            _viewModel.SetStatusText("Object selected");
            return;
        }

        if (_scene.SelectionManager.SelectedCount > 1)
        {
            _viewModel.SetMultipleSelection(_scene.SelectionManager.SelectedCount);
            _viewModel.SetStatusText($"{_scene.SelectionManager.SelectedCount} objects selected");
            return;
        }

        _viewModel.SetSelectedEntity(null);
        _viewModel.SetStatusText(_activeToolStatusText);
    }

    /// <summary>
    /// Finds a document entity by id for shell-level selection display.
    /// </summary>
    private CadEntity? FindEntityById(Guid id)
    {
        foreach (CadEntity entity in _document.Entities)
        {
            if (entity.Id == id)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the only selected id when selection contains exactly one entity.
    /// </summary>
    private Guid? GetSingleSelectedEntityId()
    {
        foreach (Guid selectedId in _scene.SelectionManager.SelectedEntityIds)
        {
            return selectedId;
        }

        return null;
    }

    /// <summary>
    /// Cancels active tool previews before document-level file commands change entities.
    /// </summary>
    private void CancelTransientToolState()
    {
        _lineTool.Cancel();
        _selectTool.Cancel();
    }

    /// <summary>
    /// Returns the workspace to selection mode after a document-level file command.
    /// </summary>
    private void ActivateSelectToolForDocumentCommand()
    {
        _viewModel.SetSelectedEntity(null);
        SetActiveTool(_selectTool, "Select tool active");
    }

    /// <summary>
    /// Applies user-facing status updates returned by document file commands.
    /// </summary>
    private void ApplyDocumentFileResult(DocumentFileOperationResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.StatusText))
        {
            _viewModel.SetStatusText(result.StatusText);
        }

        if (!string.IsNullOrWhiteSpace(result.ToolPanelText))
        {
            _viewModel.SetToolPanelText(result.ToolPanelText);
        }
    }
}
