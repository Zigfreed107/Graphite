// AddEntityCommand.cs
// Provides the undoable command boundary for adding one CAD entity to the document.
using CadApp.Core.Document;
using CadApp.Core.Entities;
using System;

namespace CadApp.Commands;

/// <summary>
/// Adds a CAD entity to the document and can undo that addition.
/// </summary>
public sealed class AddEntityCommand : ICadCommand
{
    private readonly CadDocument _document;
    private readonly CadEntity _entity;
    private bool _hasExecuted;

    /// <summary>
    /// Creates a command that owns adding the specified entity to the specified document.
    /// </summary>
    public AddEntityCommand(CadDocument document, CadEntity entity)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    /// Adds the entity to the document.
    /// </summary>
    public void Execute()
    {
        if (_hasExecuted)
        {
            return;
        }

        _document.AddEntity(_entity);
        _hasExecuted = true;
    }

    /// <summary>
    /// Removes the entity that was added by this command.
    /// </summary>
    public void Undo()
    {
        if (!_hasExecuted)
        {
            return;
        }

        _document.RemoveEntity(_entity);
        _hasExecuted = false;
    }
}
