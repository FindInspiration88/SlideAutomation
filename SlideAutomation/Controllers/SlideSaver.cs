using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SlideAutomation.Models;

namespace SlideAutomation.Controllers
{
    public class SlideSaver
    {
        public static void Save(Slide slide, string outputPath)
        {
            var picture = Image.FromFile(slide.PathToBackgroundFile);
            var pictureToOurFormat = new Bitmap(picture, 800, 600);
            PrintTitle(slide, pictureToOurFormat);
            PrintText(slide, pictureToOurFormat);
            pictureToOurFormat.Save(outputPath, ImageFormat.Jpeg);
        }

        private static void PrintText(Slide slide, Bitmap pictureToOurFormat)
        {
            var slideWidth = 400;
            float fontSize = 26;
            float offsetX = 0;
            float offsetY = 0;
            float textStartPosX = 100;
            float textStartPosY = 150;
            var words = slide.Content.Split(' ');
            words[0] = words[0].Replace("\n", " ");
            var wordStyle = new WordStyle(new Font("Arial", fontSize, FontStyle.Regular),
                new SolidBrush(Color.WhiteSmoke));
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (words[i].Contains("[$"))
                {
                    wordStyle = WordStyle.GetWordStyle(words[i], fontSize);
                    word = words[i].Remove(0, words[i].IndexOf(']') + 1);
                }
                word = word.Replace("$]", "");
                var wordPosition = new RectangleF(textStartPosX + offsetX, textStartPosY + offsetY, 800, 50);
                var part2 = Graphics.FromImage(pictureToOurFormat);
                part2.DrawString(word,
                    wordStyle.Font,
                    wordStyle.Color, wordPosition,
                    new StringFormat(StringFormatFlags.NoClip));
                offsetX += part2.MeasureString(word, wordStyle.Font).Width;
                if (words[i].Contains("$]")) wordStyle = new WordStyle(
                    new Font("Arial", fontSize, FontStyle.Regular),
                    new SolidBrush(Color.WhiteSmoke));
                if (offsetX > slideWidth)
                {
                    offsetX = 0;
                    offsetY += fontSize * 1.5F;
                }
            }
        }

        private static void PrintTitle(Slide slide, Bitmap pictureToOurFormat)
        {
            var title = slide.Title;
            var titleStyle = new WordStyle(new Font("Arial", 30, FontStyle.Bold),
                new SolidBrush(Color.WhiteSmoke));
            var titleGraphic = Graphics.FromImage(pictureToOurFormat);
            var titleMeasurement = titleGraphic.MeasureString(title, titleStyle.Font).Width;
            float titleStartPosX = 400 - (titleMeasurement / 2);
            float titleStartPosY = 50;
            var titlePosition = new RectangleF(titleStartPosX, titleStartPosY, 600, 100);
            titleGraphic.DrawString(title,
                    titleStyle.Font,
                    titleStyle.Color, titlePosition,
                    new StringFormat(StringFormatFlags.NoClip));
        }

        private class WordStyle
        {
            public Font Font { get; set; }
            public SolidBrush Color { get; set; }
            public WordStyle(Font font, SolidBrush color)
            {
                Font = font;
                Color = color;
            }

            public static WordStyle GetWordStyle(string word, float fontSize)
            {
                var style = FontStyle.Regular;
                var styleTags = word.Split(']')[0];
                if (styleTags.Contains("bold")) style = style | FontStyle.Bold;
                if (styleTags.Contains("italic")) style = style | FontStyle.Italic;
                if (styleTags.Contains("underline")) style = style | FontStyle.Underline;
                var color = new SolidBrush(System.Drawing.Color.WhiteSmoke);
                return new WordStyle(new Font("Arial", fontSize, style), color);

            }
        }
    }
}