
namespace MeetMind.Service.Contracts;

public interface ITagGeneratorService
{
    Task<string[]> GenerateTagsAsync(string transcript, int max = 5);
}