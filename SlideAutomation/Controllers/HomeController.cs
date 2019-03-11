using System;
using System.Collections.Generic;
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
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public RedirectResult Slide(string slideText, string slideTitle, string slideBg, string slideDir, string slideId, string slideName)
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
            return Redirect("/Home/Slide/" + slideId + "/" + slideName);
        }

        [HttpPost]
        public RedirectResult Index(IEnumerable<HttpPostedFileBase> upload)
        {
            var presentationId = GetPresentationId();
            var presentationDir = Server.MapPath("~/Presentations/" + presentationId);
            CreatePresentationDir(presentationDir);
            var textsFile = upload.ToArray()[0];
            var backgroundsFile = upload.ToArray()[1];
            if (backgroundsFile != null && backgroundsFile.FileName.Contains(".zip"))
            {
                ExtractBackgrounds(presentationDir, backgroundsFile);
            }
            if (textsFile != null && textsFile.FileName.Contains(".txt"))
            {
                CreateSlidesFromTxt(presentationDir, textsFile);
            }
            RedirectToAction("Slide");
            return Redirect("/Home/Slide/0/" + presentationId);
        }

        private void ExtractBackgrounds(string presentationDir, HttpPostedFileBase backgroundsFile)
        {
            var fileName = Path.GetFileName(backgroundsFile.FileName);
            var filePath = presentationDir + "/" + fileName;
            backgroundsFile.SaveAs(filePath);
           try
           {
                ZipFile.ExtractToDirectory(filePath, presentationDir + "/Backgrounds");
           }
           catch (Exception e)
           {
                System.IO.File.Copy(Server.MapPath("~/Files/default.jpg"), presentationDir + "/Backgrounds/default.jpg");
           }

        }

        private void CreateSlidesFromTxt(string presentationDir, HttpPostedFileBase textsFile)
        {
            
            var fileName = Path.GetFileName(textsFile.FileName);
            var filePath = presentationDir + "/" + fileName;
            textsFile.SaveAs(filePath);
            StreamReader fileStream = new StreamReader(filePath, Encoding.Default);
            var slideTexts = fileStream.ReadToEnd()
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
        }

        private void CreatePresentationDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path + "/Backgrounds");
                Directory.CreateDirectory(path + "/Slides");
                Directory.CreateDirectory(path + "/SlidesJSON");
            }
        }

        private string GetRandomBackground(string presentationDir)
        {
            var BackgroundPaths = Directory.GetFiles(presentationDir + "/Backgrounds");
            BackgroundPaths = BackgroundPaths
               .Where(path => path.Contains(".png") || path.Contains(".jpg")).ToArray();
            return BackgroundPaths[new Random().Next(BackgroundPaths.Length)];
        }

        private string GetPresentationId()
        {
            var dateId = DateTime.Now.ToString();
            string[] charsToRemove = { ".", " ", ":" };
            foreach (var symbol in charsToRemove)
            {
                dateId = dateId.Replace(symbol, "");
            }
            return dateId;
        }

        [HttpGet]
        public ActionResult Slide(int id, string name)
        {
            var slidePaths = Directory.GetFiles(Server.MapPath("~/Presentations/" + name + "/Slides")).ToList(); // устарело
            var slides = slidePaths
              .Select(path => "~/Presentations/" + name + "/Slides/" + Path.GetFileName(path)).ToList();
            if (id >= slides.Count) id = id - 1;
            if (id < 0) id = 0;
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            var jsonPath = Server.MapPath("~/Presentations/" + name + "/SlidesJSON/" + id.ToString() + ".json");
            using (FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
            {
                Slide slide = (Slide)jsonFormatter.ReadObject(fs);
                ViewBag.SlideText = slide.Content;
                ViewBag.SlideTitle = slide.Title;
                ViewBag.BackgroundPath = slide.PathToBackgroundFile;
            }
            ViewBag.presDir = Server.MapPath("~/Presentations/" + name);
            ViewBag.SlideId = id;
            ViewBag.SlideName = name;
            ViewBag.SlidePath = slides[id];
            ViewBag.NextSlide = "~/Home/Slide/" + (id + 1).ToString() + "/" + name;
            ViewBag.PreviousSlide = "~/Home/Slide/" + (id - 1).ToString() + "/" + name;
            return View();
        }
        
        private void SaveSlides(List<Slide> slides, string presentationDir)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            for (var i = 0; i<slides.Count;i++)
            {
                SlideSaver.Save(slides[i], presentationDir + "/Slides/" + i.ToString() + ".jpg");
                using (FileStream fs = new FileStream(presentationDir + "/SlidesJSON/" + i.ToString() + ".json", FileMode.OpenOrCreate))
                {
                    jsonFormatter.WriteObject(fs, slides[i]);
                }
            }
        }

    }
}