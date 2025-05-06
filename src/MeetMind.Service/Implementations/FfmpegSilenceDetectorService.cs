using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations;

public class FfmpegSilenceDetectorService : ISilenceDetectorService
{
    public async Task<List<SpeechSegment>> GetSpeechSegmentsAsync(string audioFilePath)
    {
        var segments = new List<SpeechSegment>();
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -i \"{audioFilePath}\" -af silencedetect=n=-30dB:d=0.5 -f null -",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();
        string output = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var silenceStartRegex = new Regex(@"silence_start: (?<start>\d+\.\d+)");
        var silenceEndRegex = new Regex(@"silence_end: (?<end>\d+\.\d+)");

        var silenceStarts = new List<TimeSpan>();
        var silenceEnds = new List<TimeSpan>();

        foreach (Match match in silenceStartRegex.Matches(output))
        {
            var start = TimeSpan.FromSeconds(double.Parse(match.Groups["start"].Value, CultureInfo.InvariantCulture));
            silenceStarts.Add(start);
        }

        foreach (Match match in silenceEndRegex.Matches(output))
        {
            var end = TimeSpan.FromSeconds(double.Parse(match.Groups["end"].Value, CultureInfo.InvariantCulture));
            silenceEnds.Add(end);
        }

        TimeSpan lastEnd = TimeSpan.Zero;
        foreach (var silenceStart in silenceStarts)
        {
            if (silenceStart > lastEnd)
            {
                segments.Add(new SpeechSegment(lastEnd, silenceStart));
            }

            var correspondingEnd = silenceEnds.Find(e => e > silenceStart);
            if (correspondingEnd > TimeSpan.Zero)
                lastEnd = correspondingEnd;
        }

        return segments;
    }
}