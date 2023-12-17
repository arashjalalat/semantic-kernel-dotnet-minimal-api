namespace OpenAIMinimalApi.Models;

public class Response
{
    public List<string>? List { get; set; }
    public List<Synopsis>? Synopses { get; set; }
}
public class Chapter
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class Synopsis
{
    public List<string>? Tag { get; set; }
    public Chapter? Chapter { get; set; }
}

public class Query
{
    public string? Value { get; set; }
}