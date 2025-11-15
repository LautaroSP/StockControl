using System.Text;
using ZXing;
using ZXing.Rendering;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StockControl
{
    using StockControl.Domain;
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using ZXing.Windows.Compatibility;

    public class TicketPrinter
    {
        private List<ItemSeleccionado> _carrito;
        private decimal _total;

        public TicketPrinter(List<ItemSeleccionado> carrito)
        {
            _carrito = carrito;
            _total = 0;
        }
        public TicketPrinter()
        { 
        }
        public static List<string> WrapText(string text, int maxChars)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < text.Length; i += maxChars)
            {
                lines.Add(text.Substring(i, Math.Min(maxChars, text.Length - i)));
            }
            return lines;
        }
        public void PrintTicketFinal(string printerName)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("\x1B\x21\x00"); 

            sb.AppendLine("\x1B\x21\x08" + "=== "+StockMain.nombreLocal+" ==="); 
            sb.AppendLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            int anchoTotal = 32;

            int anchoNombre = (int)Math.Round(anchoTotal * 0.30); 
            int espacio = (int)Math.Round(anchoTotal * 0.1);     
            int anchoNumeros = anchoTotal - anchoNombre - espacio;

            int anchoCant = Math.Min(6, anchoNumeros);
            int anchoSubtotal = anchoNumeros - anchoCant;

            string header = "Prod".PadRight(anchoNombre)
                          + new string(' ', espacio)
                          + "Cant".PadRight(anchoCant)
                          + "Subt".PadRight(anchoSubtotal);
            sb.AppendLine(header);
            sb.AppendLine(new string('-', anchoTotal));

            decimal total = 0;

            foreach (var item in _carrito)
            {
                var lineasNombre = WrapText(item.Nombre, anchoNombre);

                for (int i = 0; i < lineasNombre.Count; i++)
                {
                    string nombre = lineasNombre[i].PadRight(anchoNombre);

                    string cant = i == 0 ? item.Cantidad.ToString("0.##").PadRight(anchoCant) : "".PadRight(anchoCant);
                    string sub = i == 0 ? item.Subtotal.ToString("0.##").PadRight(anchoSubtotal) : "".PadRight(anchoSubtotal);

                    sb.AppendLine(nombre + new string(' ', espacio) + cant + sub);
                }

                total += item.Subtotal;
            }

            sb.AppendLine(new string('-', anchoTotal));


            sb.AppendLine("\x1B\x21\x08" + $"TOTAL: {total:0.##}");


            foreach (var linea in WrapText("Ticket no valido como factura", anchoTotal))
                sb.AppendLine(linea);

            sb.AppendLine("Gracias por su compra");
            sb.AppendLine(" ");
            sb.AppendLine(" "); 
            sb.AppendLine("\x1D\x56\x00"); 

            RawPrinterHelper.SendBytesToPrinter(printerName, Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public void PrintBarcode(string printerName, string texto, int cantidad)
        {

            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 350,
                    Height = 100,  
                    Margin = 2     
                },
                Renderer = new BitmapRenderer()
            };

            using Bitmap bitmap = writer.Write(texto);

            // 2️⃣ Convertir Bitmap a bytes ESC/POS
            byte[] imageBytes = ConvertBitmapToEscPos(bitmap);

            // 3️⃣ Enviar a la impresora
            for (int i = 0; i < cantidad; i++)
            {
                RawPrinterHelper.SendBytesToPrinter(printerName, imageBytes);

                // Agregar salto de línea o feed extra si querés separación
                RawPrinterHelper.SendBytesToPrinter(printerName, new byte[] { 0x0A, 0x0A });
            }
        }

        private static byte[] ConvertBitmapToEscPos(Bitmap bitmap)
        {
            using MemoryStream ms = new MemoryStream();

            // Inicializa impresora
            ms.Write(new byte[] { 0x1B, 0x40 }, 0, 2);

            int width = bitmap.Width;
            int height = bitmap.Height;

            int bytesPerRow = (width + 7) / 8; // redondeo a múltiplo de 8
            byte xL = (byte)(bytesPerRow & 0xFF);
            byte xH = (byte)((bytesPerRow >> 8) & 0xFF);
            byte yL = (byte)(height & 0xFF);
            byte yH = (byte)((height >> 8) & 0xFF);

            // GS v 0 m (modo 0 = normal)
            ms.Write(new byte[] { 0x1D, 0x76, 0x30, 0x00, xL, xH, yL, yH }, 0, 8);

            // Datos de la imagen
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 8)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (x + bit < width)
                        {
                            Color c = bitmap.GetPixel(x + bit, y);
                            bool negro = (c.R + c.G + c.B) / 3 < 128;
                            b |= (byte)((negro ? 1 : 0) << (7 - bit));
                        }
                    }
                    ms.WriteByte(b);
                }
            }

            // Salto de línea
            ms.WriteByte(0x0A);

            return ms.ToArray();
        }




    }
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr hPrinter;
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                return false;

            DOCINFOA di = new DOCINFOA { pDocName = "Ticket", pDataType = "RAW" };
            if (!StartDocPrinter(hPrinter, 1, di))
            {
                ClosePrinter(hPrinter);
                return false;
            }

            StartPagePrinter(hPrinter);
            WritePrinter(hPrinter, bytes, bytes.Length, out int written);
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);

            return true;
        }


    }

}
