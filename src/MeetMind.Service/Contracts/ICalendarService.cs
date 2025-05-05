using System.Collections;

namespace MeetMind.Service.Contracts;

public interface ICalendarService
{
    /// 
    /// Retrieves the list of participant names for a meeting scheduled
    /// between  and .
    /// 
    /// Meeting start time.
    /// Meeting end time.
    /// List of participant display names.
    Task<IList<string>> GetParticipantsAsync(DateTime start, DateTime end);
}
