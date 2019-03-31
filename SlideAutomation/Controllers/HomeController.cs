using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SlideAutomation.Models;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace SlideAutomation.Controllers
{
    public class HomeController : Controller
    {
        //get
        public ActionResult Index()
        {
            return View();
        }

        //get
        public ActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetSlideEditorView(int id, string name)
        {
            var startAddress = "~/Presentations/" + name + "/Slides";
            var slidePaths = Directory.GetFiles(Server.MapPath(startAddress)).ToList();
            var slides = slidePaths
              .Select(path => startAddress + Path.GetFileName(path)).ToList();
            //id >= slides.Count ? id -= 1 : id = 0;
            if (id < slides.Count) id = 0;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));//json-object
            var jsonPath = Server.MapPath("~/Presentations/" + name + "/SlidesJSON/" + id + ".json");
            using (FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
            {
                var slide = (Slide)jsonFormatter.ReadObject(fs);
                ViewBag.SlideText = slide.Content;
                ViewBag.SlideTitle = slide.Title;
                ViewBag.BackgroundPath = slide.PathToBackgroundFile;
                ViewBag.Warning = (slide.PathToBackgroundFile.Contains("default.jpg")) ?
                    "Был загружен стандартный фон, так как архив не был прочитан." : "";
            }
            ViewBag.presDir = Server.MapPath("~/Presentations/" + name);
            ViewBag.SlideId = id;
            ViewBag.SlideName = name;
            ViewBag.SlidePath = slides[id];
            ViewBag.NextSlide = "~/Home/Slide/" + (id + 1) + "/" + name;
            ViewBag.PreviousSlide = "~/Home/Slide/" + (id - 1) + "/" + name;
            ViewBag.DownloadLink = "~/Presentations/" + name + "/Slides.zip";
            return View();
        }

        [HttpPost]
        public RedirectResult Index(IEnumerable<HttpPostedFileBase> upload)
        {
            var presentationId = CreatePresentationId();
            var presentationDir = Server.MapPath("~/Presentations/" + presentationId);
            CreatePresentationDir(presentationDir);
            var textsFile = upload.ToArray()[0];
            var backgroundsFile = upload.ToArray()[1];
            if (backgroundsFile == null
                || !backgroundsFile.FileName.Contains(".zip")
                || !IsBackgroundsExtracted(presentationDir, backgroundsFile))
            {
                LoadDefaultBackground(presentationDir);
            }
            if (textsFile != null && textsFile.FileName.Contains(".txt") && IsSlidesCreated(presentationDir, textsFile))
            {
                return Redirect("/Home/Slide/0/" + presentationId);//переадресация по ссылке на 0 слайд новой презентации
            }
            return Redirect("/Home/Error");
        }

        [HttpPost]
        public RedirectResult GetSlideEditorView(string slideText, string slideTitle, string slideBg, string slideDir, string slideId, string slideName)
        {
            var modifiedSlide = new Slide();
            modifiedSlide.Title = slideTitle;
            modifiedSlide.Content = slideText;
            modifiedSlide.PathToBackgroundFile = slideBg;
            SlideSaver.Save(modifiedSlide, slideDir + "/Slides/" + slideId + ".jpg");
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            using (FileStream fs = new FileStream(slideDir + "/SlidesJSON/" + slideId + ".json", FileMode.Create))
            {
                jsonFormatter.WriteObject(fs, modifiedSlide);
            }
            ArchivePresentation(slideDir);
            return Redirect("/Home/Slide/" + slideId + "/" + slideName);//изменение состояния страницы,
                                                                        //можно сделать это без Redirect
        }

        private void LoadDefaultBackground(string presentationDir)
        {
            System.IO.File.Copy(Server.MapPath("~/Files/Backgrounds/default.jpg"), presentationDir + "/Backgrounds/default.jpg");
        }

        private bool IsBackgroundsExtracted(string presentationDir, HttpPostedFileBase backgroundsFile)
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

        private bool IsSlidesCreated(string presentationDir, HttpPostedFileBase textsFile)
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
                slide.Content = slideText.Replace(slide.Title, "");
                slide.PathToBackgroundFile = GetRandomBackground(presentationDir);
                slides.Add(slide);
            }
            SaveSlides(slides, presentationDir);
            return true;
        }

        private void CreatePresentationDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path + "/Backgrounds" + "/Slides" + "/SlidesJSON");
            }
        }

        private string GetRandomBackground(string presentationDir)
        {
            var backgroundPaths = Directory.GetFiles(presentationDir + "/Backgrounds");
            backgroundPaths = backgroundPaths
               .Where(path => path.Contains(".png") || path.Contains(".jpg")).ToArray();
            return backgroundPaths[new Random().Next(backgroundPaths.Length)];
        }

        private string CreatePresentationId()
        {
            var currentDate = DateTime.Now.ToString();
            string[] charsToRemove = { ".", " ", ":" };
            foreach (var symbol in charsToRemove)
            {
                currentDate = currentDate.Replace(symbol, "_");
            }
            return currentDate;
        }

        private void SaveSlides(List<Slide> slides, string presentationDir)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            for (var i = 0; i < slides.Count; i++)
            {
                SlideSaver.Save(slides[i], presentationDir + "/Slides/" + i.ToString() + ".jpg");
                using (FileStream fs = new FileStream(presentationDir + "/SlidesJSON/" + i.ToString() + ".json", FileMode.OpenOrCreate))
                {
                    jsonFormatter.WriteObject(fs, slides[i]);
                }
            }
            ArchivePresentation(presentationDir);
        }

        private void ArchivePresentation(string presentationDir)
        {
            if (System.IO.File.Exists(presentationDir + "/Slides.zip"))
                System.IO.File.Delete(presentationDir + "/Slides.zip");
            ZipFile.CreateFromDirectory(presentationDir + "/Slides", presentationDir + "/Slides.zip");
        }
    }
}