// CadCommandRunner.cs
// Executes CAD commands and keeps undo/redo history separate from tools and rendering.
using System;
using System.Collections.Generic;

namespace CadApp.Commands;

/// <summary>
/// Runs document commands and stores the command history needed for undo and redo.
/// </summary>
public sealed class CadCommandRunner
{
    private readonly Stack<ICadCommand> _undoStack = new Stack<ICadCommand>();
    private readonly Stack<ICadCommand> _redoStack = new Stack<ICadCommand>();

    /// <summary>
    /// Gets whether there is a command that can be undone.
    /// </summary>
    public bool CanUndo
    {
        get { return _undoStack.Count > 0; }
    }

    /// <summary>
    /// Gets whether there is a command that can be redone.
    /// </summary>
    public bool CanRedo
    {
        get { return _redoStack.Count > 0; }
    }

    /// <summary>
    /// Executes a new command and records it for future undo.
    /// </summary>
    public void Execute(ICadCommand command)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    /// <summary>
    /// Undoes the most recently executed command when one is available.
    /// </summary>
    public bool Undo()
    {
        if (_undoStack.Count == 0)
        {
            return false;
        }

        ICadCommand command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        return true;
    }

    /// <summary>
    /// Re-executes the most recently undone command when one is available.
    /// </summary>
    public bool Redo()
    {
        if (_redoStack.Count == 0)
        {
            return false;
        }

        ICadCommand command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        return true;
    }

    /// <summary>
    /// Clears command history when the document is replaced outside the undoable edit stream.
    /// </summary>
    public void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
