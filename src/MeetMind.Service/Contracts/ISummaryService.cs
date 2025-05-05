using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.Contracts;

public interface ISummaryService
{
    [Pure]
    Task<string> SummarizeAsync(string text);
}