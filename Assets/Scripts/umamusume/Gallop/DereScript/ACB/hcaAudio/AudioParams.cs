namespace DereTore.Exchange.Audio.HCA {
    public struct AudioParams {

        /// <summary>
        /// 0 = play as if it is a normal sequential audio.
        /// </summary>
        public uint SimulatedLoopCount { get; set; }
        /// <summary>
        /// </summary>
        /// <remarks>
        /// When applied to an HCA audio with a loop range, <see cref="OutputWaveHeader"/> must be set to <code>false</code>.
        /// Otherwise an invalid wave header will be generated.
        /// </remarks>
        public bool InfiniteLoop { get; set; }
        public bool OutputWaveHeader { get; set; }

        public static AudioParams CreateDefault() {
            return new AudioParams {
                InfiniteLoop = false,
                SimulatedLoopCount = 0,
                OutputWaveHeader = true
            };
        }

        public static readonly AudioParams Default = CreateDefault();

    }
}
