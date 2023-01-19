using System;

namespace DereTore.Exchange.Audio.HCA {
    [Flags]
    internal enum HcaAudioStreamDecodeState {
        Initialized = 0,
        WaveHeaderTransmitting = 1,
        WaveHeaderTransmitted = 2,
        DataTransmitting = 4,
        DataTransmitted = 8
    }
}
