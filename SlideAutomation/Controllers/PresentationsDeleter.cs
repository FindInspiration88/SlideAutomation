using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SlideAutomation.Controllers
{
    public static class PresentationsDeleter
    {
        public static List<Exception> ExceptionsLog => _exceptionsLog;
        private static List<Exception> _exceptionsLog;

        static PresentationsDeleter()
        {
            _exceptionsLog = new List<Exception>();
        }
        //Пример вызова удаления из класса HomeController
        //PresentationsDeleter.DeleteOldPresentations(Server.MapPath("~/Presentations/"),
        //new TimeSpan(0, 0, 30));
        public static void DeleteOldPresentations(string fromDirectory, TimeSpan olderThan)
        {
            try
            {
                DeleteAllCreatedPresentations(fromDirectory, olderThan);
            }
            catch (Exception e)
            {
                _exceptionsLog.Add(e);
            }
        }

        private static void DeleteAllCreatedPresentations(string fromDirectory, TimeSpan olderThan)
        {
            var presentationDirectories = Directory.GetDirectories(fromDirectory);
            var oldestCreationDate = DateTime.Now - olderThan;
            foreach (var presentationDirectory in presentationDirectories)
            {
                if (PresentationIsOld(presentationDirectory, oldestCreationDate))
                    Directory.Delete(presentationDirectory, true);
            }
        }

        private static bool PresentationIsOld(string presentationDirectory, DateTime oldestCreationDate)
        {
            return Directory.GetCreationTime(presentationDirectory) < oldestCreationDate;
        }
    }
}