using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SlideAutomation.Models;
using System.IO;
using System.Text;
using System.Drawing;

namespace SlideAutomation.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public RedirectResult Index(IEnumerable<HttpPostedFileBase> upload)
        {

            var presentationId = GetPresentationId();
            var presentationDir = Server.MapPath("~/Presentations/" + presentationId);
            CreatePresentationDir(presentationDir);



            var textsFile = upload.ToArray()[0];
            var backgroundsFile = upload.ToArray()[1];

            if (textsFile != null)
                {
                    var fileName = Path.GetFileName(textsFile.FileName);
                    var filePath = presentationDir + "/" + fileName;
                    textsFile.SaveAs(filePath);    
                    if (fileName.Contains(".txt"))    
                        {
                            StreamReader fileStream = new StreamReader(filePath, Encoding.Default);
                            var slideTexts = fileStream.ReadToEnd()
                                .Split(new string[] { "СЛАЙД: ", "СЛАЙД:" }, StringSplitOptions.RemoveEmptyEntries);
                            var slides = new List<Slide>();
                            foreach(var slideText in slideTexts)
                            {
                                var slide = new Slide();
                                slide.Title = slideText.Split('\n')[0];
                                slide.Content = slideText.Replace(slide.Title, "");
                                slide.PathToBackgroundFile = GetRandomBackground();
                                slides.Add(slide);
                            }
                            CreateSlides(slides);
                        }        
                    }
            
            return Redirect("/Home/Slide/" + int.Parse(presentationId) + "/0");
            //return Redirect("/Home/Slide/");
        }

        private void CreatePresentationDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path + "/Backgrounds");
                Directory.CreateDirectory(path + "/Slides");
            }
        }

        private string GetRandomBackground()
        {
            var BackgroundPaths = Directory.GetFiles(Server.MapPath("~/Files/Backgrounds"));
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
            var slidePaths = Directory.GetFiles(Server.MapPath("~/Home/Slide/Slides/"));
            var slides = slidePaths.Select(path => "Slides/" + Path.GetFileName(path)).ToList();
            ViewBag.Slides = slides;
            return View();
        }

       

        private void CreateSlides(List<Slide> slides)
        {
            for (var i = 0; i<slides.Count;i++)
            {
                Image picture = Image.FromFile(slides[i].PathToBackgroundFile); //получаем исходное изображение из файла 
                var pictureToOurFormat = new Bitmap(picture, 800, 500); // форматируем изображение 
                Graphics part2 = Graphics.FromImage(pictureToOurFormat); //получаем его часть 
                part2.DrawString(slides[i].Content,
                new System.Drawing.Font("Arial", 26, FontStyle.Regular),
                new SolidBrush(Color.WhiteSmoke), new RectangleF(100, 100, 400, 500),
                new StringFormat(StringFormatFlags.NoClip)); // наносим на эту часть текст с параметрами 
                pictureToOurFormat.Save(Server.MapPath("~/Home/Slide/Slides/") + i.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);//записываем получающееся изображение в файл
            }
        }

    }
}