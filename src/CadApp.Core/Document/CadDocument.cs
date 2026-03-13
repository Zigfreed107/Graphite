using CadApp.Core.Entities;
using System.Collections.ObjectModel;

namespace CadApp.Core.Document;

public class CadDocument
{
    public ObservableCollection<CadEntity> Entities { get; } = new();
}