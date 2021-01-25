using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neurotec.Biometrics;

namespace TwinsDay2017_VoiceDemo
{
    public class Match
    {
        //private string thumbPath = @"D:\TWD17_SERVER\TWD17 Applications (Deployable)\DEPLOY\Demo Data\Face Img\";
        public string ID { get; set; }
        public int Score { get; set; }
        public string Filename { get; set; }
        public int Rank { get; set; }
        public double FAR { get; set; }
        public double Probability { get; set; }
        public string Image { get; set; }
        public string LargeImage { get; set; }

        public Match(string filename, string id, int score, double far, double prob, string imgFolder)
        {
            this.Filename = filename;
            this.ID = id;
            this.Score = score;
            this.FAR = far;
            this.Probability = prob;

            // Get thumbnail of subject
            // If no thumbnail exists for that subject, set it to the default 'No Image' thumbnail
            imgFolder += id + @"\Thumbnail\";
            if (File.Exists(imgFolder + @"Thumbnail.jpg"))
            {
                Image = imgFolder + @"Thumbnail.jpg";

                if (File.Exists(imgFolder + @"LargeThumbnail.jpg"))
                    LargeImage = imgFolder + @"LargeThumbnail.jpg";
                else
                    LargeImage = Image;
            }
            else
            {
                Image = "/Images/NoImage.Png"; // Default Thumbnail
                LargeImage = Image;
            }
        }
    }
}
