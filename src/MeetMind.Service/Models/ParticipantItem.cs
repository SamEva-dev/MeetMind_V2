using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.Models;

public class ParticipantItem
{
    public string Name { get; set; }
    public bool IsPresent { get; set; }
    // Future: public string? VoiceId { get; set; }
}