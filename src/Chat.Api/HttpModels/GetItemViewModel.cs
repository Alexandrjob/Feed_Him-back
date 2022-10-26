namespace Chat.Api.HttpModels;

public class GetItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string StatusName { get; set; }
    public Status Status { get; set; }
}

public enum Status
{
    Start,
    Completed
}