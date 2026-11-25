using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de blindaje y diagnóstico automático SCADA para Bodega_TurnoNocturno.
/// Se ejecuta antes que Awake/Start (-500) y en menú de Editor para detectar archivos JSON faltantes,
/// objetos vitales borrados o duplicados, referencias nulas en Inspector e integridad física/NavMesh.
/// </summary>
[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class GuardiaIntegridadSCADA : MonoBehaviour
{
    public const string NombreEscenaPrincipal = "Bodega_TurnoNocturno";
    public const string NombreObjetoGuardia = "Guardia_Integridad_SCADA";

    [Header("Archivos JSON Requeridos en StreamingAssets")]
    public string[] archivosJsonRequeridos =
    {
        "manifiesto.json",
        "lote_30_pallets.json",
        "lote_260_pallets.json",
        "lote_450_pallets.json"
    };

    [Header("Ejecución Automática")]
    public bool ejecutarAlIniciarPlayMode = true;

    public class ResultadoAuditoriaSCADA
    {
        public int ArchivosJsonFaltantes;
        public int ObjetosFaltantesODuplicados;
        public int ReferenciasRotasONulas;
        public int FallosInfraestructura;
        public readonly List<string> Errores = new List<string>();
        public readonly List<string> Advertencias = new List<string>();

        public int TotalFallos => Errores.Count;
        public bool EstaSano => Errores.Count == 0;
    }

    private static bool auditoriaEjecutadaEnCarga = false;
    private static bool reporteFinalPlayModeEmitido = false;
    private static ResultadoAuditoriaSCADA resultadoPendientePlayMode = null;
    private static string resumenPendienteDashboard = null;

    // Lista blanca de campos serializados que son intencionalmente nulos o vacíos en estado sano inicial.
    private static readonly Dictionary<string, HashSet<string>> CamposOpcionalesOInicializadosEnRuntime =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            {
                nameof(RobotManagerEVE),
                new HashSet<string>(StringComparer.Ordinal)
                {
                    "amrEnAlertaBateria20"
                }
            },
            {
                nameof(DashboardUI),
                new HashSet<string>(StringComparer.Ordinal)
                {
                    "panelMenuModulos",
                    "textosEstadoManifiesto"
                }
            },
            {
                nameof(GestorAudioSCADA),
                new HashSet<string>(StringComparer.Ordinal)
                {
                    "fuenteAlarmaFabrica",
                    "clipAlarmaSonoraFabrica",
                    "clipAlertaPreventiva",
                    "clipAlarmaCritica",
                    "clipConfirmacionEntrega",
                    "clipAcopleNeumaticoVerde",
                    "clipClickUI",
                    "clipBloqueoIngestaJSON",
                    "clipIngresoPermitidoJSON",
                    "clipMotorAMR",
                    "clipRotoresEVE",
                    "clipRotoresDronSoporte"
                }
            }
        };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstadoEstatico()
    {
        auditoriaEjecutadaEnCarga = false;
        reporteFinalPlayModeEmitido = false;
        resultadoPendientePlayMode = null;
        resumenPendienteDashboard = null;
    }

    void Awake()
    {
        if (!ejecutarAlIniciarPlayMode || auditoriaEjecutadaEnCarga)
        {
            return;
        }

        auditoriaEjecutadaEnCarga = true;
        // En Awake (-500) aún no corrió NavMeshSurface.OnEnable() (orden 0); auditamos datos/escena/Inspector primero
        // y completamos NavMesh + resumen final en AfterSceneLoad/Start tras OnEnable().
        ResultadoAuditoriaSCADA resultado = EjecutarAuditoriaCompleta(
            emitirLogs: false,
            archivosJsonOverride: archivosJsonRequeridos,
            permitirSinNavMeshEnEscenaVacia: false,
            verificarNavMesh: false);

        for (int i = 0; i < resultado.Advertencias.Count; i++)
        {
            Debug.LogWarning(resultado.Advertencias[i]);
        }
        for (int i = 0; i < resultado.Errores.Count; i++)
        {
            Debug.LogError(resultado.Errores[i]);
        }

        resultadoPendientePlayMode = resultado;
    }

    void Start()
    {
        FinalizarReportePlayModeSiPendiente();
        IntentarEnviarResumenADashboard();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RespaldoPostCargaEscena()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();
        if (!auditoriaEjecutadaEnCarga && escenaActiva.IsValid() && escenaActiva.name == NombreEscenaPrincipal)
        {
            string avisoGuardia =
                $"[BLINDAJE ESCENA] No se ejecutó Awake de '{NombreObjetoGuardia}' ({nameof(GuardiaIntegridadSCADA)}). " +
                $"Pista: Alguien borró o desactivó '{NombreObjetoGuardia}' en la escena '{NombreEscenaPrincipal}'.";
            Debug.LogError(avisoGuardia);

            auditoriaEjecutadaEnCarga = true;
            reporteFinalPlayModeEmitido = true;
            ResultadoAuditoriaSCADA resultado = EjecutarAuditoriaCompleta(true);
            resultado.ObjetosFaltantesODuplicados++;
            resultado.Errores.Insert(0, avisoGuardia);
            PrepararResumenParaDashboard(resultado);
        }
        else
        {
            FinalizarReportePlayModeSiPendiente();
        }

        IntentarEnviarResumenADashboard();
    }

    private static void FinalizarReportePlayModeSiPendiente()
    {
        if (reporteFinalPlayModeEmitido || resultadoPendientePlayMode == null)
        {
            return;
        }

        reporteFinalPlayModeEmitido = true;
        ResultadoAuditoriaSCADA resultado = resultadoPendientePlayMode;
        resultadoPendientePlayMode = null;

        int erroresPrevios = resultado.Errores.Count;
        ValidarNavMeshBodega(resultado, permitirSinNavMeshEnEscenaVacia: false);
        for (int i = erroresPrevios; i < resultado.Errores.Count; i++)
        {
            Debug.LogError(resultado.Errores[i]);
        }

        if (resultado.EstaSano)
        {
            Debug.Log("<color=#10B981>[BLINDAJE SCADA OK] Integridad verificada: 0 objetos borrados, 0 referencias rotas, JSONs intactos.</color>");
        }
        else
        {
            EmitirResumenFalloConsola(resultado);
            PrepararResumenParaDashboard(resultado);
        }
    }

    private static void PrepararResumenParaDashboard(ResultadoAuditoriaSCADA resultado)
    {
        if (resultado == null || resultado.EstaSano)
        {
            resumenPendienteDashboard = null;
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"<color=#EF4444><b>[BLINDAJE SCADA] Detectados {resultado.TotalFallos} fallo(s) de integridad:</b></color>");
        int limite = Mathf.Min(resultado.Errores.Count, 5);
        for (int i = 0; i < limite; i++)
        {
            sb.Append($"\n  {i + 1}. {resultado.Errores[i]}");
        }
        if (resultado.Errores.Count > limite)
        {
            sb.Append($"\n  ... (+{resultado.Errores.Count - limite} más en Consola de Unity)");
        }

        resumenPendienteDashboard = sb.ToString();
        IntentarEnviarResumenADashboard();
    }

    private static void IntentarEnviarResumenADashboard()
    {
        if (string.IsNullOrEmpty(resumenPendienteDashboard))
        {
            return;
        }

        DashboardUI dashboard = DashboardUI.Instance;
        if (dashboard != null)
        {
            dashboard.RegistrarLog(resumenPendienteDashboard);
            resumenPendienteDashboard = null;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Verificar Blindaje de Escena")]
    public static void MenuVerificarBlindajeDeEscena()
    {
        EjecutarAuditoriaCompleta(true);
    }
#endif

    public static ResultadoAuditoriaSCADA EjecutarAuditoriaCompleta(
        bool emitirLogs = true,
        string[] archivosJsonOverride = null,
        bool permitirSinNavMeshEnEscenaVacia = false,
        bool verificarNavMesh = true)
    {
        ResultadoAuditoriaSCADA resultado = new ResultadoAuditoriaSCADA();

        string[] listaJsons = ResolverListaJsons(archivosJsonOverride);
        List<string> coordenadasManifiesto = AuditarArchivosJson(listaJsons, resultado);
        AuditarObjetosVitalesYDuplicados(resultado);
        AuditarComponentesYCamposInspector(resultado);
        AuditarInfraestructuraYNavMesh(coordenadasManifiesto, resultado, permitirSinNavMeshEnEscenaVacia, verificarNavMesh);

        if (emitirLogs)
        {
            EmitirReporteConsola(resultado);
        }

        return resultado;
    }

    private static string[] ResolverListaJsons(string[] archivosJsonOverride)
    {
        if (archivosJsonOverride != null && archivosJsonOverride.Length > 0)
        {
            return archivosJsonOverride;
        }

        GuardiaIntegridadSCADA guardia = FindFirstObjectByType<GuardiaIntegridadSCADA>(FindObjectsInactive.Include);
        if (guardia != null && guardia.archivosJsonRequeridos != null && guardia.archivosJsonRequeridos.Length > 0)
        {
            return guardia.archivosJsonRequeridos;
        }

        return new[]
        {
            "manifiesto.json",
            "lote_30_pallets.json",
            "lote_260_pallets.json",
            "lote_450_pallets.json"
        };
    }

    private static List<string> AuditarArchivosJson(string[] archivosJson, ResultadoAuditoriaSCADA resultado)
    {
        List<string> coordenadasBase = new List<string>();
        string baseStreaming = Application.streamingAssetsPath;

        if (string.IsNullOrEmpty(baseStreaming) || !Directory.Exists(baseStreaming))
        {
            resultado.ArchivosJsonFaltantes++;
            resultado.Errores.Add(
                "[BLINDAJE DATOS] No existe la carpeta 'Assets/StreamingAssets'. " +
                "Pista: Restáurala desde Git para recuperar manifiesto.json y los lotes ERP.");
            return coordenadasBase;
        }

        for (int i = 0; i < archivosJson.Length; i++)
        {
            string nombreArchivo = archivosJson[i];
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                continue;
            }

            string rutaArchivo = Path.Combine(baseStreaming, nombreArchivo);
            if (!File.Exists(rutaArchivo))
            {
                resultado.ArchivosJsonFaltantes++;
                resultado.Errores.Add(
                    $"[BLINDAJE DATOS] Falta el archivo '{nombreArchivo}' en StreamingAssets. " +
                    "Pista: Restáuralo desde Git o verifica la carpeta StreamingAssets.");
                continue;
            }

            try
            {
                string contenido = File.ReadAllText(rutaArchivo, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    resultado.ArchivosJsonFaltantes++;
                    resultado.Errores.Add(
                        $"[BLINDAJE DATOS] El archivo '{nombreArchivo}' en StreamingAssets está vacío. " +
                        "Pista: Restáuralo desde Git o verifica su contenido JSON.");
                    continue;
                }

                Manifiesto lote = JsonUtility.FromJson<Manifiesto>(contenido.TrimStart('\uFEFF').Trim());
                if (lote == null || lote.pedidos == null || lote.pedidos.Count == 0)
                {
                    resultado.ArchivosJsonFaltantes++;
                    resultado.Errores.Add(
                        $"[BLINDAJE DATOS] El archivo '{nombreArchivo}' no tiene una lista válida de 'pedidos'. " +
                        "Pista: Revisa la estructura JSON del lote o restáuralo desde Git.");
                    continue;
                }

                if (string.Equals(nombreArchivo, "manifiesto.json", StringComparison.OrdinalIgnoreCase))
                {
                    for (int p = 0; p < lote.pedidos.Count; p++)
                    {
                        Pedido ped = lote.pedidos[p];
                        if (ped != null && !string.IsNullOrEmpty(ped.coordenada_bodega) && !coordenadasBase.Contains(ped.coordenada_bodega))
                        {
                            coordenadasBase.Add(ped.coordenada_bodega);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.ArchivosJsonFaltantes++;
                resultado.Errores.Add(
                    $"[BLINDAJE DATOS] Error al leer '{nombreArchivo}': {ex.Message}. " +
                    "Pista: Verifica que el JSON esté bien formado o restáuralo desde Git.");
            }
        }

        return coordenadasBase;
    }

    private static void AuditarObjetosVitalesYDuplicados(ResultadoAuditoriaSCADA resultado)
    {
        VerificarSingletonEnEscena<RobotManagerEVE>("Robot_Manager_EVE", "Robot_Manager_EVE", true, resultado);
        VerificarSingletonEnEscena<ModuloDespachoTMS>("Gestor_Muelles_TMS", "Gestor_Muelles_TMS", true, resultado);
        VerificarSingletonEnEscena<RelojSimuladorTurno>("Robot_Manager_EVE", "Robot_Manager_EVE", true, resultado);
        VerificarSingletonEnEscena<DashboardUI>("UI_Dashboard_Corporativo", "UI_Dashboard_Corporativo", true, resultado);
        VerificarSingletonEnEscena<DirectorCinematico>("Director_Cinemachine", "Director_Cinemachine", true, resultado);
        VerificarSingletonEnEscena<DronMantenimiento>("Dron_Mantenimiento", "Dron_Mantenimiento", true, resultado);
        VerificarSingletonEnEscena<GeneradorTablaManifiesto>("Manifiesto", "UI_Dashboard_Corporativo/HUD_Operativo/Manifiesto", false, resultado);
        VerificarSingletonEnEscena<GestorAudioSCADA>("UI_Dashboard_Corporativo", "UI_Dashboard_Corporativo", true, resultado);

        AuditarFlotaAMREnEscena(resultado);
    }

    private static List<T> BuscarComponentesEnEscena<T>() where T : Component
    {
        T[] encontrados = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<T> enEscena = new List<T>(encontrados.Length);
        for (int i = 0; i < encontrados.Length; i++)
        {
            T comp = encontrados[i];
            if (comp != null && comp.gameObject != null && comp.gameObject.scene.IsValid())
            {
                enEscena.Add(comp);
            }
        }
        return enEscena;
    }

    private static void VerificarSingletonEnEscena<T>(
        string nombreEsperado,
        string rutaEsperada,
        bool requiereActivoEnJerarquia,
        ResultadoAuditoriaSCADA resultado) where T : Component
    {
        List<T> encontrados = BuscarComponentesEnEscena<T>();
        string nombreTipo = typeof(T).Name;

        if (encontrados.Count == 0)
        {
            resultado.ObjetosFaltantesODuplicados++;
            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] Falta '{nombreTipo}' en la escena. Objeto esperado: '{nombreEsperado}' (ruta: '{rutaEsperada}'). " +
                "Pista: Alguien borró este objeto o quitó su componente en el Inspector.");
            return;
        }

        if (encontrados.Count > 1)
        {
            resultado.ObjetosFaltantesODuplicados++;
            StringBuilder rutas = new StringBuilder();
            for (int i = 0; i < encontrados.Count; i++)
            {
                if (i > 0) rutas.Append(", ");
                GameObject go = encontrados[i].gameObject;
                string estado = go.activeInHierarchy ? "ACTIVO" : "INACTIVO";
                rutas.Append($"'{ObtenerRutaJerarquia(go.transform)}' [{estado}]");
            }

            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] Se encontraron {encontrados.Count} copias de '{nombreTipo}' (solo debe existir 1 en '{rutaEsperada}'): {rutas}. " +
                "Pista: Alguien duplicó el Singleton en la jerarquía; elimina las copias sobrantes (incluso si están inactivas).");
        }

        T principal = encontrados[0];
        if (requiereActivoEnJerarquia && !principal.gameObject.activeInHierarchy)
        {
            resultado.ObjetosFaltantesODuplicados++;
            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] El objeto vital '{ObtenerRutaJerarquia(principal.transform)}' ({nombreTipo}) está DESACTIVADO en la jerarquía. " +
                "Pista: Activa su casilla en el Inspector para evitar fallos en Awake/Update.");
        }
        else if (!requiereActivoEnJerarquia && !principal.gameObject.activeSelf)
        {
            resultado.ObjetosFaltantesODuplicados++;
            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] El objeto '{ObtenerRutaJerarquia(principal.transform)}' ({nombreTipo}) tiene activeSelf=false. " +
                "Pista: Activa ese GameObject en el Inspector.");
        }

        if (principal is Behaviour bh && !bh.enabled)
        {
            resultado.ObjetosFaltantesODuplicados++;
            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] El componente '{nombreTipo}' en '{ObtenerRutaJerarquia(principal.transform)}' está deshabilitado (enabled=false). " +
                "Pista: Marca el checkbox del componente en el Inspector.");
        }
    }

    private static void AuditarFlotaAMREnEscena(ResultadoAuditoriaSCADA resultado)
    {
        List<RobotAMR> amrs = BuscarComponentesEnEscena<RobotAMR>();
        if (amrs.Count != 3)
        {
            resultado.ObjetosFaltantesODuplicados++;
            StringBuilder detalle = new StringBuilder();
            for (int i = 0; i < amrs.Count; i++)
            {
                if (i > 0) detalle.Append(", ");
                detalle.Append($"'{ObtenerRutaJerarquia(amrs[i].transform)}' (id='{amrs[i].idRobot}')");
            }

            string encontradosTxt = amrs.Count == 0 ? "ninguno" : detalle.ToString();
            resultado.Errores.Add(
                $"[BLINDAJE ESCENA] Se esperaban exactamente 3 componentes 'RobotAMR' (Robot_AMR, Robot_AMR_02, Robot_AMR_03), pero hay {amrs.Count} ({encontradosTxt}). " +
                "Pista: Restaura el AMR faltante o elimina el duplicado en la jerarquía.");
        }

        string[] nombresEsperados = { "Robot_AMR", "Robot_AMR_02", "Robot_AMR_03" };
        for (int i = 0; i < nombresEsperados.Length; i++)
        {
            string nombre = nombresEsperados[i];
            RobotAMR coincidencia = null;
            for (int j = 0; j < amrs.Count; j++)
            {
                if (amrs[j].gameObject.name == nombre)
                {
                    coincidencia = amrs[j];
                    break;
                }
            }

            if (coincidencia == null)
            {
                if (amrs.Count == 3)
                {
                    resultado.ObjetosFaltantesODuplicados++;
                    resultado.Errores.Add(
                        $"[BLINDAJE ESCENA] Falta el GameObject '{nombre}' con componente RobotAMR. " +
                        "Pista: Verifica que los 3 robots se llamen Robot_AMR, Robot_AMR_02 y Robot_AMR_03.");
                }
                continue;
            }

            if (!coincidencia.gameObject.activeInHierarchy || !coincidencia.enabled)
            {
                resultado.ObjetosFaltantesODuplicados++;
                resultado.Errores.Add(
                    $"[BLINDAJE ESCENA] La unidad '{nombre}' está desactivada o tiene RobotAMR deshabilitado. " +
                    "Pista: Activa '{nombre}' y su componente RobotAMR en el Inspector.");
            }
        }

        HashSet<string> idsVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < amrs.Count; i++)
        {
            string id = amrs[i].idRobot ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                resultado.ReferenciasRotasONulas++;
                resultado.Errores.Add(
                    $"[BLINDAJE ESCENA] El robot '{amrs[i].gameObject.name}' tiene 'idRobot' vacío. " +
                    "Pista: Asigna AMR-01, AMR-02 o AMR-03 en el Inspector.");
            }
            else if (!idsVistos.Add(id))
            {
                resultado.ObjetosFaltantesODuplicados++;
                resultado.Errores.Add(
                    $"[BLINDAJE ESCENA] ID de robot duplicado '{id}' en '{amrs[i].gameObject.name}'. " +
                    "Pista: Cada RobotAMR debe tener un idRobot único (AMR-01, AMR-02, AMR-03).");
            }
        }
    }

    private static void AuditarComponentesYCamposInspector(ResultadoAuditoriaSCADA resultado)
    {
        Assembly assemblyRuntime = typeof(GuardiaIntegridadSCADA).Assembly;
        List<GameObject> todosLosObjetos = ObtenerTodosLosGameObjectsDeEscenasCargadas();

        for (int i = 0; i < todosLosObjetos.Count; i++)
        {
            GameObject go = todosLosObjetos[i];
            if (go == null) continue;

            Component[] componentes = go.GetComponents<Component>();
            for (int c = 0; c < componentes.Length; c++)
            {
                Component comp = componentes[c];
                if (comp == null)
                {
                    resultado.ReferenciasRotasONulas++;
                    resultado.Errores.Add(
                        $"[BLINDAJE INSPECTOR] En el objeto '{go.name}' (ruta: '{ObtenerRutaJerarquia(go.transform)}') hay un script perdido (Missing Script). " +
                        "Pista: Alguien borró o renombró un archivo .cs o su .meta sin actualizar la escena.");
                    continue;
                }

                if (!(comp is MonoBehaviour script))
                {
                    continue;
                }

                Type tipoScript = script.GetType();
                if (tipoScript.Assembly != assemblyRuntime || tipoScript == typeof(GuardiaIntegridadSCADA))
                {
                    continue;
                }

                if (!go.activeSelf || !script.enabled)
                {
                    continue;
                }

                InspeccionarObjetoReflexivamente(go, script, script, tipoScript, string.Empty, 0, resultado);
                ValidarReglasEspecialesDeScript(go, script, resultado);
            }
        }
    }

    private static void ValidarReglasEspecialesDeScript(GameObject go, MonoBehaviour script, ResultadoAuditoriaSCADA resultado)
    {
        if (script is DashboardUI)
        {
            FieldInfo fTmp = typeof(DashboardUI).GetField("textoLogTMP", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo fLegacy = typeof(DashboardUI).GetField("textoLogLegacy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            UnityEngine.Object valTmp = fTmp != null ? fTmp.GetValue(script) as UnityEngine.Object : null;
            UnityEngine.Object valLegacy = fLegacy != null ? fLegacy.GetValue(script) as UnityEngine.Object : null;

            if (valTmp == null && valLegacy == null)
            {
                resultado.ReferenciasRotasONulas++;
                resultado.Errores.Add(
                    $"[BLINDAJE INSPECTOR] En el objeto '{go.name}' -> Script 'DashboardUI' -> falta asignar 'textoLogTMP' o 'textoLogLegacy'. " +
                    "Pista: Alguien borró el objeto de texto de la consola EVE o lo desconectó en el Inspector.");
            }
        }
    }

    private static void InspeccionarObjetoReflexivamente(
        GameObject go,
        MonoBehaviour scriptRaiz,
        object instanciaActual,
        Type tipoActual,
        string prefijoCampo,
        int profundidad,
        ResultadoAuditoriaSCADA resultado)
    {
        if (instanciaActual == null || tipoActual == null || profundidad > 2)
        {
            return;
        }

        Assembly assemblyRuntime = typeof(GuardiaIntegridadSCADA).Assembly;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        for (Type cursor = tipoActual; cursor != null && cursor != typeof(MonoBehaviour) && cursor != typeof(object); cursor = cursor.BaseType)
        {
            FieldInfo[] campos = cursor.GetFields(flags);
            for (int i = 0; i < campos.Length; i++)
            {
                FieldInfo field = campos[i];
                if (!EsCampoSerializadoPorUnity(field))
                {
                    continue;
                }

                string nombreCompleto = string.IsNullOrEmpty(prefijoCampo) ? field.Name : $"{prefijoCampo}.{field.Name}";
                if (EsCampoOpcionalPermitido(scriptRaiz.GetType(), nombreCompleto))
                {
                    continue;
                }

                object valor = field.GetValue(instanciaActual);
                Type tipoCampo = field.FieldType;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tipoCampo))
                {
                    UnityEngine.Object objRef = valor as UnityEngine.Object;
                    if (objRef == null)
                    {
                        resultado.ReferenciasRotasONulas++;
                        resultado.Errores.Add(
                            $"[BLINDAJE INSPECTOR] En el objeto '{go.name}' -> Script '{scriptRaiz.GetType().Name}' -> falta asignar el campo '{nombreCompleto}' ({tipoCampo.Name}). " +
                            "Pista: Alguien borró el objeto vinculado o lo desconectó en el Inspector.");
                    }
                }
                else if (typeof(IList).IsAssignableFrom(tipoCampo))
                {
                    Type tipoElemento = ObtenerTipoElementoColeccion(tipoCampo);
                    if (tipoElemento != null && typeof(UnityEngine.Object).IsAssignableFrom(tipoElemento))
                    {
                        IList lista = valor as IList;
                        if (lista == null || lista.Count == 0)
                        {
                            resultado.ReferenciasRotasONulas++;
                            resultado.Errores.Add(
                                $"[BLINDAJE INSPECTOR] En el objeto '{go.name}' -> Script '{scriptRaiz.GetType().Name}' -> la colección '{nombreCompleto}' ({tipoElemento.Name}[]) está vacía. " +
                                "Pista: Alguien borró los elementos vinculados en el Inspector.");
                        }
                        else
                        {
                            for (int idx = 0; idx < lista.Count; idx++)
                            {
                                UnityEngine.Object elem = lista[idx] as UnityEngine.Object;
                                if (elem == null)
                                {
                                    resultado.ReferenciasRotasONulas++;
                                    resultado.Errores.Add(
                                        $"[BLINDAJE INSPECTOR] En el objeto '{go.name}' -> Script '{scriptRaiz.GetType().Name}' -> falta asignar el campo '{nombreCompleto}[{idx}]' ({tipoElemento.Name}). " +
                                        "Pista: Alguien borró el objeto vinculado o lo desconectó en el Inspector.");
                                }
                            }
                        }
                    }
                }
                else if (valor != null &&
                         tipoCampo.Assembly == assemblyRuntime &&
                         !tipoCampo.IsEnum &&
                         !tipoCampo.IsPrimitive &&
                         tipoCampo != typeof(string) &&
                         tipoCampo.IsDefined(typeof(SerializableAttribute), false))
                {
                    InspeccionarObjetoReflexivamente(go, scriptRaiz, valor, tipoCampo, nombreCompleto, profundidad + 1, resultado);
                }
            }
        }
    }

    private static bool EsCampoSerializadoPorUnity(FieldInfo field)
    {
        if (field == null || field.IsStatic || field.IsInitOnly || field.IsLiteral)
        {
            return false;
        }
        if (field.Name.Contains("k__BackingField"))
        {
            return false;
        }
        if (field.IsDefined(typeof(NonSerializedAttribute), true))
        {
            return false;
        }
        if (field.IsPublic)
        {
            return !field.IsDefined(typeof(HideInInspector), true);
        }
        return field.IsDefined(typeof(SerializeField), true);
    }

    private static bool EsCampoOpcionalPermitido(Type tipoScript, string nombreCampo)
    {
        if (tipoScript == typeof(DashboardUI) &&
            (nombreCampo == "textoLogTMP" || nombreCampo == "textoLogLegacy"))
        {
            return true;
        }

        if (CamposOpcionalesOInicializadosEnRuntime.TryGetValue(tipoScript.Name, out HashSet<string> camposPermitidos))
        {
            return camposPermitidos.Contains(nombreCampo);
        }

        return false;
    }

    private static Type ObtenerTipoElementoColeccion(Type tipoColeccion)
    {
        if (tipoColeccion.IsArray)
        {
            return tipoColeccion.GetElementType();
        }
        if (tipoColeccion.IsGenericType)
        {
            Type[] args = tipoColeccion.GetGenericArguments();
            if (args.Length == 1)
            {
                return args[0];
            }
        }
        return null;
    }

    private static void AuditarInfraestructuraYNavMesh(
        List<string> coordenadasManifiesto,
        ResultadoAuditoriaSCADA resultado,
        bool permitirSinNavMeshEnEscenaVacia,
        bool verificarNavMesh)
    {
        // 1. Estanterías / coordenadas del manifiesto base y estación AS/RS
        if (coordenadasManifiesto.Count == 0)
        {
            coordenadasManifiesto.AddRange(new[] { "P1-C1-F1", "P1-C2-F1", "P2-C4-F1" });
        }

        for (int i = 0; i < coordenadasManifiesto.Count; i++)
        {
            string coord = coordenadasManifiesto[i];
            GameObject rack = GameObject.Find(coord);
            if (rack == null || !rack.activeInHierarchy)
            {
                resultado.FallosInfraestructura++;
                resultado.Errores.Add(
                    $"[BLINDAJE BODEGA] No se encontró activa la estantería/coordenada '{coord}' definida en manifiesto.json. " +
                    "Pista: Restaura o activa ese slot dentro de Fabrica_Bodega/Asset/ASRS_Estante_Heroe.");
            }
        }

        GameObject puntoAcopleASRS = GameObject.Find("Punto_Acople_AMR");
        if (puntoAcopleASRS == null || !puntoAcopleASRS.activeInHierarchy)
        {
            resultado.FallosInfraestructura++;
            resultado.Errores.Add(
                "[BLINDAJE BODEGA] Falta o está desactivado 'Punto_Acople_AMR' en el AS/RS. " +
                "Pista: Verifica Fabrica_Bodega/Asset/ASRS_Estante_Heroe/Ascensor_VLM/Punto_Acople_AMR.");
        }

        GameObject plataformaAscensor = GameObject.Find("Plataforma_Ascensor");
        if (plataformaAscensor == null || !plataformaAscensor.activeInHierarchy)
        {
            resultado.FallosInfraestructura++;
            resultado.Errores.Add(
                "[BLINDAJE BODEGA] Falta o está desactivada 'Plataforma_Ascensor' en el AS/RS. " +
                "Pista: Verifica Fabrica_Bodega/Asset/ASRS_Estante_Heroe/Ascensor_VLM/Plataforma_Ascensor.");
        }

        // 2. Muelles TMS
        ModuloDespachoTMS tms = FindFirstObjectByType<ModuloDespachoTMS>(FindObjectsInactive.Include);
        if (tms != null)
        {
            Transform[] muelles = { tms.muelleCamion01, tms.muelleCamion02, tms.muelleCamion03 };
            for (int i = 0; i < muelles.Length; i++)
            {
                if (muelles[i] == null || !muelles[i].gameObject.activeInHierarchy)
                {
                    resultado.FallosInfraestructura++;
                    resultado.Errores.Add(
                        $"[BLINDAJE BODEGA] El muelle Muelle_Camion_0{i + 1} en ModuloDespachoTMS es nulo o está inactivo. " +
                        "Pista: Verifica los objetos Muelle_Camion_01..03 y su asignación en Gestor_Muelles_TMS.");
                }
            }
        }

        // 3. Puntos de carga inductiva de los 3 AMR
        string[] pads = { "Pad_Inductivo_01", "Pad_Inductivo_02", "Pad_Inductivo_03" };
        for (int i = 0; i < pads.Length; i++)
        {
            GameObject pad = GameObject.Find(pads[i]);
            if (pad == null || !pad.activeInHierarchy)
            {
                resultado.FallosInfraestructura++;
                resultado.Errores.Add(
                    $"[BLINDAJE BODEGA] Falta o está inactivo el punto de carga '{pads[i]}'. " +
                    "Pista: Verifica Fabrica_Bodega/Asset/Plataformas/Pad_Inductivo_01..03.");
            }
        }

        // 4. Validez de NavMesh en el piso de la bodega
        if (verificarNavMesh)
        {
            ValidarNavMeshBodega(resultado, permitirSinNavMeshEnEscenaVacia);
        }
    }

    private static void ValidarNavMeshBodega(ResultadoAuditoriaSCADA resultado, bool permitirSinNavMeshEnEscenaVacia)
    {
        ModuloDespachoTMS tms = FindFirstObjectByType<ModuloDespachoTMS>(FindObjectsInactive.Include);
        if (permitirSinNavMeshEnEscenaVacia && tms == null)
        {
            return;
        }

        GameObject pad01 = GameObject.Find("Pad_Inductivo_01");
        GameObject puntoAcopleASRS = GameObject.Find("Punto_Acople_AMR");
        Vector3 origenPad = pad01 != null ? pad01.transform.position : new Vector3(28.00f, 0.25886f, -54.80f);
        Vector3 puntoASRS = puntoAcopleASRS != null ? puntoAcopleASRS.transform.position : new Vector3(-21.20f, 0.25886f, -30.57f);
        Vector3 puntoMuelle01 = (tms != null && tms.muelleCamion01 != null)
            ? new Vector3(tms.muelleCamion01.position.x, 0.25886f, RobotAMR.ZPieRampaCamion)
            : new Vector3(22.68f, 0.25886f, RobotAMR.ZPieRampaCamion);

        ValidarTramoNavMesh("Pad_Inductivo_01 -> Punto_Acople_AMR", origenPad, puntoASRS, resultado);
        ValidarTramoNavMesh("Punto_Acople_AMR -> Pie de Rampa Muelle_Camion_01", puntoASRS, puntoMuelle01, resultado);
    }

    private static void ValidarTramoNavMesh(
        string nombreTramo,
        Vector3 origenAprox,
        Vector3 destinoAprox,
        ResultadoAuditoriaSCADA resultado)
    {
        if (!NavMesh.SamplePosition(origenAprox, out NavMeshHit hitOrigen, 3.0f, NavMesh.AllAreas))
        {
            resultado.FallosInfraestructura++;
            resultado.Errores.Add(
                $"[BLINDAJE BODEGA] El NavMesh no cubre el origen del tramo '{nombreTramo}' ({origenAprox}). " +
                "Pista: Verifica el NavMeshSurface en 'Fabrica_Bodega' y vuelve a ejecutar Bake.");
            return;
        }

        if (!NavMesh.SamplePosition(destinoAprox, out NavMeshHit hitDestino, 3.0f, NavMesh.AllAreas))
        {
            resultado.FallosInfraestructura++;
            resultado.Errores.Add(
                $"[BLINDAJE BODEGA] El NavMesh no cubre el destino del tramo '{nombreTramo}' ({destinoAprox}). " +
                "Pista: Verifica el NavMeshSurface en 'Fabrica_Bodega' y vuelve a ejecutar Bake.");
            return;
        }

        NavMeshPath camino = new NavMeshPath();
        bool calculado = NavMesh.CalculatePath(hitOrigen.position, hitDestino.position, NavMesh.AllAreas, camino);
        if (!calculado || camino.status != NavMeshPathStatus.PathComplete)
        {
            resultado.FallosInfraestructura++;
            resultado.Errores.Add(
                $"[BLINDAJE BODEGA] Ruta NavMesh incompleta o bloqueada en '{nombreTramo}' (estado={camino.status}). " +
                "Pista: Revisa obstáculos estáticos o re-hornea el NavMeshSurface en 'Fabrica_Bodega'.");
        }
    }

    private static List<GameObject> ObtenerTodosLosGameObjectsDeEscenasCargadas()
    {
        List<GameObject> resultado = new List<GameObject>(256);
        int sceneCount = SceneManager.sceneCount;
        for (int s = 0; s < sceneCount; s++)
        {
            Scene escena = SceneManager.GetSceneAt(s);
            if (!escena.isLoaded) continue;

            GameObject[] raices = escena.GetRootGameObjects();
            for (int r = 0; r < raices.Length; r++)
            {
                AgregarJerarquiaRecursiva(raices[r].transform, resultado);
            }
        }
        return resultado;
    }

    private static void AgregarJerarquiaRecursiva(Transform nodo, List<GameObject> acumulador)
    {
        if (nodo == null) return;
        acumulador.Add(nodo.gameObject);
        for (int i = 0; i < nodo.childCount; i++)
        {
            AgregarJerarquiaRecursiva(nodo.GetChild(i), acumulador);
        }
    }

    private static string ObtenerRutaJerarquia(Transform t)
    {
        if (t == null) return string.Empty;
        StringBuilder sb = new StringBuilder(t.name);
        Transform cursor = t.parent;
        while (cursor != null)
        {
            sb.Insert(0, '/');
            sb.Insert(0, cursor.name);
            cursor = cursor.parent;
        }
        return sb.ToString();
    }

    private static void EmitirReporteConsola(ResultadoAuditoriaSCADA resultado)
    {
        for (int i = 0; i < resultado.Advertencias.Count; i++)
        {
            Debug.LogWarning(resultado.Advertencias[i]);
        }

        if (resultado.EstaSano)
        {
            Debug.Log("<color=#10B981>[BLINDAJE SCADA OK] Integridad verificada: 0 objetos borrados, 0 referencias rotas, JSONs intactos.</color>");
            return;
        }

        for (int i = 0; i < resultado.Errores.Count; i++)
        {
            Debug.LogError(resultado.Errores[i]);
        }

        EmitirResumenFalloConsola(resultado);
    }

    private static void EmitirResumenFalloConsola(ResultadoAuditoriaSCADA resultado)
    {
        StringBuilder resumen = new StringBuilder();
        resumen.AppendLine(
            $"[BLINDAJE SCADA FALLIDO] Total de fallos: {resultado.TotalFallos} " +
            $"(JSONs: {resultado.ArchivosJsonFaltantes} | Escena/Duplicados: {resultado.ObjetosFaltantesODuplicados} | " +
            $"Inspector/Referencias: {resultado.ReferenciasRotasONulas} | Bodega/NavMesh: {resultado.FallosInfraestructura})");
        for (int i = 0; i < resultado.Errores.Count; i++)
        {
            resumen.AppendLine($"  {i + 1}. {resultado.Errores[i]}");
        }
        Debug.LogError(resumen.ToString().TrimEnd());
    }
}
