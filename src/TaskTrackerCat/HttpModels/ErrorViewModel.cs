namespace TaskTrackerCat.HttpModels;

public class ErrorViewModel<T>
{
    public string Detail { get; set; }
    public T ViewModel { get; set; }
}