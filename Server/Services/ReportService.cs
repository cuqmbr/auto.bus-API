using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Server.Data;
using Server.Models;
using Route = Server.Models.Route;

namespace Server.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _dbContext;

    public ReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool IsSucceed, IActionResult? actionResult, Stream ticketPdf)> GetTicket(int ticketGroupId)
    {
        if (!await DoesTicketGroupExist(ticketGroupId))
        {
            return (false, new NotFoundResult(), null!);
        }
        
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
            
            .Include(tg => tg.User)
            .Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment)
            .ThenInclude(ve => ve.RouteAddressDetails)
            
            .FirstAsync(tg => tg.Id == ticketGroupId);
        
        // Define document
        
        var document = new PdfDocument();
        document.Info.Title = "ticket";
        document.Info.Author = "auto.bus";
        
        // Craft document
        
        var pdfPage = document.AddPage();
        pdfPage.Size = PageSize.A4;
            
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
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].AddParagraph($"{departureAddress.GetFullName()}");

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
                row.Cells[4].Format.Font.Size = 8;
                row.Cells[4].AddParagraph($"{arrivalAddress.GetFullName()}");

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
                row.Cells[1].Format.Font.Size = 8;
                row.Cells[1].AddParagraph($"{departureAddress.GetFullName()}");
                
                row.Cells[5].MergeDown = 1;
                row.Cells[5].AddParagraph("Куди");

                row.Cells[6].MergeRight = 3;
                row.Cells[6].MergeDown = 1;
                row.Cells[6].Format.Font.Size = 8;
                row.Cells[6].AddParagraph($"{arrivalAddress.GetFullName()}");

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
                var details = routeAddress.RouteAddressDetails
                    .First(rad => rad.RouteAddressId == routeAddress.Id);
                
                if (routeAddress.AddressId == ticket.FirstRouteAddressId)
                {
                    departureDateTimeUtc += details.WaitTimeSpan;
                    break;
                }

                departureDateTimeUtc += details.TimeSpanToNextCity;
                departureDateTimeUtc += details.WaitTimeSpan;
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
                var details = routeAddress.RouteAddressDetails
                    .First(rad => rad.RouteAddressId == routeAddress.Id);
                
                if (routeAddress.AddressId == ticket.LastRouteAddressId)
                {
                    break;
                }

                arrivalDateTimeUtc += details.TimeSpanToNextCity;
                arrivalDateTimeUtc += details.WaitTimeSpan;
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
                var details = routeAddress.RouteAddressDetails
                    .First(rad => rad.RouteAddressId == routeAddress.Id);
                
                cost += details.CostToNextCity;
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

        async Task<bool> DoesTicketGroupExist(int id)
        {
            return await _dbContext.TicketGroups.AnyAsync(tg => tg.Id == id);
        }
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, Stream reportPdf)> 
        GetCompanyReport(int companyId, DateTime fromDate, DateTime toDate)
    {
        if (!await DoesCompanyExist(companyId))
        {
            return (false, new NotFoundResult(), null!);
        }

        var dbCompany = await _dbContext.Companies
            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.Reviews)

            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.Tickets)
            .ThenInclude(t => t.TicketGroup)

            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.Route)
            .ThenInclude(r => r.RouteAddresses)
            .ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City)
            .ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)

            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.RouteAddressDetails)

            .Select(c => new {
                Company = c,
                VehicleEnrollments = c.Vehicles
                    .Select(v => new {
                        Vehicle = v,
                        VehicleEnrollments = v.VehicleEnrollments.Where(ve =>
                            ve.DepartureDateTimeUtc.Date >= fromDate &&
                            ve.DepartureDateTimeUtc <= toDate)
                    })
            })
            .Select(o => o.Company)
            .FirstAsync(c => c.Id == companyId);

        var routesEnrolled = new List<Route>();
        foreach (var vehicle in dbCompany.Vehicles)
        {
            foreach (var vehicleEnrollment in vehicle.VehicleEnrollments)
            {
                var route = vehicleEnrollment.Route;
                if (!routesEnrolled.Contains(route))
                {
                    routesEnrolled.Add(route);
                }
            }
        }

        var vehicleEnrolled = dbCompany.Vehicles;
        
        // Define document
        
        var document = new PdfDocument();
        document.Info.Title = "ticket";
        document.Info.Author = "auto.bus";

        var doc = new Document();
        doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(1);
        doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
        doc.DefaultPageSetup.MirrorMargins = true;
            
        DefineStyles(doc);
        CreatePage(doc);
        FillPage(doc);

        var docRender = new DocumentRenderer(doc);
        docRender.PrepareDocument();

        var pageCount = docRender.FormattedDocument.PageCount;
        for (int i = 0; i < pageCount; i++)
        {
            var pdfPage = document.AddPage();
            pdfPage.Size = PageSize.A4;

            using var gfx = XGraphics.FromPdfPage(pdfPage);
            
            // HACK²
            gfx.MUH = PdfFontEncoding.Unicode;

            docRender.RenderPage(gfx, i + 1);
        }

        // Save document
        
        var memoryStream = new MemoryStream();
        document.Save(memoryStream);

        return (true, null, memoryStream);

        void DefineStyles(Document doc)
        {
            var styles = doc.Styles["Normal"];
            styles.Font.Name = "Courier New Cyr";
            styles.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;

            styles = doc.Styles.AddStyle("Table", "Normal");
            styles.Font.Size = 10;
            styles.ParagraphFormat.SpaceBefore = 2.5;
            styles.ParagraphFormat.SpaceAfter = 2.5;
            styles.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        }

        void CreatePage(Document doc)
        {
            var section = doc.AddSection();

            // Create footer
            // var paragraph = section.Footers.Primary.AddParagraph();
            // paragraph.Format.Alignment = ParagraphAlignment.Center;
            // paragraph.AddPageField();
            // section.Footers.
            // section.PageSetup.DifferentFirstPageHeaderFooter = true;
            
            
            var paragraph = section.AddParagraph("auto.bus");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(5);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(5);

            paragraph = section.AddParagraph("Фінансовий звіт");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            
            paragraph = section.AddParagraph($"{dbCompany.Name}");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(5);
            
            paragraph = section.AddParagraph($"{fromDate:dd.MM.yyyy} – {toDate:dd.MM.yyyy}");
            paragraph.Format.Font.Size = 20;
            paragraph.Format.LineSpacingRule = LineSpacingRule.OnePtFive;
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            Table table;

            // Section and table for each enrolled route
            for (int i = 0; i < routesEnrolled.Count; i++)
            {
                section = doc.AddSection();
                
                // Add table and define columns
                table = section.AddTable();
                table.Style = "Table";
                table.Borders.Color = Colors.Black;
                table.Borders.Width = 0.25;
                table.Borders.Left.Width = 0.5;
                table.Borders.Right.Width = 0.5;
                table.Rows.LeftIndent = 0;
                table.Rows.Height = Unit.FromPoint(15);
                table.Rows.VerticalAlignment = VerticalAlignment.Center;

                for (int j = 0; j < 12; j++)
                {
                    var column = table.AddColumn(Unit.FromCentimeter(1.583));
                    column.Format.Alignment = ParagraphAlignment.Center;
                }
            }
            
            // Section for total
            section = doc.AddSection();
        }
        
        void FillPage(Document doc)
        {
            Section section;
            Paragraph paragraph;

            int i = 1;
            foreach (var route in routesEnrolled)
            {
                section = doc.Sections[i];
                paragraph = section.Footers.Primary.AddParagraph();
                paragraph.AddPageField();
                paragraph.Format.Alignment = ParagraphAlignment.Center;
                
                var table = section.LastTable;

                var row = table.AddRow();
                
                row.Cells[0].MergeRight = 11;
                row.Cells[0].AddParagraph($"МАРШРУТ №{route.Id}");
                row.Cells[0].Format.Font.Bold = true;
                
                row = table.AddRow();

                row.Cells[0].MergeRight = 1;
                row.Cells[0].AddParagraph("Відправлення");

                row.Cells[2].MergeRight = 3;
                row.Cells[2].AddParagraph($"{route.RouteAddresses.First().Address.GetFullName()}");

                row.Cells[6].MergeRight = 1;
                row.Cells[6].AddParagraph("Прибуття");

                row.Cells[8].MergeRight = 3;
                row.Cells[8].AddParagraph($"{route.RouteAddresses.Last().Address.GetFullName()}");
                
                row = table.AddRow();
                
                row.Cells[0].MergeRight = 11;
                row.Cells[0].AddParagraph("КОРОТКО");
                row.Cells[0].Format.Font.Bold = true;

                row = table.AddRow();

                row.Cells[0].MergeRight = 1;
                row.Cells[0].AddParagraph("Поїздок проведено");

                row.Cells[2].MergeRight = 1;
                row.Cells[2].AddParagraph("Поїздок скасовано");

                row.Cells[4].MergeRight = 1;
                row.Cells[4].AddParagraph("Квитків продано");

                row.Cells[6].MergeRight = 1;
                row.Cells[6].AddParagraph("Неповних квитків");

                row.Cells[8].MergeRight = 1;
                row.Cells[8].AddParagraph("Грошей зароблено");

                row.Cells[10].MergeRight = 1;
                row.Cells[10].AddParagraph("Середній рейтинг");

                row = table.AddRow();
                row.Shading.Color = Color.FromRgbColor(25, Colors.Black);

                row.Cells[0].MergeRight = 1;
                row.Cells[0].AddParagraph($"{GetRouteEnrollmentCount(route)}");

                row.Cells[2].MergeRight = 1;
                row.Cells[2].AddParagraph($"{GetRouteCanceledEnrollmentCount(route)}");

                row.Cells[4].MergeRight = 1;
                row.Cells[4].AddParagraph($"{GetRouteSelledTicketCount(route)}");

                row.Cells[6].MergeRight = 1;
                row.Cells[6].AddParagraph($"{GetRouteIndirectTicketCount(route)}");

                row.Cells[8].MergeRight = 1;
                row.Cells[8].AddParagraph($"{GetRouteTotalRevenu(route)}");

                row.Cells[10].MergeRight = 1;
                var routeAverageRating = GetRouteAvarageRating(route);
                row.Cells[10].AddParagraph($"{(routeAverageRating == 0 ? "-" : routeAverageRating)}");
                
                row = table.AddRow();
                
                row.Cells[0].MergeRight = 11;
                row.Cells[0].AddParagraph("ДОКЛАДНО");
                row.Cells[0].Format.Font.Bold = true;

                row = table.AddRow();

                row.Cells[0].MergeRight = 2;
                row.Cells[0].AddParagraph("Ідентифікатор, тип та номер транспорту");

                row.Cells[3].MergeRight = 1;
                row.Cells[3].AddParagraph("Поїздок заплановано, проведена та скасовано");

                row.Cells[5].MergeRight = 2;
                row.Cells[5].AddParagraph("Квитків продано та повернено, з яких неповні");

                row.Cells[8].MergeRight = 1;
                row.Cells[8].AddParagraph("Грошей зароблено");

                row.Cells[10].MergeRight = 1;
                row.Cells[10].AddParagraph("Середній рейтинг");

                var isFilled = true;
                foreach (var vehicle in vehicleEnrolled)
                {
                    if (route.VehicleEnrollments.Count(ve =>
                            ve.VehicleId == vehicle.Id) == 0)
                    {
                        continue;
                    }
                    
                    row = table.AddRow();
                    row.Shading.Color = isFilled ? Color.FromRgbColor(25, Colors.Black) : Colors.White;
                    isFilled = !isFilled;
                    
                    row.Cells[0].MergeRight = 2;
                    row.Cells[0].AddParagraph($"{vehicle.Id}, {vehicle.Type}, {vehicle.Number}");

                    var executedEnrollmentCount = GetVehicleEnrollmentCount(vehicle, route.Id);
                    var canceledEnrollmentCount = GetVehicleCanceledEnrollmentCount(vehicle, route.Id);
                    row.Cells[3].MergeRight = 1;
                    row.Cells[3].AddParagraph($"{executedEnrollmentCount + canceledEnrollmentCount}, " +
                                              $"{executedEnrollmentCount}, {canceledEnrollmentCount}");

                    row.Cells[5].MergeRight = 2;
                    row.Cells[5].AddParagraph($"{GetVehicleSelledTicketCount(vehicle, route.Id)}, {GetVehicleReturnedTicketCount(vehicle, route.Id)}; " +
                                              $"{GetVehicleIndirectTicketCount(vehicle, route.Id)}, {GetVehicleReturnedIndirectTicketCount(vehicle, route.Id)}");

                    row.Cells[8].MergeRight = 1;
                    row.Cells[8].AddParagraph($"{GetVehicleTotalRevenue(vehicle, route.Id)}");

                    row.Cells[10].MergeRight = 1;
                    var vehicleAverageRating = GetVehicleAverageRating(vehicle, route.Id);
                    row.Cells[10].AddParagraph($"{(vehicleAverageRating == 0 ? "-" : vehicleAverageRating)}");
                }
                
                i++;
            }

            section = doc.Sections[doc.Sections.Count - 1];

            paragraph = section.AddParagraph("ПІДСУМОК");
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 14;
            paragraph.Format.Font.Bold = true;
            section.AddParagraph();

            var totalEnrollmentCount = GetTotalEnrollmentCount(dbCompany);
            var totalCanceledEnrollmentCount = GetTotalCanceledEnrollmentCount(dbCompany);
            var totalSoldTicketCount = GetTotalSoldTickets(dbCompany);
            var totalReturnedTicketCount = GetTotalReturnedTicketCount(dbCompany);
            var totalRevenue = GetTotalRevenu(dbCompany);
            var totalAverageRating = GetTotalAverageRating(dbCompany);

            paragraph = section.AddParagraph(
                $"У період з {fromDate:dd.MM.yyyy} по {toDate:dd.MM.yyyy} " +
                $"({(toDate - fromDate).Days} днів) компанією {dbCompany.Name} " +
                $"було заплановано {(totalEnrollmentCount)} поїздки, " +
                $"з яких {totalCanceledEnrollmentCount} було скасовано, " +
                $"продано {totalSoldTicketCount} квитків, " +
                $"з яких {totalReturnedTicketCount} було повернено. " +
                $"За цей час було зароблено {totalRevenue} гривень. " +
                $"Середній рейтинг по всім поїздкам: {totalAverageRating}");
            paragraph.Format.Alignment = ParagraphAlignment.Justify;
            paragraph.Format.Font.Size = 14;
            
            int GetRouteEnrollmentCount(Route route)
            {
                return route.VehicleEnrollments.Count(ve => !ve.IsCanceled);
            }

            int GetVehicleEnrollmentCount(Vehicle vehicle, int routeId)
            {
                return vehicle.VehicleEnrollments.Count(ve =>
                    !ve.IsCanceled && ve.RouteId == routeId);
            }

            int GetTotalEnrollmentCount(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleEnrollmentCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }
            
            int GetRouteCanceledEnrollmentCount(Route route)
            {
                return route.VehicleEnrollments.Count(ve => ve.IsCanceled);
            }
            
            int GetVehicleCanceledEnrollmentCount(Vehicle vehicle, int routeId)
            {
                return vehicle.VehicleEnrollments.Count(ve =>
                    ve.IsCanceled && ve.RouteId == routeId);
            }

            int GetTotalCanceledEnrollmentCount(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleCanceledEnrollmentCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }

            int GetRouteSelledTicketCount(Route route)
            {
                int result = 0;

                foreach (var enrollment in route.VehicleEnrollments)
                {
                    result += enrollment.Tickets.Count(t => !t.IsReturned);
                }

                return result;
            }

            int GetVehicleSelledTicketCount(Vehicle vehicle, int routeId)
            {
                int result = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    result += enrollment.Tickets.Count(t => !t.IsReturned);
                }

                return result;
            }

            int GetTotalSoldTickets(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleSelledTicketCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }
            
            int GetVehicleReturnedTicketCount(Vehicle vehicle, int routeId)
            {
                int result = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    result += enrollment.Tickets.Count(t => t.IsReturned);
                }

                return result;
            }

            int GetTotalReturnedTicketCount(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleReturnedTicketCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }

            int GetRouteIndirectTicketCount(Route route)
            {
                int result = 0;

                int departureAddressId = route.RouteAddresses.First().AddressId;
                int arrivalAddressId = route.RouteAddresses.Last().AddressId;
                
                foreach (var enrollment in route.VehicleEnrollments)
                {
                    result += enrollment.Tickets.Count(t => !t.IsReturned &&
                        t.FirstRouteAddressId != departureAddressId ||
                        t.LastRouteAddressId != arrivalAddressId);
                }

                return result;
            }

            int GetVehicleIndirectTicketCount(Vehicle vehicle, int routeId)
            {
                int result = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
                    var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
                    
                    result += enrollment.Tickets.Count(t => !t.IsReturned &&
                        t.FirstRouteAddressId != departureRouteAddressId ||
                        t.LastRouteAddressId != arrivalRouteAddressId);
                }

                return result;
            }

            int GetTotalIndirectTicketCount(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleIndirectTicketCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }
            
            int GetVehicleReturnedIndirectTicketCount(Vehicle vehicle, int routeId)
            {
                int result = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
                    var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
                    
                    result += enrollment.Tickets.Count(t => t.IsReturned &&
                        (t.FirstRouteAddressId != departureRouteAddressId ||
                        t.LastRouteAddressId != arrivalRouteAddressId));
                }

                return result;
            }

            int GetTotalReturnedIndirectTicketCount(Company company)
            {
                int result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleReturnedIndirectTicketCount(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }

            double GetRouteTotalRevenu(Route route)
            {
                double result = 0;

                foreach (var enrollment in route.VehicleEnrollments)
                {
                    foreach (var ticket in enrollment.Tickets)
                    {
                        result += GetTicketCost(ticket);
                    }
                }

                return result;
            }

            double GetVehicleTotalRevenue(Vehicle vehicle, int routeId)
            {
                double result = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    foreach (var ticket in enrollment.Tickets)
                    {
                        result += GetTicketCost(ticket);
                    }
                }

                return result;
            }

            double GetTotalRevenu(Company company)
            {
                double result = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        result += GetVehicleTotalRevenue(vehicle, enrollment.RouteId);
                    }
                }

                return result;
            }

            double GetRouteAvarageRating(Route route)
            {
                double result = 0;
                int reviewCount = 0;

                foreach (var enrollment in route.VehicleEnrollments)
                {
                    if (enrollment.Reviews.Count == 0)
                    {
                        continue;
                    }
                    
                    foreach (var review in enrollment.Reviews)
                    {
                        result += review.Rating;
                        reviewCount += enrollment.Reviews.Count;  
                    }
                }

                result /= reviewCount;
                result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
                
                return result;
            }

            double GetVehicleAverageRating(Vehicle vehicle, int routeId)
            {
                double result = 0;
                int reviewCount = 0;

                foreach (var enrollment in vehicle.VehicleEnrollments
                             .Where(ve => ve.RouteId == routeId))
                {
                    reviewCount += enrollment.Reviews.Count;
                    
                    foreach (var review in enrollment.Reviews)
                    {
                        result += review.Rating;
                    }
                }

                result /= reviewCount;
                result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;

                return Math.Round(result, 3);
            }

            double GetTotalAverageRating(Company company)
            {
                double result = 0;
                int enrollmentCount = 0;

                foreach (var vehicle in company.Vehicles)
                {
                    foreach (var enrollment in vehicle.VehicleEnrollments)
                    {
                        if (enrollment.Reviews.Count == 0)
                        {
                            continue;
                        }
                        
                        result += GetVehicleAverageRating(vehicle, enrollment.RouteId);
                        enrollmentCount++;
                    }
                }

                result /= enrollmentCount;
                result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
                
                return result;
            }
            
            // TODO: repeated function, yoinked from ticket generation
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
                    var details = routeAddress.RouteAddressDetails
                        .First(rad => rad.RouteAddressId == routeAddress.Id);
                
                    cost += details.CostToNextCity;
                }
            
                return cost;
            }
        }
        
        async Task<bool> DoesCompanyExist(int id)
        {
            return await _dbContext.Companies.AnyAsync(c => c.Id == id);
        }
    }
}