using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations;

public class MockSummaryService : ISummaryService
{
    public Task<string> SummarizeAsync(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return Task.FromResult("[Empty transcript]");

        var sentences = transcript.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
        var top3 = sentences.OrderByDescending(s => s.Length).Take(3);

        var summary = string.Join(". ", top3.Select(s => s.Trim())) + ".";
        return Task.FromResult(summary);
    }
}