namespace CadApp.Core.Tools;

public class ToolManager
{
    public ITool? ActiveTool { get; private set; }

    public void SetTool(ITool tool)
    {
        ActiveTool = tool;
    }
}