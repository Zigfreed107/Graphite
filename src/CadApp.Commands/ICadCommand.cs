namespace CadApp.Commands;

public interface ICadCommand
{
    void Execute();
    void Undo();
}