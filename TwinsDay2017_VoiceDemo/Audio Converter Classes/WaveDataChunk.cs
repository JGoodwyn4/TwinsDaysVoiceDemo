using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinsDay2017_VoiceDemo
{
    class WaveDataChunk
    {
        public string sChunkID;
        public uint dwChunkSize;
        public ushort[] shortArray;

        public WaveDataChunk()
        {
            shortArray = new ushort[0];
            dwChunkSize = 0;
            sChunkID = "data";
        }
    }
}
