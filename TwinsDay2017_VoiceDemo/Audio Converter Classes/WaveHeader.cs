using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinsDay2017_VoiceDemo
{
    class WaveHeader
    {
        public string sGroupID;
        public uint dwFileLength;
        public string sRiffType;

        public WaveHeader()
        {
            dwFileLength = 0;
            sGroupID = "RIFF";
            sRiffType = "WAVE";
        }
    }
}
