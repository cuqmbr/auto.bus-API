using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Server.Data;
using Server.Models;

namespace Server.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _dbContext;

    public ReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool IsSucceed, string? message, Stream ticketPdf)> GetTicket(int ticketGroupId)
    {
        var dbTicketGroup = await _dbContext.TicketGroups
            .Include(tg => tg.User)
            .Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment)
            .ThenInclude(ve => ve.Vehicle)
            .ThenInclude(v => v.Company)
            .Include(tg => tg.User)
            .Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment)
            .ThenInclude(ve => ve.Route)
            .ThenInclude(r => r.RouteAddresses)
            .ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City)
            .ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .FirstOrDefaultAsync(tg => tg.Id == ticketGroupId);
        
        // Define document
        
        var document = new PdfDocument();
        document.Info.Title = "ticket";
        document.Info.Author = "auto.bus";
        
        // Craft document
        
        var pdfPage = document.AddPage();
        pdfPage.Width = XUnit.FromCentimeter(21.0);
        pdfPage.Height = XUnit.FromCentimeter(29.7);
            
        var gfx = XGraphics.FromPdfPage(pdfPage);
        // HACK²
        gfx.MUH = PdfFontEncoding.Unicode;

        var doc = new Document();
        doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(1);
        doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
        doc.DefaultPageSetup.MirrorMargins = true;
            
        DefineStyles(doc);
        CreatePage(doc);
        FillPage(doc, dbTicketGroup);

        var docRender = new DocumentRenderer(doc);
        docRender.PrepareDocument();
            
        docRender.RenderPage(gfx, 1);
        
        // Save document
        
        var memoryStream = new MemoryStream();
        document.Save(memoryStream);

        return (true, null, memoryStream);
        
        void DefineStyles(Document doc)
        {
            var styles = doc.Styles["Normal"];
            styles.Font.Name = "Courier New Cyr";

            styles = doc.Styles.AddStyle("Table", "Normal");
            styles.Font.Size = 10;
            styles.ParagraphFormat.SpaceBefore = 2.5;
            styles.ParagraphFormat.SpaceAfter = 2.5;
        }
        
        void CreatePage(Document doc)
        {
            var section = doc.AddSection();

            // Create header
            var paragraph = section.Headers.Primary.AddParagraph();
            paragraph.AddText("auto.bus");
            paragraph.Style = StyleNames.Header;
            paragraph.Format.Font.Size = 20;
            paragraph.Format.Font.Bold = true;

            paragraph = section.Headers.Primary.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 0;
            paragraph.Format.Borders.Top = new Border { Width = 1, Color = Colors.Black };
            paragraph.Format.Borders.Bottom = new Border { Color = Colors.Transparent };

            // Add title
            paragraph = section.AddParagraph();
            paragraph.AddText("Посадочний документ");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.Font.Bold = true;
            
            // Add break line before table
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 0;
            paragraph.Format.Borders.Top = new Border { Width = 1, Color = Colors.Black };
            paragraph.Format.Borders.Bottom = new Border { Color = Colors.Transparent };
            paragraph.Format.Borders.Style = BorderStyle.DashLargeGap;
            paragraph.Format.SpaceBefore = 5;
            paragraph.Format.SpaceAfter = 5;

            // Add table and define columns
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = Colors.Black;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;
            table.Rows.Height = Unit.FromPoint(15);
            table.Rows.VerticalAlignment = VerticalAlignment.Center;

            for (int i = 0; i < 12; i++)
            {
                var column = table.AddColumn(Unit.FromCentimeter(1.583));
                column.Format.Alignment = ParagraphAlignment.Center;
            }
            
            // Add break line after table
            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 0;
            paragraph.Format.Borders.Top = new Border { Width = 1, Color = Colors.Black };
            paragraph.Format.Borders.Bottom = new Border { Color = Colors.Transparent };
            paragraph.Format.Borders.Style = BorderStyle.DashLargeGap;
            paragraph.Format.SpaceBefore = 5;
            paragraph.Format.SpaceAfter = 5;
        }
        
        void FillPage(Document doc, TicketGroup ticketGroup)
        {
            var table = doc.LastSection.LastTable;

            var row = table.AddRow();
            table.AddRow();

            row.Cells[0].MergeRight = 2;
            row.Cells[0].MergeDown = 1;
            row.Cells[0].AddParagraph("aut.bus – м. Харків, просп. Науки, 14");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[3].MergeRight = 2;
            row.Cells[3].MergeDown = 1;
            row.Cells[3].AddParagraph("ПОСАДОЧНИЙ ДОКУМЕНТ");
            row.Cells[3].Format.Font.Bold = true;

            string ticketNums = "";
            foreach (var ticket in ticketGroup.Tickets)
            {
                ticketNums += $"{ticket.Id}";

                if (!ticketGroup.Tickets.Last().Equals(ticket))
                {
                    ticketNums += "; ";
                    continue;
                }

                ticketNums += ".";
            }
            
            row.Cells[6].MergeRight = 2;
            row.Cells[6].MergeDown = 1;
            row.Cells[6].AddParagraph($"Група: {ticketGroup.Id}.\nКвитки: {ticketNums}");
            row.Cells[6].Format.Alignment = ParagraphAlignment.Left;
            
            row.Cells[9].MergeRight = 2;
            row.Cells[9].MergeDown = 1;
            row.Cells[9].AddParagraph($"{ticketGroup.Tickets.First().PurchaseDateTimeUtc:dd.MM.yyyy HH:mm:ss}");

            row = table.AddRow();

            row.Cells[0].MergeRight = 2;
            row.Cells[0].AddParagraph("Прізвище, Ім'я");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[3].MergeRight = 8;
            row.Cells[3].AddParagraph($"{ticketGroup.User.LastName} {ticketGroup.User.FirstName}");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;

            // Fill stations
            
            row = table.AddRow();

            row.Cells[0].MergeRight = 11;
            row.Cells[0].AddParagraph("СТАНЦІЇ");
            row.Cells[0].Format.Font.Bold = true;
            
            var isFilled = false;
            
            for (var i = 0; i < ticketGroup.Tickets.Count; i++)
            {
                var ticket = ticketGroup.Tickets[i];
                
                var vehicle = ticket.VehicleEnrollment.Vehicle;

                var departureDateTimeUtc = GetDepartureTime(ticket);
                var arrivalDateTimeUtc = GetArrivalTime(ticket);

                var departureAddress = GetDepartureAddress(ticket);
                var arrivalAddress = GetArrivalAddress(ticket);

                row = table.AddRow();
                table.AddRow();
                row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
                isFilled = !isFilled;

                row.Cells[0].MergeRight = 1;
                row.Cells[0].MergeDown = 1;

                if (ticketGroup.Tickets.First().Equals(ticket))
                {
                    row.Cells[0].AddParagraph("Відправлення");
                }
                else
                {
                    row.Cells[0].AddParagraph("Пересадка на");
                }

                row.Cells[2].MergeRight = 1;
                row.Cells[2].MergeDown = 1;
                row.Cells[2].AddParagraph($"{departureDateTimeUtc:dd.MM.yyyy HH:mm}");

                row.Cells[4].MergeRight = 3;
                row.Cells[4].MergeDown = 1;
                row.Cells[4].AddParagraph($"{departureAddress}");

                row.Cells[8].MergeRight = 1;
                row.Cells[8].MergeDown = 1;
                row.Cells[8].AddParagraph("Тип, номер автобуса");

                row.Cells[10].MergeRight = 1;
                row.Cells[10].MergeDown = 1;
                row.Cells[10].AddParagraph($"{vehicle.Type}, {vehicle.Number}");

                row = table.AddRow();
                table.AddRow();
                row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
                isFilled = !isFilled;

                row.Cells[0].MergeRight = 1;
                row.Cells[0].MergeDown = 1;

                if (!ticketGroup.Tickets.Last().Equals(ticket))
                {
                    row.Cells[0].AddParagraph("Пересадка з");
                }
                else
                {
                    row.Cells[0].AddParagraph("Прибуття");
                }

                row.Cells[2].MergeRight = 1;
                row.Cells[2].MergeDown = 1;
                row.Cells[2].AddParagraph($"{arrivalDateTimeUtc:dd.MM.yyyy HH:mm}");

                row.Cells[4].MergeRight = 3;
                row.Cells[4].MergeDown = 1;
                row.Cells[4].AddParagraph($"{arrivalAddress}");

                row.Cells[8].MergeRight = 1;
                row.Cells[8].MergeDown = 1;
                row.Cells[8].AddParagraph("Тип, номер автобуса");

                row.Cells[10].MergeRight = 1;
                row.Cells[10].MergeDown = 1;
                row.Cells[10].AddParagraph($"{vehicle.Type}, {vehicle.Number}");

                if (!ticketGroup.Tickets.Last().Equals(ticket))
                {
                    var nextDepartureTimeUtc = GetDepartureTime(ticketGroup.Tickets[i + 1]);
                    var freeTime = nextDepartureTimeUtc - arrivalDateTimeUtc;
                    
                    row = table.AddRow();
                    table.AddRow();
                    row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
                    isFilled = !isFilled;
                    
                    row.Cells[0].MergeRight = 2;
                    row.Cells[0].MergeDown = 1;
                    row.Cells[0].AddParagraph("Вільний час");

                    row.Cells[3].MergeRight = 5;
                    row.Cells[3].MergeDown = 1;
                    row.Cells[3].AddParagraph($"{arrivalDateTimeUtc:dd.MM.yyyy HH:mm} – {nextDepartureTimeUtc:dd.MM.yyyy HH:mm}");

                    row.Cells[9].MergeRight = 2;
                    row.Cells[9].MergeDown = 1;
                    row.Cells[9].AddParagraph($"{freeTime.ToString(@"dd\.hh\:mm\:ss")}");
                }
            }

            // Fill value
            
            row = table.AddRow();

            row.Cells[0].MergeRight = 11;
            row.Cells[0].AddParagraph("ВАРТІСТЬ");
            row.Cells[0].Format.Font.Bold = true;
                
            isFilled = false;
            
            foreach (var ticket in ticketGroup.Tickets)
            {
                var departureAddress = GetDepartureAddress(ticket);
                var arrivalAddress = GetArrivalAddress(ticket);
                var cost = GetTicketCost(ticket);
                
                row = table.AddRow();
                table.AddRow();
                row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
                isFilled = !isFilled;
                
                row.Cells[0].MergeDown = 1;
                row.Cells[0].AddParagraph("Звідки");

                row.Cells[1].MergeRight = 3;
                row.Cells[1].MergeDown = 1;
                row.Cells[1].AddParagraph($"{departureAddress}");
                
                row.Cells[5].MergeDown = 1;
                row.Cells[5].AddParagraph("Куди");

                row.Cells[6].MergeRight = 3;
                row.Cells[6].MergeDown = 1;
                row.Cells[6].AddParagraph($"{arrivalAddress}");

                row.Cells[10].MergeDown = 1;
                row.Cells[10].AddParagraph("Ціна");

                row.Cells[11].MergeDown = 1;
                row.Cells[11].AddParagraph($"{cost}");
            }

            var totalCost = GetTicketGroupCost(ticketGroup);
            
            row = table.AddRow();
            row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
            isFilled = !isFilled;

            row.Cells[0].MergeRight = 9;
            row.Cells[0].AddParagraph("Загальна");
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;

            row.Cells[10].Borders.Right.Color = Colors.Transparent;
            
            row.Cells[11].AddParagraph($"{totalCost}");
        }

        DateTime GetDepartureTime(Ticket ticket)
        {
            var departureDateTimeUtc = ticket.VehicleEnrollment.DepartureDateTimeUtc;

            var routeAddresses = ticket.VehicleEnrollment.Route.RouteAddresses
                .OrderBy(ra => ra.Order).ToArray();
            
            foreach (var routeAddress in routeAddresses)
            {
                if (routeAddress.AddressId == ticket.FirstRouteAddressId)
                {
                    break;
                }

                departureDateTimeUtc += routeAddress.TimeSpanToNextCity;
                departureDateTimeUtc += routeAddress.WaitTimeSpan;
            }

            return departureDateTimeUtc;
        }

        DateTime GetArrivalTime(Ticket ticket)
        {
            var arrivalDateTimeUtc = ticket.VehicleEnrollment.DepartureDateTimeUtc;
            
            var routeAddresses = ticket.VehicleEnrollment.Route.RouteAddresses
                .OrderBy(ra => ra.Order).ToArray();
            
            foreach (var routeAddress in routeAddresses)
            {
                if (routeAddress.AddressId == ticket.LastRouteAddressId)
                {
                    break;
                }

                arrivalDateTimeUtc += routeAddress.TimeSpanToNextCity;
            }

            return arrivalDateTimeUtc;
        }

        Address GetDepartureAddress(Ticket ticket)
        {
            return ticket.VehicleEnrollment.Route.RouteAddresses
                .First(ra => ra.AddressId == ticket.FirstRouteAddressId)
                .Address;
        }

        Address GetArrivalAddress(Ticket ticket)
        {
            return ticket.VehicleEnrollment.Route.RouteAddresses
                .First(ra => ra.AddressId == ticket.LastRouteAddressId)
                .Address;
        }

        double GetTicketCost(Ticket ticket)
        {
            double cost = 0;

            var routeAddresses = ticket.VehicleEnrollment.Route.RouteAddresses
                .OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.AddressId != ticket.FirstRouteAddressId)
                .TakeWhile(ra => ra.AddressId != ticket.LastRouteAddressId)
                .ToArray();
            
            foreach (var routeAddress in routeAddresses)
            {
                cost += routeAddress.CostToNextCity;
            }
            
            return cost;
        }

        double GetTicketGroupCost(TicketGroup ticketGroup)
        {
            double cost = 0;

            foreach (var ticket in ticketGroup.Tickets)
            {
                cost += GetTicketCost(ticket);
            }

            return cost;
        }
    }

    public async Task<(bool isSucceed, string? message, Stream reportPdf)> GetCompanyReport()
    {
        throw new NotImplementedException();
    }
}