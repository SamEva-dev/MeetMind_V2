using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindUI.Models;

public class RecordingItem
{
    public string FileName { get; set; } = string.Empty;
    public string TranscriptPath => Path.ChangeExtension(FileName, ".txt");
    public string SummaryPath => Path.ChangeExtension(FileName, ".summary.txt");
    public string AudioPath => FileName;
    public string DisplayName => Path.GetFileNameWithoutExtension(FileName);
    public DateTime Created => File.GetCreationTime(FileName);
}