using System.IO;
using System.Text;
using System.Xml.Serialization;
using log4net;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Tour_Planner.DAL;
using Tour_Planner.Models;

public class ExportImportService
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(App));

    public static void ExportTourToFile(Tour tour)
    {
        try
        {
            ExportAsCSV(new List<Tour> { tour });
            Log.Info($"Single tour exported as CSV using shared method.");
        }
        catch (Exception ex)
        {
            Log.Info($"An error occurred while exporting single tour: {ex.Message}");
        }
    }

    public static Tour? ImportTourFromFile(string filePath, List<Tour> tours)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            Tour? importedTour = JsonConvert.DeserializeObject<Tour>(json);

            if (importedTour == null)
            {
                Log.Info("Import failed: deserialized tour is null.");
                return null;
            }

            // Prüfen, ob Tour mit gleicher ID schon existiert
            if (tours.Any(t => t.Id == importedTour.Id))
            {
                Log.Info($"Same Tour with ID {importedTour.Id} already exists. Skipping import.");
                return null;
            }

            // Prüfen, ob TourLogs mit gleicher ID schon existieren
            if (importedTour.TourLogs != null)
            {
                foreach (var tourLog in importedTour.TourLogs)
                {
                    foreach (var tour in tours)
                    {
                        if (tour.TourLogs.Any(log => log.Id == tourLog.Id))
                        {
                            Log.Info($"Same TourLog with ID {tourLog.Id} already exists. Skipping import.");
                            return null;
                        }
                    }

                    // Datum auf UTC setzen
                    tourLog.DateTime = tourLog.DateTime.ToUniversalTime();
                }
            }

            return importedTour;
        }
        catch (Exception ex)
        {
            Log.Info($"An error occurred while importing tour from file: {ex.Message}");
            return null;
        }
    }

    public static void ExportAsCSV(List<Tour> tours)
    {
        string downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"ToursWithLogs_{timeStamp}.csv";
        string filePath = Path.Combine(downloadsDirectory, fileName);

        var sb = new StringBuilder();
        sb.AppendLine("TourName;Description;From;To;Distance;EstimatedTime;Popularity;ChildFriendliness;Date;Comment;Difficulty;Distance;Time;Rating");

        foreach (var tour in tours)
        {
            if (tour.TourLogs != null && tour.TourLogs.Any())
            {
                foreach (var log in tour.TourLogs)
                {
                    sb.AppendLine($"{tour.Name};{tour.Description};{tour.From};{tour.To};{tour.Distance};{tour.EstimatedTime};{tour.Popularity};{tour.ChildFriendliness};{log.DateTime};{log.Comment};{log.Difficulty};{log.TotalDistance};{log.TotalTime};{log.Rating}");
                }
            }
            else
            {
                // Empty log placeholders
                sb.AppendLine($"{tour.Name};{tour.Description};{tour.From};{tour.To};{tour.Distance};{tour.EstimatedTime};{tour.Popularity};{tour.ChildFriendliness};;;;;;");
            }
        }

        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true)); // true = include BOM
        Log.Info($"Tours with logs exported successfully to CSV: {filePath}");
    }

    public static void ExportAsXML(List<Tour> tours)
    {
        string downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"Tours_{timeStamp}.xml";
        string filePath = Path.Combine(downloadsDirectory, fileName);

        try
        {
            var serializer = new XmlSerializer(typeof(List<Tour>));
            using (var stream = new StreamWriter(filePath))
            {
                serializer.Serialize(stream, tours);
            }

            Log.Info($"Tours exported successfully to XML: {filePath}");
        }
        catch (Exception ex)
        {
            Log.Info($"An error occurred while exporting tours to XML: {ex.Message}");
        }
    }

    public static void ExportAsJSON(List<Tour> tours)
    {
        string downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"Tours_{timeStamp}.json";
        string filePath = Path.Combine(downloadsDirectory, fileName);

        try
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            string json = JsonConvert.SerializeObject(tours, Formatting.Indented, settings);
            File.WriteAllText(filePath, json);

            Log.Info($"Tours exported successfully to JSON: {filePath}");
        }
        catch (Exception ex)
        {
            Log.Info($"An error occurred while exporting tours to JSON: {ex.Message}");
        }
    }

    public static void ExportAsPdf(List<Tour> tours)
    {
        string downloadsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"Tours_{timeStamp}.pdf";
        string filePath = Path.Combine(downloadsDirectory, fileName);

        try
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12, XFontStyle.Regular);

            double y = 40;
            gfx.DrawString("Exported Tours", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black,
                new XRect(0, 10, page.Width, page.Height), XStringFormats.TopCenter);

            foreach (var tour in tours)
            {
                string line = $"Name: {tour.Name}, From: {tour.From}, To: {tour.To}, Distance: {tour.Distance} km";
                gfx.DrawString(line, font, XBrushes.Black, new XPoint(40, y));
                y += 20;

                if (y > page.Height - 40)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            document.Save(fs);

            Log.Info($"Tours exported successfully to PDF: {filePath}");
        }
        catch (Exception ex)
        {
            Log.Info($"An error occurred while exporting tours to PDF: {ex.Message}");
        }
    }
}
