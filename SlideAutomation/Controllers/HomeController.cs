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

        [HttpGet]
        public ActionResult Error()
        {
            return View();
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
                ViewBag.SlideText = slide.Text;
                ViewBag.SlideTitle = slide.Title;
                ViewBag.BackgroundPath = slide.PathToBackgroundPicture;
                ViewBag.Warning = (slide.PathToBackgroundPicture.Contains("default.jpg")) ?
                    "Был загружен стандартный фон, так как архив не был прочитан." : "";
            }
            ViewBag.presDir = Server.MapPath("~/Presentations/" + name);
            ViewBag.SlideId = id;
            ViewBag.SlideName = name;
            ViewBag.SlidePath = slides[id];
            ViewBag.NextSlide = "~/Home/Slide/" + (id + 1).ToString() + "/" + name;
            ViewBag.PreviousSlide = "~/Home/Slide/" + (id - 1).ToString() + "/" + name;
            ViewBag.DownloadLink = "~/Presentations/" + name + "/Slides.zip";
            return View();
        }
        [HttpPost]
        public RedirectResult Index(IEnumerable<HttpPostedFileBase> upload)
        {
            var presentationId = SlideProcessor.GetPresentationId();
            var presentationDir = Server.MapPath("~/Presentations/" + presentationId);
            SlideProcessor.CreatePresentationDir(presentationDir);
            var textsFile = upload.ToArray()[0];
            var backgroundsFile = upload.ToArray()[1];
            if (backgroundsFile == null
                || !backgroundsFile.FileName.Contains(".zip")
                || !SlideProcessor.IsBackgroundsExtracted(presentationDir, backgroundsFile))
            {
                SlideProcessor.LoadDefaultBackground(presentationDir,
                    Server.MapPath("~/Files/Backgrounds/default.jpg"),
                    presentationDir + "/Backgrounds/default.jpg");
            }
            if (textsFile != null && textsFile.FileName.Contains(".txt") && SlideProcessor.IsSlidesCreated(presentationDir, textsFile))
            {
                return Redirect("/Home/Slide/0/" + presentationId);
            }
            return Redirect("/Home/Error");
        }

        [HttpPost]
        public RedirectResult Slide(string slideText, string slideTitle, string slideBg, string slideDir, string slideId, string slideName)
        {
            var modifiedSlide = new Slide();
            modifiedSlide.Title = slideTitle;
            modifiedSlide.Text = slideText;
            modifiedSlide.PathToBackgroundPicture = slideBg;
            SlideSaver.SaveSlideAsJpeg(modifiedSlide, slideDir + "/Slides/" + slideId + ".jpg");
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
            using (FileStream fs = new FileStream(slideDir + "/SlidesJSON/" + slideId + ".json", FileMode.Create))
            {
                jsonFormatter.WriteObject(fs, modifiedSlide);
            }

            SlideProcessor.ArchivePresentation(slideDir);
            return Redirect("/Home/Slide/" + slideId + "/" + slideName);
        }
    }
}