namespace BackgroundService.Application.Notification.Common.Models;

public sealed class HtmlTableColumn
{
    public string Key { get; init; } = "";
    public string Header { get; set; } = "";    
    public string Width { get; init; } = "auto";
    public string Align { get; init; } = "left";
    public string? Format { get; init; }     
    public bool? Wrap { get; set; }
    public bool? Bold { get; set; }       
}

public sealed class HtmlTableSpec
{
    public List<HtmlTableColumn> Columns { get; init; } = new();
    public List<Dictionary<string, object?>> Rows { get; init; } = new();
}
