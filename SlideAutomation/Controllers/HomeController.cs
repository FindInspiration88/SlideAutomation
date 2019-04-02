using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SlideAutomation.Models;
using System.IO;
using System.Runtime.Serialization.Json;

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
            var slidePaths = GetPresentationPaths(name); // устарело
            var slides = GetSlidesPaths(name, slidePaths);
            id = CheckIdValidity(id, slides);
            LoadSlideViewFromJson(id, name, slides);
            return View();
        }

        private void LoadSlideViewFromJson(int id, string name, List<string> slides)
        {
            var jsonPath = Server.MapPath("~/Presentations/" + name + "/SlidesJSON/" + id.ToString() + ".json");
            using (FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Slide));
                Slide slide = (Slide)jsonFormatter.ReadObject(fs);
                FillViewBagWithSlide(id, name, slide, slides);
            }
        }

        private void FillViewBagWithSlide(int id, string name, Slide slide, List<string> slides)
        {
            ViewBag.SlideText = slide.Text;
            ViewBag.SlideTitle = slide.Title;
            ViewBag.BackgroundPath = slide.PathToBackgroundPicture;
            ViewBag.Warning = (slide.PathToBackgroundPicture.Contains("default.jpg"))
                ? "Был загружен стандартный фон, так как архив не был прочитан."
                : "";
            ViewBag.presDir = Server.MapPath("~/Presentations/" + name);
            ViewBag.SlideId = id;
            ViewBag.SlideName = name;
            ViewBag.SlidePath = slides[id];
            ViewBag.NextSlide = "~/Home/Slide/" + (id + 1).ToString() + "/" + name;
            ViewBag.PreviousSlide = "~/Home/Slide/" + (id - 1).ToString() + "/" + name;
            ViewBag.DownloadLink = "~/Presentations/" + name + "/Slides.zip";
        }

        private static int CheckIdValidity(int id, List<string> slides)
        {
            if (id >= slides.Count) id = id - 1;
            if (id < 0) id = 0;
            return id;
        }

        private static List<string> GetSlidesPaths(string name, List<string> slidePaths)
        {
            return slidePaths
                .Select(path => "~/Presentations/" + name + "/Slides/" + Path.GetFileName(path)).ToList();
        }

        private List<string> GetPresentationPaths(string name)
        {
            return Directory.GetFiles(Server.MapPath("~/Presentations/" + name + "/Slides")).ToList();
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