using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.Models;

public class UserSettings
{
    public bool AutoRecord { get; set; } = false;
    public bool AutoTranscribe { get; set; } = true;
    public bool AutoSummarize { get; set; } = true;
    public string PreferredLanguage { get; set; } = "auto";
    public bool UseSystemLanguage { get; set; } = true;
    public bool AutoPlayAudio { get; set; } = false;
    public bool ShowAudioControls { get; set; } = true;
}
