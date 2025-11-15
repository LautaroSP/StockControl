using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace StockControl
{
    public static class LicenciaHelper
    {
        private readonly static string HmacKey = ConfigurationManager.AppSettings["LicenciaKey"] ?? "DEFAULT";


        private static readonly string LicFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WinFormsAppLicencia",
        "license.dat");


        public static bool LicenciaInstaladaYValida()
        {
            try
            {
                if (!File.Exists(LicFilePath)) return false;
                var content = File.ReadAllText(LicFilePath).Trim();
                var parts = content.Split('|');
                if (parts.Length != 2) return false;
                string serial = parts[0];
                string firma = parts[1];


                string machineId = ObtenerMachineIdInterno();


                // Verificar que el serial sea válido para esta máquina
                string esperado = GenerarSerial(machineId);
                if (!string.Equals(NormalizarSerial(serial), NormalizarSerial(esperado), StringComparison.OrdinalIgnoreCase))
                    return false;


                // Verificar integridad (evitar editar el archivo)
                string firmaEsperada = Firmar(machineId + "|" + NormalizarSerial(serial));
                return string.Equals(firma, firmaEsperada, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        public static bool ValidarYGuardarLicencia(string serialIngresado)
        {
            string machineId = ObtenerMachineIdInterno();
            string esperado = GenerarSerial(machineId);


            if (!string.Equals(NormalizarSerial(serialIngresado), NormalizarSerial(esperado), StringComparison.OrdinalIgnoreCase))
                return false;


            // Si es válido, guardamos serial + firma (HMAC)
            Directory.CreateDirectory(Path.GetDirectoryName(LicFilePath)!);
            string firma = Firmar(machineId + "|" + NormalizarSerial(serialIngresado));
            File.WriteAllText(LicFilePath, NormalizarSerial(serialIngresado) + "|" + firma);
            return true;
        }


        public static string ObtenerMachineIdMostrar()
        {
            // Formato corto y legible para pegar en el mail/whatsapp
            string machineId = ObtenerMachineIdInterno();
            return FormatearEnGrupos(machineId, 4, '-');
        }
        private static string ObtenerMachineIdInterno()
        {
            try
            {
                var bios = SafeWmiFirstOrEmpty("Win32_BIOS", "SerialNumber");
                var baseBoard = SafeWmiFirstOrEmpty("Win32_BaseBoard", "SerialNumber");
                var cpu = SafeWmiFirstOrEmpty("Win32_Processor", "ProcessorId");
                var disk = SafeWmiFirstOrEmpty("Win32_DiskDrive", "SerialNumber");
                var mac = FirstActiveMacAddress();


                string concatenado = string.Join("|", new[] { bios, baseBoard, cpu, disk, mac });
                string hash = Sha256Hex(concatenado + "::pepper_estatico"); // pequeño pepper extra
                                                                            // Usamos los primeros 24 hex (12 bytes) para hacerlo manejable
                return hash.Substring(0, 24).ToUpperInvariant();
            }
            catch
            {
                // Fallback mínimo: nombre de máquina + usuario
                string fallback = Environment.MachineName + "|" + Environment.UserName;
                return Sha256Hex(fallback).Substring(0, 24).ToUpperInvariant();
            }
        }
        private static string FirstActiveMacAddress()
        {
            try
            {
                var mac = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                (n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                .Select(n => n.GetPhysicalAddress().ToString())
                .FirstOrDefault();
                return mac ?? string.Empty;
            }
            catch { return string.Empty; }
        }


        private static string SafeWmiFirstOrEmpty(string wmiClass, string prop)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {prop} FROM {wmiClass}");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var val = obj[prop]?.ToString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }
            }
            catch { }
            return string.Empty;
        }
        public static string GenerarSerial(string machineId)
        {
            // HMAC-SHA256(machineId, HmacKey) y nos quedamos con 20 hex (10 bytes) para serial corto
            var h = HmacSha256Hex(machineId.ToUpperInvariant(), HmacKey);
            string corto = h.Substring(0, 20).ToUpperInvariant();
            return FormatearEnGrupos(corto, 5, '-'); // XXXX-XXXXX-XXXXX-XXXXX (20 hex en 4 grupos de 5)
        }


        private static string Firmar(string data)
        {
            return HmacSha256Hex(data, HmacKey).Substring(0, 32).ToUpperInvariant();
        }


        private static string HmacSha256Hex(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }


        private static string Sha256Hex(string data)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private static string FormatearEnGrupos(string inputHex, int grupo, char sep)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < inputHex.Length; i++)
            {
                if (i > 0 && i % grupo == 0) sb.Append(sep);
                sb.Append(inputHex[i]);
            }
            return sb.ToString();
        }


        private static string NormalizarSerial(string s)
        {
            return new string((s ?? string.Empty).Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpperInvariant();
        }
    }
}
