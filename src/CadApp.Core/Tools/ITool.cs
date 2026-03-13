namespace CadApp.Core.Tools;

public interface ITool
{
    void OnMouseDown(double x, double y);
    void OnMouseMove(double x, double y);
    void OnMouseUp(double x, double y);
}