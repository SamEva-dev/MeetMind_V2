using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.Models;

public class TranscriptionResult
{
    public int Id { get; set; }
    public bool Success { get; set; }
    public string? Transcript { get; set; }
    public string? Error { get; set; }
    public string? Log { get; set; }
}
