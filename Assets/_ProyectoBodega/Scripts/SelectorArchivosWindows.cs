using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Utilidad de selección nativa de archivos JSON y resolución/normalización de rutas para Windows.
/// </summary>
public static class SelectorArchivosWindows
{
    /// <summary>
    /// Resuelve la carpeta inicial prioritaria para lotes ERP:
    /// 1) &lt;CarpetaRaizApp&gt;/Bandeja_ERP
    /// 2) &lt;StreamingAssets&gt;/Bandeja_ERP
    /// 3) Application.streamingAssetsPath
    /// 4) Carpeta Personal del usuario
    /// </summary>
    public static string ResolverDirectorioInicialBandejaERP(string streamingAssetsPath = null, string dataPath = null)
    {
        string sa = streamingAssetsPath;
        if (string.IsNullOrEmpty(sa))
        {
            sa = Application.streamingAssetsPath;
        }

        string dp = dataPath;
        if (string.IsNullOrEmpty(dp))
        {
            dp = Application.dataPath;
        }

        if (!string.IsNullOrEmpty(dp))
        {
            try
            {
                DirectoryInfo padreApp = Directory.GetParent(dp);
                if (padreApp != null)
                {
                    string bandejaExe = Path.Combine(padreApp.FullName, "Bandeja_ERP");
                    if (Directory.Exists(bandejaExe))
                    {
                        return bandejaExe;
                    }
                }
            }
            catch (Exception)
            {
                // Ignorar rutas inválidas y continuar con el siguiente candidato
            }
        }

        if (!string.IsNullOrEmpty(sa))
        {
            try
            {
                string bandejaStreaming = Path.Combine(sa, "Bandeja_ERP");
                if (Directory.Exists(bandejaStreaming))
                {
                    return bandejaStreaming;
                }
            }
            catch (Exception)
            {
                // Ignorar rutas inválidas y continuar con el siguiente candidato
            }

            try
            {
                if (Directory.Exists(sa))
                {
                    return sa;
                }
            }
            catch (Exception)
            {
                // Ignorar rutas inválidas y continuar con fallback del SO
            }
        }

        string personal = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        if (!string.IsNullOrEmpty(personal) && Directory.Exists(personal))
        {
            return personal;
        }

        return !string.IsNullOrEmpty(sa) ? sa : Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Normaliza una ruta con separadores invertidos de Windows (\) para comdlg32.dll.
    /// </summary>
    public static string NormalizarRutaWindows(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return string.Empty;
        }

        string recortada = ruta.Trim();
        try
        {
            bool esRutaUnidadWindows = recortada.Length >= 2 && char.IsLetter(recortada[0]) && recortada[1] == ':';
            bool esRutaUNC = recortada.StartsWith("\\\\") || recortada.StartsWith("//");
            bool resolverCompleto = Path.DirectorySeparatorChar == '\\' || (!esRutaUnidadWindows && !esRutaUNC);
            string baseRuta = resolverCompleto ? Path.GetFullPath(recortada) : recortada;
            string normalizada = baseRuta.Replace('/', '\\');

            if (normalizada.Length > 3 && normalizada.EndsWith("\\"))
            {
                normalizada = normalizada.TrimEnd('\\');
            }

            return normalizada;
        }
        catch (Exception)
        {
            string fallback = recortada.Replace('/', '\\');
            if (fallback.Length > 3 && fallback.EndsWith("\\"))
            {
                fallback = fallback.TrimEnd('\\');
            }
            return fallback;
        }
    }

#if UNITY_STANDALONE_WIN
    private const int OFN_READONLY = 0x00000001;
    private const int OFN_HIDEREADONLY = 0x00000004;
    private const int OFN_NOCHANGEDIR = 0x00000008;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_EXPLORER = 0x00080000;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
#endif

    /// <summary>
    /// Abre el diálogo nativo Unicode de Windows (comdlg32.dll) evitando subprocesos PowerShell.
    /// </summary>
    public static string AbrirDialogoJson(string titulo, string dirInicial)
    {
#if UNITY_STANDALONE_WIN
        const int maxChars = 4096;
        IntPtr buffer = Marshal.AllocHGlobal(maxChars * sizeof(char));
        try
        {
            Marshal.WriteInt16(buffer, 0, 0);

            string dirNormalizado = NormalizarRutaWindows(dirInicial);
            if (!string.IsNullOrEmpty(dirNormalizado) && !Directory.Exists(dirNormalizado))
            {
                dirNormalizado = NormalizarRutaWindows(ResolverDirectorioInicialBandejaERP());
            }

            var ofn = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                hwndOwner = GetActiveWindow(),
                hInstance = IntPtr.Zero,
                lpstrFilter = "Archivos JSON (*.json)\0*.json\0Todos los archivos (*.*)\0*.*\0\0",
                lpstrCustomFilter = null,
                nMaxCustFilter = 0,
                nFilterIndex = 1,
                lpstrFile = buffer,
                nMaxFile = maxChars,
                lpstrFileTitle = null,
                nMaxFileTitle = 0,
                lpstrInitialDir = string.IsNullOrEmpty(dirNormalizado) ? null : dirNormalizado,
                lpstrTitle = string.IsNullOrEmpty(titulo) ? "Cargar Lote ERP" : titulo,
                Flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR | OFN_HIDEREADONLY,
                nFileOffset = 0,
                nFileExtension = 0,
                lpstrDefExt = "json",
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = null,
                pvReserved = IntPtr.Zero,
                dwReserved = 0,
                flagsEx = 0
            };

            if (GetOpenFileName(ref ofn))
            {
                string ruta = Marshal.PtrToStringUni(buffer);
                return string.IsNullOrWhiteSpace(ruta) ? string.Empty : ruta.Trim();
            }

            return string.Empty;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
#else
        return string.Empty;
#endif
    }
}
