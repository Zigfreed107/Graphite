using System.Windows.Input;
using HelixToolkit.Wpf.SharpDX;

namespace CadApp.Rendering.Tools;

public interface ITool
{
    void OnMouseDown(MouseButtonEventArgs e, Viewport3DX viewport);
    void OnMouseMove(MouseEventArgs e, Viewport3DX viewport);
}