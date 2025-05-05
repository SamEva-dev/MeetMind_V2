using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MeetMind.Service.InteropServices;

public static class WhisperInterop
{
    [DllImport("whisper")]
    public static extern int whisper_transcribe(string audioPath, string modelPath, StringBuilder outputBuffer, int bufferSize);
}