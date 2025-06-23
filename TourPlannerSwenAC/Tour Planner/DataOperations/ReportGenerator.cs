using System.Globalization;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tour_Planner.BL.GeoLocationAPI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tour_Planner.DAL;
using Tour_Planner.Models;
using System.Windows;

public class ReportGenerator
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(App));

    public static void GenerateTourReport(Tour tour)
    {
        Document document = new Document();

        try
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string reportFileName = $"SingleTourReport_{tour.Name}_{timestamp}.pdf";
            string filePath = Path.Combine(appDataDirectory, reportFileName);
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            // Add Title
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            Paragraph title = new Paragraph($"Tour Report for {tour.Name}", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // Add Tour Details
            Font detailsFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            document.Add(new Paragraph($"Description: {tour.Description}", detailsFont));
            document.Add(new Paragraph($"From: {tour.From}", detailsFont));
            document.Add(new Paragraph($"To: {tour.To}", detailsFont));
            document.Add(new Paragraph($"Transport Type: {tour.TransportType}", detailsFont));

            document.Add(new Paragraph($"Distance: {FormatMetersToKilometers(tour.Distance)}", detailsFont));
            document.Add(new Paragraph($"Estimated Time: {FormatSecondsToTime(tour.EstimatedTime)}", detailsFont));

            document.Add(new Paragraph($"Popularity: {tour.Popularity}", detailsFont));
            document.Add(new Paragraph($"Child-Friendliness: {tour.ChildFriendliness}", detailsFont));

            // Add Image
            if (!string.IsNullOrEmpty(tour.Img))
            {
                try
                {
                    Paragraph para = new Paragraph("Image:");
                    string imgurl = tour.Img;
                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imgurl);
                    jpg.ScaleToFit(500f, 500f);
                    jpg.SpacingBefore = 20f;
                    jpg.SpacingAfter = 20f;
                    jpg.Alignment = Element.ALIGN_CENTER;

                    document.Add(para);
                    document.Add(jpg);
                }

                catch (Exception ex)
                {
                    Log.Error($"Failed to load image from URL: {tour.Img}. Exception: {ex.Message}");
                    document.Add(new Paragraph($"Image: {tour.Img} (Failed to load image)", detailsFont));
                }
            }

            else
            {
                document.Add(new Paragraph("No image available.", detailsFont));
            }

            // Add Tour Logs
            if (tour.TourLogs != null && tour.TourLogs.Any())
            {
                document.Add(new Paragraph("\nTour Logs:", detailsFont));

                foreach (var log in tour.TourLogs)
                {
                    string formattedDistance = FormatMetersToKilometers(Convert.ToInt32(log.TotalDistance));
                    string formattedTime = FormatSecondsToTime(Convert.ToInt32(log.TotalTime));

                    document.Add(new Paragraph($"\nDate: {log.DateTime}, Comment: {log.Comment}, Difficulty: {log.Difficulty}, Distance: {formattedDistance}, Time: {formattedTime}, Rating: {log.Rating} / 10", detailsFont));
                }
            }

            else
            {
                document.Add(new Paragraph("No tour logs available.", detailsFont));
            }
        }

        catch (DocumentException ex)
        {
            Log.Info($"Download Exception TourReport:  {ex.Message}");
            Console.WriteLine($"An error occurred while generating PDF: {ex.Message}");
        }

        catch (IOException ex)
        {
            Log.Info($"IO Exception TourReport:  {ex.Message}");
            Console.WriteLine($"An error occurred while generating PDF: {ex.Message}");
        }

        finally
        {
            Log.Info($"Download Exception TourReport:  Worked");
            document.Close();
        }
    }

    public static void GenerateSummarizeReport(Tour tour)
    {
        Document document = new Document();

        try
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string reportFileName = $"SummarizedReport_{tour.Name}_{timestamp}.pdf";
            string filePath = Path.Combine(appDataDirectory, reportFileName);
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            // Add Title
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
            Font detailsFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            Paragraph title = new Paragraph($"Summarize Report for {tour.Name}", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            if (tour.TourLogs != null && tour.TourLogs.Any())
            {
                var avgTime = tour.TourLogs.Average(log => long.Parse(log.TotalTime));
                var avgDistance = tour.TourLogs.Average(log => double.Parse(log.TotalDistance));
                var avgRating = tour.TourLogs.Average(log => log.Rating);

                string formattedAvgTime = FormatSecondsToTime((int)(avgTime));
                string formattedAvgDistance = FormatMetersToKilometers((int)avgDistance);

                document.Add(new Paragraph($"Average Time: {formattedAvgTime}", detailsFont));
                document.Add(new Paragraph($"Average Distance: {formattedAvgDistance}", detailsFont));
                document.Add(new Paragraph($"Average Rating: {avgRating} / 10", detailsFont));
                document.Add(new Paragraph($"Popularity: {tour.Popularity}", detailsFont));
                document.Add(new Paragraph($"Child-Friendliness: {tour.ChildFriendliness}", detailsFont));
            }

            else
            {
                document.Add(new Paragraph("No tour logs available.", detailsFont));
            }
        }

        catch (DocumentException ex)
        {
            Log.Info($"Download Exception SummarizedTourReport:  {ex.Message}");
            Console.WriteLine($"An error occurred while generating PDF: {ex.Message}");
        }

        finally
        {
            Log.Info($"Download Exception SummarizedTourReport:  Worked");
            document.Close();
        }
    }

    public static string FormatMetersToKilometers(int meters)
    {
        double kilometers = meters / 1000.0;
        return $"{kilometers:F2} km";
    }

    public static string FormatSecondsToTime(int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return $"{(int)time.TotalHours}h {time.Minutes}min {time.Seconds}sec";
    }
}
