using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinsDay2017_VoiceDemo
{
    class WaveFormatChunk
    {
        public string sChunkID;
        public uint dwChunkSize;
        public ushort wFormatTag;
        public ushort wChannels;
        public uint dwSamplesPerSec;
        public uint dwAvgBytesPerSec;
        public ushort wBlockAlign;
        public ushort wBitsPerSample;

        public WaveFormatChunk(uint sps)
        {
            sChunkID = "fmt ";
            dwChunkSize = 16;
            wFormatTag = 1;
            wChannels = 1;
            dwSamplesPerSec = sps;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
        }
    }
}
