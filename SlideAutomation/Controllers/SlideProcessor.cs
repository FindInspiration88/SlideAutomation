using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using SlideAutomation.Models;

namespace SlideAutomation.Controllers
{
    public static class SlideProcessor
    {

        public static void LoadDefaultBackground(string presentationDir, string sourceFileName, string destFileName)
        {
            System.IO.File.Copy(sourceFileName, destFileName);
        }

        public static bool IsBackgroundsExtracted(string presentationDir, HttpPostedFileBase backgroundsFile)
        {
            var fileName = Path.GetFileName(backgroundsFile.FileName);
            var filePath = presentationDir + "/" + fileName;
            backgroundsFile.SaveAs(filePath);
            try
            {
                ZipFile.ExtractToDirectory(filePath, presentationDir + "/Backgrounds");
                if (Directory.GetFiles(presentationDir + "/Backgrounds")
                        .Where(path => path.Contains(".png") || path.Contains(".jpg")).Count() == 0)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;

        }

        public static bool IsSlidesCreated(string presentationDir, HttpPostedFileBase textsFile)
        {
            var fileName = Path.GetFileName(textsFile.FileName);
            var filePath = presentationDir + "/" + fileName;
            textsFile.SaveAs(filePath);
            StreamReader fileStream = new StreamReader(filePath, Encoding.Default);
            var fileText = fileStream.ReadToEnd();
            if (!fileText.Contains("СЛАЙД"))
                return false;
            var slideTexts = fileText
                .Split(new string[] { "СЛАЙД: ", "СЛАЙД:" }, StringSplitOptions.RemoveEmptyEntries);
            var slides = new List<Slide>();
            foreach (var slideText in slideTexts)
            {
                var slide = new Slide();
                slide.Title = slideText.Split('\n')[0];
                slide.Text = slideText.Replace(slide.Title, "");
                slide.PathToBackgroundPicture = GetRandomBackground(presentationDir);
                slides.Add(slide);
            }
            SaveSlides(slides, presentationDir);
            return true;
        }

        public static void CreatePresentationDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path + "/Backgrounds");
                Directory.CreateDirectory(path + "/Slides");
                Directory.CreateDirectory(path + "/SlidesJSON");
            }
        }

        private static string GetRandomBackground(string presentationDir)
        {
            var BackgroundPaths = Directory.GetFiles(presentationDir + "/Backgrounds");
            BackgroundPaths = BackgroundPaths
                .Where(path => path.Contains(".png") || path.Contains(".jpg")).ToArray();
            return BackgroundPaths[new Random().Next(BackgroundPaths.Length)];
        }

        public static string GetPresentationId()
        {
            var dateId = DateTime.Now.ToString();
            string[] charsToRemove = { ".", " ", ":" };
            foreach (var symbol in charsToRemove)
            {
                dateId = dateId.Replace(symbol, "");
            }
            return dateId;
        }

        private static void SaveSlides(List<Slide> slides, string presentationDir)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            for (var i = 0; i < slides.Count; i++)
            {
                SlideSaver.SaveSlideAsJpeg(slides[i], presentationDir + "/Slides/" + i.ToString() + ".jpg");
                using (FileStream fs = new FileStream(presentationDir + "/SlidesJSON/" + i.ToString() + ".json", FileMode.OpenOrCreate))
                {
                    jsonFormatter.WriteObject(fs, slides[i]);
                }
            }
            ArchivePresentation(presentationDir);
        }

        public static void ArchivePresentation(string presentationDir)
        {
            if (System.IO.File.Exists(presentationDir + "/Slides.zip"))
                System.IO.File.Delete(presentationDir + "/Slides.zip");
            ZipFile.CreateFromDirectory(presentationDir + "/Slides", presentationDir + "/Slides.zip");
        }
    }
}