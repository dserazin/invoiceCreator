using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

public class PDFHelper
{
    public static string txtCustomerName { get; set; }
    public static string txtCustomerAddress { get; set; }
    public static string txtCustomerCity { get; set; }
    public static string txtProductName { get; set; }
    public static string txtOverheadCost { get; set; }
    public static string txtProductionCost { get; set; }
    public static string txtSetupFee { get; set; }
    public static string txtProjectTransfer { get; set; }

    public static void GenerateInvoicePDF(string filePath, string customerName, string customerAddress, string customerCity, string productName, int quantity, decimal textilePrice, string calculationMethod, decimal totalPrice)
    {
        // Definiere benutzerdefinierte Schriftarten (Calibri)
        BaseFont calibri = BaseFont.CreateFont("Fonts/calibri.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        BaseFont calibriBold = BaseFont.CreateFont("Fonts/calibrib.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

        Font titleFont = new Font(calibriBold, 16);
        Font invoiceFont = new Font(calibriBold, 11);
        Font headerFont = new Font(calibri, 11);
        Font bodyFont = new Font(calibri, 9);
        Font footerFont = new Font(calibri, 8);

        // Variablen für Produktionskosten, Overhead-Kosten, Einrichtungsgebühr und Projektübergabe basierend auf der Methode
        //decimal overheadCost = decimal.TryParse(txtOverheadCost, out decimal parsedOverheadCost) ? parsedOverheadCost : 0.00m;
        //decimal productionCost = decimal.TryParse(txtProductionCost, out decimal parsedProductionCost) ? parsedProductionCost : 0.00m;
        //decimal setupFee = decimal.TryParse(txtSetupFee, out decimal parsedSetupFee) ? parsedSetupFee : 0.00m;
        //decimal projectTransfer = decimal.TryParse(txtProjectTransfer, out decimal parsedProjectTransfer) ? parsedProjectTransfer : 0.00m;

        decimal overheadCost = calculationMethod == "DTF" ? 6.00m : (calculationMethod == "Flock" ? 8.00m : 0.00m);
        decimal productionCost = calculationMethod == "DTF" ? 8.67m : (calculationMethod == "Flock" ? 12.67m : 0.00m);
        decimal setupFee = decimal.TryParse(txtSetupFee, out decimal parsedSetupFee) ? parsedSetupFee : 4.00m;
        decimal projectTransfer = decimal.TryParse(txtProjectTransfer, out decimal parsedProjectTransfer) ? parsedProjectTransfer : 2.30m;

        // Berechnung der Gesamtsummen für die Spalte "Summe netto"
        decimal setupFeeTotal = quantity * setupFee;
        decimal productionCostTotal = quantity * productionCost;
        decimal overheadCostTotal = quantity * overheadCost;
        decimal projectTransferTotal = quantity * projectTransfer;
        decimal textilePriceTotal = quantity * textilePrice;

        // Berechnung der Rechnungssumme
        totalPrice = setupFeeTotal + productionCostTotal + overheadCostTotal + projectTransferTotal + textilePriceTotal;

        Document doc = new Document(PageSize.A4);
        PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
        doc.Open();

        // Logo hinzufügen
        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("Images/spaShadow.png");
        logo.ScaleToFit(220, 60);
        logo.Alignment = Element.ALIGN_LEFT;
        doc.Add(logo);
        doc.Add(new Paragraph("\n\n\n\n")); // Mehr Platz zwischen Logo und Adressblock

        // Adressenbereich
        PdfPTable addressTable = new PdfPTable(2) { WidthPercentage = 100 };
        addressTable.SetWidths(new float[] { 1, 1 });

        PdfPCell cell = new PdfPCell(new Phrase(customerName + "\n" + customerAddress + "\n" + customerCity, bodyFont)) { Border = Rectangle.NO_BORDER };
        addressTable.AddCell(cell);

        cell = new PdfPCell(new Phrase(
            "\nSticky Prime Apparel\n" +
            "Sabinastraße 13\n" +
            "45136 Essen\n" +
            "Tel.: +49 173 5358880\n" +
            "E-Mail: info@stickyprime.de", bodyFont))
        {
            HorizontalAlignment = Element.ALIGN_RIGHT,
            Border = Rectangle.NO_BORDER,
        };
        addressTable.AddCell(cell);

        doc.Add(addressTable);

        // Rechnungsinformationen und Begrüßungstext
        doc.Add(new Paragraph("\nRechnung", titleFont));
        doc.Add(new Paragraph("Rechnungsnummer: 1001", bodyFont));
        doc.Add(new Paragraph(productName + "\n", bodyFont));
        doc.Add(new Paragraph("Rechnungsdatum: 26.06.2024", bodyFont));
        doc.Add(new Paragraph("Lieferdatum: Januar 2024", bodyFont));
        doc.Add(new Paragraph("Zahlbar bis: 14.07.2024\n\n", bodyFont));
        doc.Add(new Paragraph("Sehr geehrter Damen und Herren,", bodyFont));
        doc.Add(new Paragraph("Vielen Dank für Ihren Auftrag und Ihr Vertrauen.", bodyFont));
        doc.Add(new Paragraph("Für unsere Arbeit und Material stellen wir Ihnen folgende Summe in Rechnung:\n\n", bodyFont));

        // Tabelle für Artikel, Menge, Einzelpreis und Gesamt
        PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 1, 3, 1, 2, 2 });

        AddCellToHeader(table, "Pos", calibriBold);
        AddCellToHeader(table, "Artikel", calibriBold);
        AddCellToHeader(table, "Anzahl", calibriBold);
        AddCellToHeader(table, "Einzelpreis (€)", calibriBold);
        AddCellToHeader(table, "Summe netto (€)", calibriBold);

        AddCellToBody(table, "1", calibri);
        AddCellToBody(table, "Einrichtungsgebühr", calibri);
        AddCellToBody(table, quantity.ToString(), calibri);
        AddCellToBody(table, setupFee.ToString("0.00 €"), calibri);
        AddCellToBody(table, setupFeeTotal.ToString("0.00 €"), calibri);

        AddCellToBody(table, "2", calibri);
        AddCellToBody(table, "Produktionskosten", calibri);
        AddCellToBody(table, quantity.ToString(), calibri);
        AddCellToBody(table, productionCost.ToString("0.00 €"), calibri);
        AddCellToBody(table, productionCostTotal.ToString("0.00 €"), calibri);

        AddCellToBody(table, "3", calibri);
        AddCellToBody(table, "Overhead-Kosten", calibri);
        AddCellToBody(table, quantity.ToString(), calibri);
        AddCellToBody(table, overheadCost.ToString("0.00 €"), calibri);
        AddCellToBody(table, overheadCostTotal.ToString("0.00 €"), calibri);

        AddCellToBody(table, "4", calibri);
        AddCellToBody(table, "Projektübergabe", calibri);
        AddCellToBody(table, quantity.ToString(), calibri);
        AddCellToBody(table, projectTransfer.ToString("0.00 €"), calibri);
        AddCellToBody(table, projectTransferTotal.ToString("0.00 €"), calibri);

        AddCellToBody(table, "5", calibri);
        AddCellToBody(table, "Textilpreis", calibri);
        AddCellToBody(table, quantity.ToString(), calibri);
        AddCellToBody(table, textilePrice.ToString("0.00 €"), calibri);
        AddCellToBody(table, textilePriceTotal.ToString("0.00 €"), calibri);

        doc.Add(table);

        // Rechnungssumme
        doc.Add(new Paragraph($"\nRechnungssumme: {totalPrice:0.00 €}", invoiceFont));
        doc.Add(new Paragraph("Nach § 19 Abs. 1 UStG wird keine Umsatzsteuer berechnet.\n\n", invoiceFont));

        // Abschiedsgruß
        doc.Add(new Paragraph("Wir danken Ihnen für die gute Zusammenarbeit.\nMit besten Grüßen,\nDaniel Serazin\n\n\n\n\n\n", bodyFont));

        // Fußbereich / Spalten PdfPTable(4) und { 1, 1, 1, 1 } anpassen
        PdfPTable footerTable = new PdfPTable(4) { WidthPercentage = 100 };
        footerTable.SetWidths(new float[] { 1, 1, 1, 1 });

        // Icons im Footer
        iTextSharp.text.Image homeIcon = iTextSharp.text.Image.GetInstance("Images/home.png");
        homeIcon.ScaleToFit(20, 20);
        PdfPCell iconCell = new PdfPCell(homeIcon)
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER
        };
        footerTable.AddCell(iconCell);

        iTextSharp.text.Image contactIcon = iTextSharp.text.Image.GetInstance("Images/contact.png");
        contactIcon.ScaleToFit(20, 20);
        iconCell = new PdfPCell(contactIcon)
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER
        };
        footerTable.AddCell(iconCell);

        iTextSharp.text.Image bankIcon = iTextSharp.text.Image.GetInstance("Images/bank.png");
        bankIcon.ScaleToFit(20, 20);
        iconCell = new PdfPCell(bankIcon)
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER
        };
        footerTable.AddCell(iconCell);

        iTextSharp.text.Image ownerIcon = iTextSharp.text.Image.GetInstance("Images/owner.png");
        ownerIcon.ScaleToFit(20, 20);
        iconCell = new PdfPCell(ownerIcon)
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER
        };
        footerTable.AddCell(iconCell);

        // Linie vor dem Fußbereich hinzufügen
        PdfPTable lineTable = new PdfPTable(1) { WidthPercentage = 100 };
        PdfPCell lineCell = new PdfPCell(new Phrase(""))
        {
            Border = Rectangle.BOTTOM_BORDER,
            BorderWidthBottom = 1f,
            PaddingTop = 10f,
            PaddingBottom = 10f
        };
        lineTable.AddCell(lineCell);
        doc.Add(lineTable);
        doc.Add(new Paragraph("\n\n")); // Mehr Platz zwischen Linie und Icons
        Console.WriteLine("Line added to PDF.");

        AddCellToFooter(footerTable, "Sticky Prime Apparel\nSabinastraße 13\n45136 Essen", footerFont);
        AddCellToFooter(footerTable, "Tel.: +49 173 5358880\nMail: info@stickyprime.de\nWeb: www.stickyprime.de", footerFont);
        AddCellToFooter(footerTable, "Commerzbank\nIBAN: DE43 3604 0039 0400 7969 00\nBIC: COBADEFFXXX", footerFont);
        AddCellToFooter(footerTable, "Inhaber: Daniel Serazin\nUSt.-IdNr. DE368324868", footerFont);

        doc.Add(footerTable);

        doc.Close();
    }

    // Abstände einstellen
    private static void AddCellToHeader(PdfPTable table, string text, BaseFont font)
    {
        Font headerFont = new Font(font, 10, Font.BOLD);
        PdfPCell cell = new PdfPCell(new Phrase(text, headerFont))
        {
            BackgroundColor = new BaseColor(230, 230, 230),
            HorizontalAlignment = Element.ALIGN_CENTER,
            Padding = 5
        };
        table.AddCell(cell);
    }

    private static void AddCellToBody(PdfPTable table, string text, BaseFont font)
    {
        Font bodyFont = new Font(font, 10);
        PdfPCell cell = new PdfPCell(new Phrase(text, bodyFont))
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            Padding = 5
        };
        table.AddCell(cell);
    }

    private static void AddCellToFooter(PdfPTable table, string text, Font font)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, font))
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_LEFT
        };
        table.AddCell(cell);
    }
}
