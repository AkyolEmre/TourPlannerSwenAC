using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Exceptions;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using log4net;
using Tour_Planner.Models;

public class ReportGenerator
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ReportGenerator));

    public static void GenerateTourReport(IEnumerable<Tour> tours, string filePath)
    {
        // 0) Validation
        if (tours == null || !tours.Any())
        {
            MessageBox.Show(
                "Keine Tours zum Export gefunden.",
                "Export abgebrochen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        try
        {
            // 1) Zielverzeichnis anlegen
            var dir = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 2) PDF-Setup mit A4
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);

            // 3) Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var detailsFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // 4) Titel-Seite
            document.Add(new Paragraph("Complete Tours Report")
                .SetFont(titleFont)
                .SetFontSize(18)
                .SetMarginBottom(20));

            // 5) Für jede Tour eine Sektion
            foreach (var tour in tours)
            {
                // 5.1) Tour-Überschrift
                document.Add(new Paragraph($"Tour: {tour.Name}")
                    .SetFont(titleFont)
                    .SetFontSize(16)
                    .SetMarginTop(10)
                    .SetMarginBottom(5));

                // 5.2) Tour-Details
                var infoLines = new[]
                {
                    $"Von: {tour.From}",
                    $"Nach: {tour.To}",
                    $"Distanz: {tour.Distance/1000.0:F2} km",
                    $"Geschätzte Zeit: {tour.EstimatedTime} Sekunden",
                    $"Transport: {tour.TransportType}",
                    $"Beschreibung: {tour.Description}",
                    $"Beliebtheit: {tour.Popularity}",
                    $"Kinderfreundlichkeit: {tour.ChildFriendliness}"
                };
                foreach (var line in infoLines)
                    document.Add(new Paragraph(line)
                        .SetFont(detailsFont)
                        .SetFontSize(12)
                        .SetMarginBottom(3));

                // 5.3) TourLogs (falls vorhanden)
                if (tour.TourLogs?.Any() == true)
                {
                    document.Add(new Paragraph("Tour Logs:")
                        .SetFont(detailsFont)
                        .SetFontSize(14)
                        .SetMarginTop(10)
                        .SetMarginBottom(5));

                    var table = new Table(UnitValue.CreatePercentArray(new[] { 20f, 50f, 15f, 15f }))
                        .UseAllAvailableWidth();

                    table.AddHeaderCell("Datum");
                    table.AddHeaderCell("Kommentar");
                    table.AddHeaderCell("Rating");
                    table.AddHeaderCell("Dauer (s)");

                    foreach (var log in tour.TourLogs)
                    {
                        table.AddCell(log.DateTime.ToString("dd.MM.yyyy"));
                        table.AddCell(log.Comment ?? "");
                        table.AddCell(log.Rating.ToString());
                        table.AddCell(log.TotalTime ?? "0");
                    }

                    document.Add(table);
                }

                // 5.4) Seitenumbruch zwischen den Tours
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
            }
        }
        catch (Exception ex)
        {
            Log.Error("Fehler beim Generieren des Tours-Reports", ex);
            MessageBox.Show(
                $"Fehler beim Generieren des Tours-Reports:\n{ex.Message}",
                "Export-Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    public static void GenerateSummarizeReport(Tour tour, string filePath)
    {
        // 0) Validation
        if (tour == null || string.IsNullOrWhiteSpace(tour.Name))
        {
            MessageBox.Show(
                "Keine Tour ausgewählt oder Name fehlt",
                "Export abgebrochen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        try
        {
            // 1) Ensure output directory
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // 2) Setup PDF
            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);

            var logs = tour.TourLogs ?? Enumerable.Empty<TourLog>();
            var totalTimes = logs
                .Select(l => int.TryParse(
                                 l.TotalTime,
                                 NumberStyles.Any,
                                 CultureInfo.InvariantCulture,
                                 out var t)
                             ? t
                             : 0)
                .ToList();
            var totalDists = logs
                .Select(l => double.TryParse(
                                   l.TotalDistance,
                                   NumberStyles.Any,
                                   CultureInfo.InvariantCulture,
                                   out var d)
                               ? d
                               : 0.0)
                .ToList();
            var avgTime = totalTimes.Any() ? totalTimes.Average() : 0.0;
            var avgDistance = totalDists.Any() ? totalDists.Average() : 0.0;
            var avgRating = logs.Any() ? logs.Average(l => l.Rating) : 0.0;

            // 3) Fonts
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var detailsFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // 4) Title
            document.Add(new Paragraph($"Summarized Report: {tour.Name}")
                .SetFont(titleFont)
                .SetFontSize(18)
                .SetMarginBottom(20));

            // 5) Meta‐Infos
            var metaLines = new[]
            {
                $"Von: {tour.From}",
                $"Nach: {tour.To}",
                $"Distanz: {tour.Distance/1000.0:F2} km",
                $"Geschätzte Zeit: {tour.EstimatedTime} Sekunden",
                $"Transport: {tour.TransportType}",
                $"Beschreibung: {tour.Description}",
                $"Beliebtheit: {tour.Popularity}",
                $"Kinderfreundlichkeit: {tour.ChildFriendliness}",
                $"Durchschnittliche Bewertung: {avgRating:F1}/10",
            };
            foreach (var line in metaLines)
            {
                document.Add(new Paragraph(line)
                    .SetFont(detailsFont)
                    .SetFontSize(12)
                    .SetMarginBottom(5));
            }

            // 7) Statistics block
            document.Add(new Paragraph("Statistik:")
                .SetFont(detailsFont)
                .SetFontSize(14)
                .SetMarginTop(20)
                .SetMarginBottom(10));

            var statsLines = new[]
            {
                $"Durchschnittliche Distanz: {avgDistance/1000.0:F2} km",
                $"Durchschnittliche Zeit: {(int)avgTime} Sekunden",
            };
            foreach (var line in statsLines)
            {
                document.Add(new Paragraph(line)
                    .SetFont(detailsFont)
                    .SetFontSize(12)
                    .SetMarginBottom(5));
            }

            // 8) Logs‐Tabelle (optional)
            if (logs.Any())
            {
                document.Add(new Paragraph("Tour Logs:")
                    .SetFont(detailsFont)
                    .SetFontSize(14)
                    .SetMarginTop(20)
                    .SetMarginBottom(10));

                var table = new Table(UnitValue.CreatePercentArray(new[] { 20f, 50f, 15f, 15f }))
                    .UseAllAvailableWidth();

                table.AddHeaderCell("Datum");
                table.AddHeaderCell("Kommentar");
                table.AddHeaderCell("Rating");
                table.AddHeaderCell("Dauer");

                foreach (var log in logs)
                {
                    table.AddCell(log.DateTime.ToString("dd.MM.yyyy"));
                    table.AddCell(log.Comment ?? "");
                    table.AddCell(log.Rating.ToString());
                    var secs = int.TryParse(
                                   log.TotalTime,
                                   NumberStyles.Any,
                                   CultureInfo.InvariantCulture,
                                   out var t2)
                               ? t2
                               : 0;
                    table.AddCell(secs.ToString() + " Sekunden");
                }

                document.Add(table);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Fehler beim Generieren des Summarize-Reports", ex);
            MessageBox.Show(
                $"Fehler beim Generieren des Summarize-Reports:\n{ex.Message}",
                "Export-Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    /// <summary>
    /// Legacy-Overloaden, damit existing VM-Aufrufe ohne Pfad weiter funktionieren.
    /// </summary>
    public static void GenerateTourReport(Tour tour)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
        var filename = $"SingleTourReport_{tour.Name}_{ts}.pdf";
        var path = Path.Combine(docs, filename);

        // Wrap Tour in IEnumerable<Tour>
        GenerateTourReport(new[] { tour }, path);
    }

    public static void GenerateSummarizeReport(Tour tour)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
        var filename = $"SummarizedReport_{tour.Name}_{ts}.pdf";
        var path = Path.Combine(docs, filename);

        GenerateSummarizeReport(tour, path);
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
