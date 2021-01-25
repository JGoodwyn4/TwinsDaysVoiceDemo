using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinsDay2017_VoiceDemo
{
    class AudioConverter
    {
        /*
         * Note: the majority of the following code and classes were adapted from https://blogs.msdn.microsoft.com/dawate/2009/06/24/intro-to-audio-programming-part-3-synthesizing-simple-wave-audio-using-c/
         * aside from the actual conversion of 24-bit to 16-bit
         */

        private WaveHeader header;
        private WaveFormatChunk format;
        private WaveDataChunk data;
        private byte[] audio;

        public AudioConverter(string filename)
        {
            audio = File.ReadAllBytes(filename);
        }

        public bool Convert()
        {
            int index = 12;

            // Check to see if the WAV header contains a "JUNK" filler/subchunk
            if (audio[index] == 74 && audio[index + 1] == 85 && audio[index + 2] == 78 && audio[index + 3] == 75)
            {
                index += 4; // Move to next byte group
                int chunkSize = GetByteInfo(4, index); // Get the size of the JUNK subchunk
                index += 4 + chunkSize; // Move to next byte group then jump to next subchunk
            }
            // index should now be placed right on the start of subchunk1
                

            // From the start of subchunk1, look to see if the BitsPerSample is already 16
            // If so, then no conversion is needed
            if (audio[index + 22] == 16)
                return false;
            else if (audio[index + 22] != 24)
                throw new Exception("Audio header is incorrect format or audio is not 16/24-bit");

            try
            {
                // Create header info
                header = new WaveHeader();
                format = new WaveFormatChunk((uint)GetByteInfo(4, index + 12)); // Get info from SampleRate byte subgroup, which is 12 bytes from the start of subchunk1
                data = new WaveDataChunk();


                // While not at "data" byte subgroup, which signifies the start of subchunk2
                while (!(audio[index] == 100 && audio[index + 1] == 97 && audio[index + 2] == 116 && audio[index + 3] == 97))
                {
                    index += 4; // Move index from start of subchunk1 to next byte group which contains the size of subchunk1
                    int chunkSize = GetByteInfo(4, index); // Read in the byte data to get the size of the subchunk
                    index += 4 + chunkSize; // Move to start of next byte group and jump to next subchunk, which should be subchunk2
                }
                index += 4; // Move index to start of byte group that contains the total size of the data

                // Read in data and re-calculate number of samples. 24 to 16 bit audio will have 1/3 less samples (since 24 bit = 3 bytes and 16 bit = 2 bytes per sample)
                int numSamples = GetByteInfo(4, index) / 3;
                data.shortArray = new ushort[numSamples]; // Set our converted audio data to the correct number of samples

                index += 4; // Move index to the beginning of the data section

                // Read every group of 3 bytes and convert their value into the 16 bit equivalent
                for (int i = 0; i < numSamples - 1; i++)
                {
                    data.shortArray[i] = (ushort)(GetByteInfo(3, index) / 256);
                    index += 3;
                }

                data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));
            }
            catch
            {
                throw new Exception("Error occurred in conversion process");
            }

            return true;
        }

        private int GetByteInfo(int numBytes, int index)
        {
            int total = 0;
            for (int i = 0; i < numBytes; i++)
                total += audio[i + index] * (int)Math.Pow(256, i);

            return total;
        }

        public byte[] GetConvertedAudio()
        {
            byte[] result = new byte[0];

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(header.sGroupID.ToCharArray());
                    writer.Write(header.dwFileLength);
                    writer.Write(header.sRiffType.ToCharArray());

                    writer.Write(format.sChunkID.ToCharArray());
                    writer.Write(format.dwChunkSize);
                    writer.Write(format.wFormatTag);
                    writer.Write(format.wChannels);
                    writer.Write(format.dwSamplesPerSec);
                    writer.Write(format.dwAvgBytesPerSec);
                    writer.Write(format.wBlockAlign);
                    writer.Write(format.wBitsPerSample);

                    writer.Write(data.sChunkID.ToCharArray());
                    writer.Write(data.dwChunkSize);
                    foreach (short dataPoint in data.shortArray)
                    {
                        writer.Write(dataPoint);
                    }

                    writer.Seek(4, SeekOrigin.Begin);
                    uint filesize = (uint)writer.BaseStream.Length;
                    writer.Write(filesize - 8);
                }

                result = ms.ToArray();
            }

            return result;
        }

        public byte[] GetOriginalAudio()
        {
            return audio;
        }
    }
}
