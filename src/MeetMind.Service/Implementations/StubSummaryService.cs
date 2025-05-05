using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations;

public class StubSummaryService : ISummaryService
{
    public Task<string> SummarizeAsync(string text)
    {
        // Placeholder logic: always return this fixed summary.
        string summary = "[Summary] This is a placeholder summary of length ~20% of original text.";
        return Task.FromResult(summary);
    }

}

