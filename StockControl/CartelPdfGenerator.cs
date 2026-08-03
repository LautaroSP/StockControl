using PdfSharp.Drawing;
using PdfSharp.Pdf;
using StockControl.Domain;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;

namespace StockControl
{
    public enum CartelTamano
    {
        Grande,
        Chico
    }

    public static class CartelPdfGenerator
    {
        private const float Dpi = 150f;
        private const float PageWidthMm = 210f;
        private const float PageHeightMm = 297f;
        private const float MarginMm = 5f;
        private const float GapMm = 2.5f;

        // Densidad: 3x2 = 6 grandes / 6x5 = 30 chicos
        private const float GrandeWidthMm = 64f;
        private const float GrandeHeightMm = 114f;
        private const float ChicoWidthMm = 32f;
        private const float ChicoHeightMm = 57f;

        public static int CartelesPorHoja(CartelTamano tamano) =>
            tamano == CartelTamano.Grande ? 6 : 30;

        public static int CalcularHojas(int cantidad, CartelTamano tamano)
        {
            if (cantidad <= 0) return 0;
            int porHoja = CartelesPorHoja(tamano);
            return (int)Math.Ceiling(cantidad / (double)porHoja);
        }

        public static string Generar(IList<CartelItem> items, CartelTamano tamano)
        {
            string plantillaPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Carteles",
                tamano == CartelTamano.Grande ? "grande.png" : "Chico.png");

            if (!File.Exists(plantillaPath))
                throw new FileNotFoundException("No se encontró la plantilla del cartel.", plantillaPath);

            float cartelW = tamano == CartelTamano.Grande ? GrandeWidthMm : ChicoWidthMm;
            float cartelH = tamano == CartelTamano.Grande ? GrandeHeightMm : ChicoHeightMm;
            int cols = tamano == CartelTamano.Grande ? 3 : 6;
            int rows = tamano == CartelTamano.Grande ? 2 : 5;
            int porHoja = cols * rows;

            using var plantilla = Image.FromFile(plantillaPath);
            var culture = new CultureInfo("es-AR");

            using var document = new PdfDocument();
            document.Info.Title = "Carteles de precio";

            var streams = new List<MemoryStream>();
            try
            {
                for (int i = 0; i < items.Count; i += porHoja)
                {
                    var pageItems = items.Skip(i).Take(porHoja).ToList();
                    using var pageBitmap = RenderPage(pageItems, plantilla, cartelW, cartelH, cols, rows, culture);
                    var ms = new MemoryStream();
                    pageBitmap.Save(ms, ImageFormat.Jpeg);
                    ms.Position = 0;
                    streams.Add(ms);

                    var page = document.AddPage();
                    page.Width = XUnit.FromMillimeter(PageWidthMm);
                    page.Height = XUnit.FromMillimeter(PageHeightMm);

                    using var xImage = XImage.FromStream(ms);
                    using var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(xImage, 0, 0, page.Width.Point, page.Height.Point);
                }

                string path = Path.Combine(Path.GetTempPath(), $"carteles_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                document.Save(path);
                return path;
            }
            finally
            {
                foreach (var s in streams)
                    s.Dispose();
            }
        }

        private static Bitmap RenderPage(
            List<CartelItem> items,
            Image plantilla,
            float cartelWMm,
            float cartelHMm,
            int cols,
            int rows,
            CultureInfo culture)
        {
            int pageW = MmToPx(PageWidthMm);
            int pageH = MmToPx(PageHeightMm);
            int margin = MmToPx(MarginMm);
            int gap = MmToPx(GapMm);
            int cartelW = MmToPx(cartelWMm);
            int cartelH = MmToPx(cartelHMm);

            var bmp = new Bitmap(pageW, pageH);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            for (int idx = 0; idx < items.Count; idx++)
            {
                int col = idx % cols;
                int row = idx / cols;
                int x = margin + col * (cartelW + gap);
                int y = margin + row * (cartelH + gap);
                DrawCartel(g, plantilla, items[idx], x, y, cartelW, cartelH, culture);
            }

            return bmp;
        }

        private static void DrawCartel(
            Graphics g,
            Image plantilla,
            CartelItem item,
            int x,
            int y,
            int w,
            int h,
            CultureInfo culture)
        {
            g.DrawImage(plantilla, x, y, w, h);

            // La marca "Don Sabio" va rotada -90° (de abajo hacia arriba).
            // Nombre y precio van en la misma orientación para leerse horizontal
            // al girar el cartel (marca abajo).
            float leftPad = w * 0.06f;
            float rightPad = w * 0.30f; // franja de la marca
            float topPad = h * 0.08f;
            float bottomPad = h * 0.08f;

            float areaLeft = x + leftPad;
            float areaTop = y + topPad;
            float areaWidth = w - leftPad - rightPad;
            float areaHeight = h - topPad - bottomPad;

            float cx = areaLeft + areaWidth / 2f;
            float cy = areaTop + areaHeight / 2f;

            // Tras rotar -90°: eje local X = alto del cartel, eje local Y = hacia la marca
            float textRunLength = areaHeight;
            float textStackWidth = areaWidth;

            float nameFontSize = Math.Max(8f, Math.Min(textRunLength * 0.12f, textStackWidth * 0.35f));
            float priceFontSize = Math.Max(12f, Math.Min(textRunLength * 0.18f, textStackWidth * 0.50f));

            using var nameFont = new Font("Segoe UI", nameFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            using var priceFont = new Font("Segoe UI", priceFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Color.FromArgb(40, 40, 40));
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.LineLimit
            };

            string nombre = item.Nombre ?? string.Empty;
            string precio = item.Precio.ToString("C2", culture);

            var state = g.Save();
            try
            {
                g.TranslateTransform(cx, cy);
                g.RotateTransform(-90f);

                // En coords locales: X a lo largo del cartel, Y hacia la marca (derecha en portrait).
                // Nombre más lejos de la marca; precio más cerca — al girar el cartel
                // (marca abajo) se leen nombre arriba y precio abajo.
                float nameBand = textStackWidth * 0.42f;
                float priceBand = textStackWidth * 0.50f;
                var nameRect = new RectangleF(-textRunLength / 2f, -textStackWidth * 0.42f, textRunLength, nameBand);
                var priceRect = new RectangleF(-textRunLength / 2f, textStackWidth * 0.02f, textRunLength, priceBand);

                g.DrawString(nombre, nameFont, brush, nameRect, sf);
                g.DrawString(precio, priceFont, brush, priceRect, sf);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static int MmToPx(float mm) => (int)Math.Round(mm / 25.4f * Dpi);
    }
}
