namespace WindowsRepairToolkit.Services;

public interface IUiDispatcher
{
    void Invoke(Action action);
}
