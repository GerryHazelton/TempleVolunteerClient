using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TempleVolunteerClient.Common
{
    public class ReportHeaderFooter : PdfPageEventHelper
    {
        private readonly string _title;
        public ReportHeaderFooter(string title)
        {
            this._title = title;
        }

        Font headerFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
        Font footerFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            Phrase header = new Phrase(_title, headerFont);
            Phrase footer = new Phrase(string.Format("Copyright {0} - Summer Day Program Portal", DateTime.Now.Year), footerFont);
            PdfContentByte cb = writer.DirectContent;
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, header, (document.Right - document.Left) / 2 + document.LeftMargin, document.Top + 10, 0);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, footer, (document.Right - document.Left) / 2 + document.LeftMargin, document.Bottom - 10, 0);
        }
    }
}
