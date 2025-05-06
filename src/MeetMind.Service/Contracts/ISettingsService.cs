using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMind.Service.Models;

namespace MeetMind.Service.Contracts;

public interface ISettingsService
{
    Task<UserSettings> LoadAsync();
    Task SaveAsync(UserSettings settings);
}