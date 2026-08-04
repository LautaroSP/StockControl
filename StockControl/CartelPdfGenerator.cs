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

            // Área de contenido (se puede pisar un poco la marca si hace falta)
            float leftPad = w * 0.07f;
            float rightPad = w * 0.18f;
            float topPad = h * 0.07f;
            float bottomPad = h * 0.07f;

            int areaX = (int)(x + leftPad);
            int areaY = (int)(y + topPad);
            int areaW = Math.Max(8, (int)(w - leftPad - rightPad));
            int areaH = Math.Max(8, (int)(h - topPad - bottomPad));

            // Bitmap en orientación de lectura (horizontal): nombre arriba, precio abajo.
            // Luego se rota -90° para alinearlo con la marca del cartel.
            int textW = areaH; // largo del texto = alto del área en el cartel
            int textH = areaW; // apilado nombre/precio = ancho del área

            using var textBmp = new Bitmap(textW, textH, PixelFormat.Format32bppArgb);
            using (var tg = Graphics.FromImage(textBmp))
            {
                tg.Clear(Color.Transparent);
                tg.SmoothingMode = SmoothingMode.HighQuality;
                tg.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                tg.InterpolationMode = InterpolationMode.HighQualityBicubic;

                string nombre = item.Nombre ?? string.Empty;
                string precio = item.Precio.ToString("C2", culture);

                // Más alto para el nombre (2 renglones); precio debajo
                float nameBand = textH * 0.48f;
                float priceBand = textH * 0.42f;
                var nameRect = new RectangleF(2, textH * 0.02f, textW - 4, nameBand);
                var priceRect = new RectangleF(2, textH * 0.54f, textW - 4, priceBand);

                using var sfName = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.LineLimit,
                    Trimming = StringTrimming.None
                };
                using var sfPrecio = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.None
                };

                float nameFontSize = FitFontSizeWrapped(
                    tg, nombre, "Segoe UI", FontStyle.Bold,
                    nameRect.Width, nameRect.Height,
                    maxSize: Math.Min(textH * 0.22f, textW * 0.11f),
                    minSize: 7f,
                    sfName);
                float priceFontSize = FitFontSize(
                    tg, precio, "Segoe UI", FontStyle.Bold,
                    priceRect.Width * 0.96f, priceRect.Height * 0.90f,
                    maxSize: Math.Min(textH * 0.22f, textW * 0.09f),
                    minSize: 6f);

                using var nameFont = new Font("Segoe UI", nameFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                using var priceFont = new Font("Segoe UI", priceFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                using var brush = new SolidBrush(Color.FromArgb(30, 30, 30));

                tg.DrawString(nombre, nameFont, brush, nameRect, sfName);
                tg.DrawString(precio, priceFont, brush, priceRect, sfPrecio);
            }

            textBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            // Tras Rotate270: queda areaW x areaH y el texto lee de abajo hacia arriba
            g.DrawImage(textBmp, areaX, areaY, areaW, areaH);
        }

        private static float FitFontSize(
            Graphics g,
            string text,
            string fontFamily,
            FontStyle style,
            float maxWidth,
            float maxHeight,
            float maxSize,
            float minSize)
        {
            float size = Math.Max(minSize, maxSize);
            while (size > minSize)
            {
                using var font = new Font(fontFamily, size, style, GraphicsUnit.Pixel);
                var measured = g.MeasureString(text, font);
                if (measured.Width <= maxWidth && measured.Height <= maxHeight)
                    return size;
                size -= 0.5f;
            }
            return minSize;
        }

        private static float FitFontSizeWrapped(
            Graphics g,
            string text,
            string fontFamily,
            FontStyle style,
            float maxWidth,
            float maxHeight,
            float maxSize,
            float minSize,
            StringFormat sf)
        {
            float size = Math.Max(minSize, maxSize);
            var layout = new SizeF(maxWidth, maxHeight);
            while (size > minSize)
            {
                using var font = new Font(fontFamily, size, style, GraphicsUnit.Pixel);
                var measured = g.MeasureString(text, font, layout, sf, out int charsFitted, out int linesFilled);
                bool fitsWidth = measured.Width <= maxWidth + 0.5f;
                bool fitsHeight = measured.Height <= maxHeight + 0.5f;
                bool allChars = charsFitted >= text.Length;
                bool maxTwoLines = linesFilled <= 2;
                if (fitsWidth && fitsHeight && allChars && maxTwoLines)
                    return size;
                size -= 0.5f;
            }
            return minSize;
        }

        private static int MmToPx(float mm) => (int)Math.Round(mm / 25.4f * Dpi);
    }
}
