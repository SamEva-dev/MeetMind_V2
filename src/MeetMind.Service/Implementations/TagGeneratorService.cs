using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations;

public class TagGeneratorService : ITagGeneratorService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "is", "at", "which", "on", "and", "a", "an", "of", "in", "to", "it", "for",
            "with", "this", "that", "by", "from", "or", "as", "be", "are", "was", "were", "has", "had",
            "will", "would", "can", "could", "should", "shall", "may", "might", "do", "did", "not",
            "you", "we", "they", "he", "she", "i", "my", "me", "your", "our", "their"
        };

    public Task<string[]> GenerateTagsAsync(string transcript, int max = 5)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return Task.FromResult(Array.Empty<string>());

        var words = Regex.Matches(transcript.ToLowerInvariant(), "\b[a-z]{4,}\b")
                         .Select(m => m.Value)
                         .Where(word => !StopWords.Contains(word))
                         .GroupBy(w => w)
                         .OrderByDescending(g => g.Count())
                         .Take(max)
                         .Select(g => g.Key)
                         .ToArray();

        return Task.FromResult(words);
    }
}