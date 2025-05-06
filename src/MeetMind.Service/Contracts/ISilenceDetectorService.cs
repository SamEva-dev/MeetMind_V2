using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.Contracts;

public record SpeechSegment(TimeSpan Start, TimeSpan End);

public interface ISilenceDetectorService
{
    Task<List<SpeechSegment>> GetSpeechSegmentsAsync(string audioFilePath);
}