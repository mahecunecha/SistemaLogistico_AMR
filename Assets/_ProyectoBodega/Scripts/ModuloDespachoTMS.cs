using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuloDespachoTMS : MonoBehaviour
{
    public static ModuloDespachoTMS Instance { get; private set; }

    [Header("Bahías de Carga / Muelles (Sector Este)")]
    public Transform muelleCamion01;
    public Transform muelleCamion02;
    public Transform muelleCamion03;

    [Header("Compuertas Traseras (Sellado 8/8)")]
    public GameObject puertaCerradaCamion1;
    public GameObject puertaCerradaCamion2;
    public GameObject puertaCerradaCamion3;

    [Header("Trazabilidad LIFO por Camión")]
    private Stack<Pedido> matrizLIFOCamion1 = new Stack<Pedido>();
    private Stack<Pedido> matrizLIFOCamion2 = new Stack<Pedido>();
    private Stack<Pedido> matrizLIFOCamion3 = new Stack<Pedido>();

    private List<GameObject> palletsCamion1 = new List<GameObject>();
    private List<GameObject> palletsCamion2 = new List<GameObject>();
    private List<GameObject> palletsCamion3 = new List<GameObject>();

    private float pesoAcumuladoCamion1 = 0f;
    private float pesoAcumuladoCamion2 = 0f;
    private float pesoAcumuladoCamion3 = 0f;

    public const float CapacidadMaximaCamionKg = 12000f;
    public const int CapacidadMaximaPalletsCamion = 8;
    public const int ColumnasPorCamion = 2;
    public const int FilasPorCamion = 4;
    public const float OffsetColumnaLateralMetros = 0.65f;
    public const float OffsetFilaFondoZMetros = 1.80f;
    private const float EspaciadoSlotLIFOMetros = 1.40f;

    // Índices 1..3 correspondientes a Muelle_Camion_01..03
    private readonly int[] slotsReservados = new int[4];
    private readonly bool[,] slotReservadoUOcupado = new bool[4, CapacidadMaximaPalletsCamion + 1];
    private readonly bool[,] slotEntregadoFisico = new bool[4, CapacidadMaximaPalletsCamion + 1];
    private readonly float[,] pesoReservadoPorSlot = new float[4, CapacidadMaximaPalletsCamion + 1];
    private readonly bool[] cerradoPorTopePeso = new bool[4];
    private readonly bool[] renovacionEnCurso = new bool[4];
    private readonly bool[] camionSellado = new bool[4];
    private bool reinicioConvoyEnCurso = false;
    private int camionTurnoActual = 1;

    [Header("Protocolo de Inspección Humana en Muelle")]
    public float duracionInspeccionHumana = 45f;
    private readonly bool[] inspeccionHumanaEnCurso = new bool[4];
    private readonly Coroutine[] rutinasInspeccionHumana = new Coroutine[4];

    [Header("Clausura por Falla / Bloqueo en Bahía (Manual)")]
    private readonly bool[] muelleClausuradoPorIncidente = new bool[4];
    private readonly string[] motivoClausuraMuelle = new string[4];
    private readonly Coroutine[] rutinasRecargaEscalonada = new Coroutine[4];
    private bool alertaTodosMuellesCerradosBuzonActiva = false;
    public event System.Action<int, bool, string> OnEstadoClausuraMuelleCambiado;
    public event System.Action OnMuellesDisponiblesNuevamente;

    [System.Serializable]
    public struct RegistroEstibaTurno
    {
        public string Timestamp;
        public int NumeroCamion;
        public int SlotLIFO;
        public string IdPallet;
        public string Producto;
        public string SKU;
        public float PesoKg;
    }

    private readonly List<RegistroEstibaTurno> historialEstibasTurno = new List<RegistroEstibaTurno>();
    public IReadOnlyList<RegistroEstibaTurno> HistorialEstibasTurno => historialEstibasTurno;

    public Stack<Pedido> MatrizLIFOCamion1 => matrizLIFOCamion1;
    public Stack<Pedido> MatrizLIFOCamion2 => matrizLIFOCamion2;
    public Stack<Pedido> MatrizLIFOCamion3 => matrizLIFOCamion3;

    public float PesoCamion1 => pesoAcumuladoCamion1;
    public float PesoCamion2 => pesoAcumuladoCamion2;
    public float PesoCamion3 => pesoAcumuladoCamion3;
    public int CamionTurnoActual => camionTurnoActual;

    public int ObtenerCantidadEstibasCamion(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 1: return Mathf.Max(palletsCamion1.Count, matrizLIFOCamion1.Count);
            case 2: return Mathf.Max(palletsCamion2.Count, matrizLIFOCamion2.Count);
            case 3: return Mathf.Max(palletsCamion3.Count, matrizLIFOCamion3.Count);
            default: return 0;
        }
    }

    public bool EstaCamionSellado(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return camionSellado[numeroCamion];
    }

    public bool EstaEnInspeccionHumana(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return inspeccionHumanaEnCurso[numeroCamion];
    }

    public bool EstaEnRenovacion(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return renovacionEnCurso[numeroCamion];
    }

    public bool EstaMuelleClausuradoPorIncidente(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return muelleClausuradoPorIncidente[numeroCamion];
    }

    public string ObtenerMotivoClausuraMuelle(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return string.Empty;
        return motivoClausuraMuelle[numeroCamion] ?? string.Empty;
    }

    public bool TieneAlgunMuelleClausuradoOEnInspeccion(out int primerMuelleAfectado)
    {
        for (int i = 1; i <= 3; i++)
        {
            if (muelleClausuradoPorIncidente[i] || inspeccionHumanaEnCurso[i])
            {
                primerMuelleAfectado = i;
                return true;
            }
        }
        primerMuelleAfectado = 0;
        return false;
    }

    public int ContarMuellesAbiertosParaEntrega()
    {
        int abiertos = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (EstaMuelleAbiertoParaEntrega(i)) abiertos++;
        }
        return abiertos;
    }

    public bool EstaMuelleAbiertoParaEntrega(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return !camionSellado[numeroCamion] &&
               !renovacionEnCurso[numeroCamion] &&
               !cerradoPorTopePeso[numeroCamion] &&
               !inspeccionHumanaEnCurso[numeroCamion] &&
               !muelleClausuradoPorIncidente[numeroCamion];
    }

    public bool TieneAlgunMuelleAbierto()
    {
        return EstaMuelleAbiertoParaEntrega(1) || EstaMuelleAbiertoParaEntrega(2) || EstaMuelleAbiertoParaEntrega(3);
    }

    public void EstablecerCamionSellado(int numeroCamion, bool sellado)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return;
        camionSellado[numeroCamion] = sellado;
        ConfigurarPuertaCamion(numeroCamion, sellado);
        if (!sellado)
        {
            cerradoPorTopePeso[numeroCamion] = false;
            alertaTodosMuellesCerradosBuzonActiva = false;
            OnMuellesDisponiblesNuevamente?.Invoke();
        }
    }

    public float ObtenerPesoCamion(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 1: return pesoAcumuladoCamion1;
            case 2: return pesoAcumuladoCamion2;
            case 3: return pesoAcumuladoCamion3;
            default: return 0f;
        }
    }

    public float ObtenerPesoReservadoCamion(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return 0f;
        float total = 0f;
        for (int s = 1; s <= CapacidadMaximaPalletsCamion; s++)
        {
            total += pesoReservadoPorSlot[numeroCamion, s];
        }
        return total;
    }

    public float ObtenerPesoProyectadoCamion(int numeroCamion)
    {
        return ObtenerPesoCamion(numeroCamion) + ObtenerPesoReservadoCamion(numeroCamion);
    }

    private int ContarSlotsOcupadosOReservados(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return 0;
        int conteo = 0;
        for (int s = 1; s <= CapacidadMaximaPalletsCamion; s++)
        {
            if (slotReservadoUOcupado[numeroCamion, s]) conteo++;
        }
        return conteo;
    }

    private int ObtenerPrimerSlotLibre(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return 1;
        for (int s = 1; s <= CapacidadMaximaPalletsCamion; s++)
        {
            if (!slotReservadoUOcupado[numeroCamion, s]) return s;
        }
        return CapacidadMaximaPalletsCamion;
    }

    private bool TieneReservasEnTransito(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        for (int s = 1; s <= CapacidadMaximaPalletsCamion; s++)
        {
            if (slotReservadoUOcupado[numeroCamion, s] && !slotEntregadoFisico[numeroCamion, s])
            {
                return true;
            }
        }
        return false;
    }

    private void LimpiarEstadoSlotsCamion(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return;
        for (int s = 0; s <= CapacidadMaximaPalletsCamion; s++)
        {
            slotReservadoUOcupado[numeroCamion, s] = false;
            slotEntregadoFisico[numeroCamion, s] = false;
            pesoReservadoPorSlot[numeroCamion, s] = 0f;
        }
    }

    public static float CalcularXOffsetLIFO(int slotLIFO)
    {
        int slotClamped = Mathf.Clamp(slotLIFO, 1, CapacidadMaximaPalletsCamion);
        bool esColumnaIzquierda = ((slotClamped - 1) % ColumnasPorCamion) == 0;
        return esColumnaIzquierda ? -OffsetColumnaLateralMetros : OffsetColumnaLateralMetros;
    }

    public static float CalcularZOffsetLIFO(int slotLIFO)
    {
        int slotClamped = Mathf.Clamp(slotLIFO, 1, CapacidadMaximaPalletsCamion);
        int filaIndex = (slotClamped - 1) / ColumnasPorCamion;
        return OffsetFilaFondoZMetros - (filaIndex * EspaciadoSlotLIFOMetros);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Auto-resolver referencias si no han sido asignadas en Inspector
        if (muelleCamion01 == null)
        {
            GameObject m1 = GameObject.Find("Muelle_Camion_01") ?? GameObject.Find("Zona_Camion");
            if (m1 != null) muelleCamion01 = m1.transform;
        }
        if (muelleCamion02 == null)
        {
            GameObject m2 = GameObject.Find("Muelle_Camion_02");
            if (m2 != null) muelleCamion02 = m2.transform;
        }
        if (muelleCamion03 == null)
        {
            GameObject m3 = GameObject.Find("Muelle_Camion_03");
            if (m3 != null) muelleCamion03 = m3.transform;
        }

        ResolverPuertasCerradas();
        AbrirTodasLasCompuertas();
    }

    private void ResolverPuertasCerradas()
    {
        if (puertaCerradaCamion1 == null && muelleCamion01 != null && muelleCamion01.parent != null)
        {
            Transform t = muelleCamion01.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion1 = t.gameObject;
        }
        if (puertaCerradaCamion2 == null && muelleCamion02 != null && muelleCamion02.parent != null)
        {
            Transform t = muelleCamion02.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion2 = t.gameObject;
        }
        if (puertaCerradaCamion3 == null && muelleCamion03 != null && muelleCamion03.parent != null)
        {
            Transform t = muelleCamion03.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion3 = t.gameObject;
        }
    }

    private void ConfigurarPuertaCamion(int numeroCamion, bool cerrada)
    {
        GameObject puerta = null;
        switch (numeroCamion)
        {
            case 1: puerta = puertaCerradaCamion1; break;
            case 2: puerta = puertaCerradaCamion2; break;
            case 3: puerta = puertaCerradaCamion3; break;
        }

        if (puerta != null && puerta.activeSelf != cerrada)
        {
            puerta.SetActive(cerrada);
        }
    }

    private void AbrirTodasLasCompuertas()
    {
        ConfigurarPuertaCamion(1, false);
        ConfigurarPuertaCamion(2, false);
        ConfigurarPuertaCamion(3, false);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ReiniciarEstadoMuelles()
    {
        StopAllCoroutines();
        ResolverPuertasCerradas();
        AbrirTodasLasCompuertas();

        LimpiarListaPalletsCamion(palletsCamion1);
        LimpiarListaPalletsCamion(palletsCamion2);
        LimpiarListaPalletsCamion(palletsCamion3);

        matrizLIFOCamion1.Clear();
        matrizLIFOCamion2.Clear();
        matrizLIFOCamion3.Clear();
        historialEstibasTurno.Clear();

        pesoAcumuladoCamion1 = 0f;
        pesoAcumuladoCamion2 = 0f;
        pesoAcumuladoCamion3 = 0f;

        for (int i = 0; i < 4; i++)
        {
            slotsReservados[i] = 0;
            cerradoPorTopePeso[i] = false;
            renovacionEnCurso[i] = false;
            camionSellado[i] = false;
            inspeccionHumanaEnCurso[i] = false;
            rutinasInspeccionHumana[i] = null;
            rutinasRecargaEscalonada[i] = null;
            muelleClausuradoPorIncidente[i] = false;
            motivoClausuraMuelle[i] = string.Empty;
            if (i >= 1)
            {
                LimpiarEstadoSlotsCamion(i);
            }
        }
        alertaTodosMuellesCerradosBuzonActiva = false;
        reinicioConvoyEnCurso = false;
        camionTurnoActual = 1;

        for (int i = 1; i <= 3; i++)
        {
            OnEstadoClausuraMuelleCambiado?.Invoke(i, false, string.Empty);
        }
    }

    private static void LimpiarListaPalletsCamion(List<GameObject> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] != null)
            {
                if (Application.isPlaying) Destroy(lista[i]);
                else DestroyImmediate(lista[i]);
            }
        }
        lista.Clear();
    }

    private List<GameObject> ObtenerListaPallets(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 2: return palletsCamion2;
            case 3: return palletsCamion3;
            default: return palletsCamion1;
        }
    }

    public Transform ConsultarMuelleDestino(Pedido pedido, out int numeroCamion, out int slotLIFOProyectado)
    {
        numeroCamion = 0;
        float pesoPedido = (pedido != null && pedido.peso_kg > 0f) ? pedido.peso_kg : 0f;

        // Rotación secuencial 1 -> 2 -> 3 respetando máximo 8 pallets (2x4, físicos o reservados), tonelaje máximo 12,000 kg, sellado, clausura e inspección humana
        for (int offset = 0; offset < 3; offset++)
        {
            int candidato = ((camionTurnoActual - 1 + offset) % 3) + 1;
            int ocupados = Mathf.Max(slotsReservados[candidato], Mathf.Max(ContarSlotsOcupadosOReservados(candidato), ObtenerListaPallets(candidato).Count));
            float pesoProyectado = ObtenerPesoProyectadoCamion(candidato);
            bool dentroCapacidadPeso = (pesoProyectado + pesoPedido) <= CapacidadMaximaCamionKg;

            if (EstaMuelleAbiertoParaEntrega(candidato))
            {
                if (ocupados < CapacidadMaximaPalletsCamion && dentroCapacidadPeso)
                {
                    numeroCamion = candidato;
                    break;
                }

                // Si el nuevo pallet excedería 12,000 kg y el camión ya tiene estibas sin reservas en vuelo, cerrar y rotar
                if (!dentroCapacidadPeso && ocupados > 0 && !TieneReservasEnTransito(candidato))
                {
                    cerradoPorTopePeso[candidato] = true;
                    if (Application.isPlaying && !renovacionEnCurso[candidato] && !camionSellado[candidato])
                    {
                        StartCoroutine(SellarCamionYVerificarConvoy(candidato, 2.5f));
                    }
                    else
                    {
                        ConfigurarPuertaCamion(candidato, true);
                        camionSellado[candidato] = true;
                        EvaluarReinicioConvoySiCorresponde();
                    }
                }
            }
        }

        if (numeroCamion == 0)
        {
            slotLIFOProyectado = 0;
            if (!TieneAlgunMuelleAbierto())
            {
                const string msgBloqueo = "<color=#EF4444>[API MÓDULO 5 -> WMS]:</color> Todos los muelles (01..03) están cerrados/sellados o en inspección humana. Asignación bloqueada.";
                DashboardUI.Instance?.RegistrarLog(msgBloqueo);
                if (!alertaTodosMuellesCerradosBuzonActiva)
                {
                    alertaTodosMuellesCerradosBuzonActiva = true;
                    DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                        $"[BLOQUEO TOTAL DE MUELLES (01-03) — {RelojSimuladorTurno.HoraActualFormateada}]: Todos los muelles están cerrados, sellados o en inspección humana. Flota en espera segura sin detener el turno hasta despeje de bahía.");
                }
                Debug.LogWarning("[API MÓDULO 5 -> TMS]: Todos los muelles (01..03) están cerrados/sellados o en inspección humana. Asignación bloqueada.");
                return null;
            }

            Debug.LogWarning("[API MÓDULO 5 -> TMS]: Muelles abiertos en renovación o al límite de 8 pallets / 12,000 kg; esperando disponibilidad.");
            return null;
        }

        alertaTodosMuellesCerradosBuzonActiva = false;
        int slotAsignado = ObtenerPrimerSlotLibre(numeroCamion);
        slotReservadoUOcupado[numeroCamion, slotAsignado] = true;
        pesoReservadoPorSlot[numeroCamion, slotAsignado] = pesoPedido;
        slotsReservados[numeroCamion] = Mathf.Clamp(
            Mathf.Max(ContarSlotsOcupadosOReservados(numeroCamion), ObtenerListaPallets(numeroCamion).Count),
            1,
            CapacidadMaximaPalletsCamion);
        slotLIFOProyectado = slotAsignado;

        if (slotsReservados[numeroCamion] >= CapacidadMaximaPalletsCamion ||
            ObtenerPesoProyectadoCamion(numeroCamion) + Pedido.PesoMinimoIndustrialKg > CapacidadMaximaCamionKg)
        {
            camionTurnoActual = (numeroCamion % 3) + 1;
        }
        else
        {
            camionTurnoActual = numeroCamion;
        }

        string idPallet = pedido != null ? pedido.id_pallet : "PAL-DESC";
        string logMensaje = $"<color=#38BDF8>[API MÓDULO 5 -> WMS]:</color> Ruta GPS validada. Asignando {idPallet} ({pesoPedido:F0} kg) a Muelle_Camion_0{numeroCamion} (Slot LIFO #{slotLIFOProyectado}/{CapacidadMaximaPalletsCamion} | Proyectado: {ObtenerPesoProyectadoCamion(numeroCamion):F0}/{CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMensaje);
        Debug.Log(logMensaje);

        return ObtenerTransformMuelle(numeroCamion);
    }

    private int ResolverSiguienteCamionAbierto(int numeroCamionExcluido)
    {
        for (int offset = 1; offset <= 2; offset++)
        {
            int candidato = ((numeroCamionExcluido - 1 + offset) % 3) + 1;
            int ocupados = Mathf.Max(slotsReservados[candidato], Mathf.Max(ContarSlotsOcupadosOReservados(candidato), ObtenerListaPallets(candidato).Count));
            if (EstaMuelleAbiertoParaEntrega(candidato) &&
                ocupados < CapacidadMaximaPalletsCamion &&
                ObtenerPesoProyectadoCamion(candidato) < CapacidadMaximaCamionKg)
            {
                return candidato;
            }
        }

        for (int offset = 1; offset <= 2; offset++)
        {
            int candidato = ((numeroCamionExcluido - 1 + offset) % 3) + 1;
            if (EstaMuelleAbiertoParaEntrega(candidato))
            {
                return candidato;
            }
        }

        return 0;
    }

    public int ClausurarMuellePorIncidenteAMR(int numeroCamionBloqueado, string motivo, out int siguienteCamionOperativo)
    {
        numeroCamionBloqueado = Mathf.Clamp(numeroCamionBloqueado, 1, 3);
        bool flancoClausura = !muelleClausuradoPorIncidente[numeroCamionBloqueado];
        if (rutinasInspeccionHumana[numeroCamionBloqueado] != null)
        {
            StopCoroutine(rutinasInspeccionHumana[numeroCamionBloqueado]);
            rutinasInspeccionHumana[numeroCamionBloqueado] = null;
        }
        inspeccionHumanaEnCurso[numeroCamionBloqueado] = false;
        muelleClausuradoPorIncidente[numeroCamionBloqueado] = true;
        motivoClausuraMuelle[numeroCamionBloqueado] = string.IsNullOrEmpty(motivo) ? "AMR averiado/bloqueado en bahía" : motivo;

        siguienteCamionOperativo = ResolverSiguienteCamionAbierto(numeroCamionBloqueado);
        if (siguienteCamionOperativo > 0)
        {
            camionTurnoActual = siguienteCamionOperativo;
        }

        string msgClausura = $"<color=#EF4444>[TMS CLAUSURA MUELLE 0{numeroCamionBloqueado}]:</color> {motivoClausuraMuelle[numeroCamionBloqueado]}. " +
            (siguienteCamionOperativo > 0 ? $"Redirigiendo a Muelle_Camion_0{siguienteCamionOperativo}." : "Sin otros muelles abiertos.");
        DashboardUI.Instance?.RegistrarLog(msgClausura);
        Debug.LogWarning(msgClausura);

        if (flancoClausura)
        {
            string hora = RelojSimuladorTurno.HoraActualFormateada;
            if (siguienteCamionOperativo > 0)
            {
                DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                    $"[CONTINGENCIA MUELLE 0{numeroCamionBloqueado} — {hora}]: {motivoClausuraMuelle[numeroCamionBloqueado]}. Operación reasignada automáticamente a Muelle_Camion_0{siguienteCamionOperativo} preservando continuidad del turno.");
            }
            else
            {
                alertaTodosMuellesCerradosBuzonActiva = true;
                DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                    $"[BLOQUEO TOTAL DE MUELLES (01-03) — {hora}]: {motivoClausuraMuelle[numeroCamionBloqueado]}. Sin muelles alternativos abiertos; flota en espera segura pre-rampa sin cancelar el turno.");
            }
        }

        OnEstadoClausuraMuelleCambiado?.Invoke(numeroCamionBloqueado, true, motivoClausuraMuelle[numeroCamionBloqueado]);
        return siguienteCamionOperativo;
    }

    public bool ConfirmarDespejeHumanoMuelle(int numeroCamion = 0)
    {
        if (numeroCamion <= 0)
        {
            TieneAlgunMuelleClausuradoOEnInspeccion(out numeroCamion);
        }
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        if (!muelleClausuradoPorIncidente[numeroCamion] && !inspeccionHumanaEnCurso[numeroCamion])
        {
            return false;
        }

        if (rutinasInspeccionHumana[numeroCamion] != null)
        {
            StopCoroutine(rutinasInspeccionHumana[numeroCamion]);
            rutinasInspeccionHumana[numeroCamion] = null;
        }

        muelleClausuradoPorIncidente[numeroCamion] = false;
        inspeccionHumanaEnCurso[numeroCamion] = false;
        motivoClausuraMuelle[numeroCamion] = string.Empty;
        alertaTodosMuellesCerradosBuzonActiva = false;
        slotsReservados[numeroCamion] = Mathf.Max(ContarSlotsOcupadosOReservados(numeroCamion), ObtenerCantidadEstibasCamion(numeroCamion));

        if (!EstaMuelleAbiertoParaEntrega(camionTurnoActual))
        {
            camionTurnoActual = numeroCamion;
        }

        string msgDespeje = $"<color=#10B981>[TMS - DESPEJE HUMANO CONFIRMADO]:</color> Operario en piso liberó Muelle_Camion_0{numeroCamion}. Bahía operativa nuevamente.";
        DashboardUI.Instance?.RegistrarLog(msgDespeje);
        Debug.Log(msgDespeje);

        OnEstadoClausuraMuelleCambiado?.Invoke(numeroCamion, false, string.Empty);
        EvaluarReinicioConvoySiCorresponde();
        if (EstaMuelleAbiertoParaEntrega(numeroCamion))
        {
            OnMuellesDisponiblesNuevamente?.Invoke();
        }
        return true;
    }

    public int ReportarInspeccionHumanaMuelle(int numeroCamionBloqueado, out int siguienteCamionOperativo)
    {
        numeroCamionBloqueado = Mathf.Clamp(numeroCamionBloqueado, 1, 3);
        bool flancoInspeccion = !inspeccionHumanaEnCurso[numeroCamionBloqueado] && !muelleClausuradoPorIncidente[numeroCamionBloqueado];
        inspeccionHumanaEnCurso[numeroCamionBloqueado] = true;

        if (Application.isPlaying)
        {
            if (rutinasInspeccionHumana[numeroCamionBloqueado] != null)
            {
                StopCoroutine(rutinasInspeccionHumana[numeroCamionBloqueado]);
            }
            rutinasInspeccionHumana[numeroCamionBloqueado] = StartCoroutine(AutoLiberarInspeccionHumana(numeroCamionBloqueado));
        }

        siguienteCamionOperativo = ResolverSiguienteCamionAbierto(numeroCamionBloqueado);
        if (siguienteCamionOperativo > 0)
        {
            camionTurnoActual = siguienteCamionOperativo;
        }

        if (flancoInspeccion)
        {
            string hora = RelojSimuladorTurno.HoraActualFormateada;
            if (siguienteCamionOperativo > 0)
            {
                DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                    $"[INSPECCIÓN HUMANA MUELLE 0{numeroCamionBloqueado} — {hora}]: Bahía temporalmente en revisión; operación mudada automáticamente a Muelle_Camion_0{siguienteCamionOperativo}.");
            }
            else
            {
                alertaTodosMuellesCerradosBuzonActiva = true;
                DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                    $"[BLOQUEO TOTAL DE MUELLES (01-03) — {hora}]: Muelle_Camion_0{numeroCamionBloqueado} en inspección humana y sin bahías abiertas restantes; flota en espera segura pre-rampa.");
            }
        }

        OnEstadoClausuraMuelleCambiado?.Invoke(numeroCamionBloqueado, true, "Inspección humana en curso");
        return siguienteCamionOperativo;
    }

    private IEnumerator AutoLiberarInspeccionHumana(int numeroCamion)
    {
        yield return new WaitForSeconds(Mathf.Max(1f, duracionInspeccionHumana));
        if (muelleClausuradoPorIncidente[numeroCamion])
        {
            rutinasInspeccionHumana[numeroCamion] = null;
            yield break;
        }
        inspeccionHumanaEnCurso[numeroCamion] = false;
        rutinasInspeccionHumana[numeroCamion] = null;
        alertaTodosMuellesCerradosBuzonActiva = false;

        string msgDespeje = $"<color=#10B981>[TMS - INSPECCIÓN HUMANA]:</color> Operario en piso despejó Muelle_Camion_0{numeroCamion}. Bahía habilitada nuevamente.";
        DashboardUI.Instance?.RegistrarLog(msgDespeje);
        Debug.Log(msgDespeje);
        OnEstadoClausuraMuelleCambiado?.Invoke(numeroCamion, false, string.Empty);
        EvaluarReinicioConvoySiCorresponde();
        if (EstaMuelleAbiertoParaEntrega(numeroCamion))
        {
            OnMuellesDisponiblesNuevamente?.Invoke();
        }
    }

    public void CancelarReservaSlot(int numeroCamion, int slotReservado)
    {
        if (numeroCamion < 1 || numeroCamion > 3 || slotReservado <= 0) return;

        int slotClamped = Mathf.Clamp(slotReservado, 1, CapacidadMaximaPalletsCamion);
        if (!slotEntregadoFisico[numeroCamion, slotClamped])
        {
            slotReservadoUOcupado[numeroCamion, slotClamped] = false;
            pesoReservadoPorSlot[numeroCamion, slotClamped] = 0f;
        }

        int entregados = ObtenerCantidadEstibasCamion(numeroCamion);
        slotsReservados[numeroCamion] = Mathf.Max(entregados, ContarSlotsOcupadosOReservados(numeroCamion));
    }

    public Transform ReasignarMuelleAbierto(Pedido pedido, int numeroCamionAnterior, int slotAnterior, out int nuevoCamion, out int nuevoSlot)
    {
        CancelarReservaSlot(numeroCamionAnterior, slotAnterior);
        return ConsultarMuelleDestino(pedido, out nuevoCamion, out nuevoSlot);
    }

    public Transform ObtenerTransformMuelle(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 1: return muelleCamion01;
            case 2: return muelleCamion02;
            case 3: return muelleCamion03;
            default: return muelleCamion01;
        }
    }

    public int ObtenerSlotLIFOProyectado(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) numeroCamion = 1;
        return ObtenerPrimerSlotLibre(numeroCamion);
    }

    public Vector3 ObtenerCoordenadaSlotLIFO(int numeroCamion, int slotLIFOProyectado)
    {
        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        if (muelleTransform == null) return Vector3.zero;

        // Matriz LIFO física 2x4 (Flatbed): 2 columnas simétricas en X (-0.65m / +0.65m) y 4 filas de fondo a rampa en Z
        float xOffset = CalcularXOffsetLIFO(slotLIFOProyectado);
        float zOffset = CalcularZOffsetLIFO(slotLIFOProyectado);

        Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
        float ySuperficie = 1.81264f;
        float zCentroCarga = muelleTransform.position.z;
        float xCentroCarga = muelleTransform.position.x;

        if (sueloFalso != null)
        {
            Collider colSuelo = sueloFalso.GetComponent<Collider>();
            ySuperficie = colSuelo != null ? colSuelo.bounds.max.y : (sueloFalso.position.y + 0.066f);
            zCentroCarga = sueloFalso.position.z;
            xCentroCarga = sueloFalso.position.x;
        }

        return new Vector3(xCentroCarga + xOffset, ySuperficie, zCentroCarga + zOffset);
    }

    public Vector3 ObtenerCoordenadaSlotActivo(int numeroCamion)
    {
        int slotProyectado = ObtenerSlotLIFOProyectado(numeroCamion);
        return ObtenerCoordenadaSlotLIFO(numeroCamion, slotProyectado);
    }

    public void RegistrarEstibaEnCamion(int numeroCamion, Pedido pedido, GameObject palletFisico, int slotAsignado = 0)
    {
        if (pedido == null) return;
        if (numeroCamion < 1 || numeroCamion > 3) numeroCamion = 1;

        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        Stack<Pedido> pilaObjetivo;
        List<GameObject> listaPallets;
        float pesoAcumulado;

        switch (numeroCamion)
        {
            case 2:
                matrizLIFOCamion2.Push(pedido);
                pesoAcumuladoCamion2 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion2;
                listaPallets = palletsCamion2;
                pesoAcumulado = pesoAcumuladoCamion2;
                break;
            case 3:
                matrizLIFOCamion3.Push(pedido);
                pesoAcumuladoCamion3 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion3;
                listaPallets = palletsCamion3;
                pesoAcumulado = pesoAcumuladoCamion3;
                break;
            default:
                matrizLIFOCamion1.Push(pedido);
                pesoAcumuladoCamion1 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion1;
                listaPallets = palletsCamion1;
                pesoAcumulado = pesoAcumuladoCamion1;
                break;
        }

        int estibasActuales = Mathf.Max(listaPallets.Count + (palletFisico != null ? 1 : 0), pilaObjetivo.Count);
        int slotRegistrado = slotAsignado > 0
            ? Mathf.Clamp(slotAsignado, 1, CapacidadMaximaPalletsCamion)
            : Mathf.Clamp(estibasActuales, 1, CapacidadMaximaPalletsCamion);

        slotEntregadoFisico[numeroCamion, slotRegistrado] = true;
        slotReservadoUOcupado[numeroCamion, slotRegistrado] = true;
        pesoReservadoPorSlot[numeroCamion, slotRegistrado] = 0f;

        if (palletFisico != null && muelleTransform != null)
        {
            listaPallets.Add(palletFisico);
            palletFisico.transform.SetParent(null, true);

            float xOffset = CalcularXOffsetLIFO(slotRegistrado);
            float zOffset = CalcularZOffsetLIFO(slotRegistrado);

            Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
            float ySuperficie = 1.81264f;
            float zCentroCarga = muelleTransform.position.z;
            float xCentroCarga = muelleTransform.position.x;

            if (sueloFalso != null)
            {
                Collider colSuelo = sueloFalso.GetComponent<Collider>();
                ySuperficie = colSuelo != null ? colSuelo.bounds.max.y : (sueloFalso.position.y + 0.066f);
                zCentroCarga = sueloFalso.position.z;
                xCentroCarga = sueloFalso.position.x;
            }

            Vector3 posicionLIFO = new Vector3(
                xCentroCarga + xOffset,
                ySuperficie,
                zCentroCarga + zOffset
            );

            palletFisico.transform.position = posicionLIFO;
            palletFisico.transform.rotation = muelleTransform.rotation;
        }

        estibasActuales = Mathf.Max(listaPallets.Count, pilaObjetivo.Count);
        slotsReservados[numeroCamion] = Mathf.Max(ContarSlotsOcupadosOReservados(numeroCamion), estibasActuales);

        if (estibasActuales == 1)
        {
            EvaluarRecargaEscalonadaEnPrimeraEntrega(numeroCamion);
        }

        historialEstibasTurno.Add(new RegistroEstibaTurno
        {
            Timestamp = RelojSimuladorTurno.HoraActualFormateada,
            NumeroCamion = numeroCamion,
            SlotLIFO = slotRegistrado,
            IdPallet = !string.IsNullOrEmpty(pedido.id_pallet) ? pedido.id_pallet : "PAL-DESC",
            Producto = !string.IsNullOrEmpty(pedido.nombre_producto) ? pedido.nombre_producto : "Carga Industrial",
            SKU = !string.IsNullOrEmpty(pedido.codigo_sku) ? pedido.codigo_sku : "SKU-STD",
            PesoKg = pedido.peso_kg
        });

        string logMatriz = $"<color=cyan>[MATRIZ LIFO CAMIÓN {numeroCamion}]:</color> Estiba {pedido.id_pallet} apilada en Slot #{slotRegistrado} ({estibasActuales}/{CapacidadMaximaPalletsCamion} | Peso acumulado: {pesoAcumulado:F0} kg / {CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMatriz);
        Debug.Log(logMatriz);
        GestorAudioSCADA.Instance?.ReproducirConfirmacionEntrega();

        bool llenoPorSlots = estibasActuales >= CapacidadMaximaPalletsCamion;
        bool llenoPorTopePeso = !TieneReservasEnTransito(numeroCamion) &&
                                (pesoAcumulado + Pedido.PesoMinimoIndustrialKg > CapacidadMaximaCamionKg);

        if ((llenoPorSlots || llenoPorTopePeso) && !renovacionEnCurso[numeroCamion] && !camionSellado[numeroCamion])
        {
            if (llenoPorSlots)
            {
                slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
            }
            if (llenoPorTopePeso)
            {
                cerradoPorTopePeso[numeroCamion] = true;
            }

            if (Application.isPlaying)
            {
                StartCoroutine(SellarCamionYVerificarConvoy(numeroCamion, 2.5f));
            }
            else
            {
                ConfigurarPuertaCamion(numeroCamion, true);
                camionSellado[numeroCamion] = true;
            }
        }
    }

    private bool EstaMuelleOperativoSinBloqueoHumano(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) return false;
        return !muelleClausuradoPorIncidente[numeroCamion] && !inspeccionHumanaEnCurso[numeroCamion];
    }

    private void EvaluarRecargaEscalonadaEnPrimeraEntrega(int muelleConPrimeraEntrega)
    {
        if (muelleConPrimeraEntrega == 3)
        {
            IntentarRecargarMuelleEscalonado(1);
            IntentarRecargarMuelleEscalonado(2);
            return;
        }

        if (muelleConPrimeraEntrega == 1)
        {
            IntentarRecargarMuelleEscalonado(3);
            if (!EstaMuelleOperativoSinBloqueoHumano(3))
            {
                IntentarRecargarMuelleEscalonado(2);
            }
            return;
        }

        if (muelleConPrimeraEntrega == 2)
        {
            if (!EstaMuelleOperativoSinBloqueoHumano(3))
            {
                IntentarRecargarMuelleEscalonado(1);
            }
            if (!EstaMuelleOperativoSinBloqueoHumano(1))
            {
                IntentarRecargarMuelleEscalonado(3);
            }
        }
    }

    public bool IntentarRecargarMuelleEscalonado(int numeroCamion)
    {
        if (!EstaMuelleOperativoSinBloqueoHumano(numeroCamion)) return false;

        bool requiereRecarga = camionSellado[numeroCamion] ||
                               cerradoPorTopePeso[numeroCamion] ||
                               renovacionEnCurso[numeroCamion] ||
                               ObtenerCantidadEstibasCamion(numeroCamion) >= CapacidadMaximaPalletsCamion;
        if (!requiereRecarga) return false;

        bool ocupadoAhora = renovacionEnCurso[numeroCamion] ||
                            TieneReservasEnTransito(numeroCamion) ||
                            (Application.isPlaying && AlgunAMREnMuelle(numeroCamion));

        if (!ocupadoAhora)
        {
            ReiniciarCamionParaNuevoConvoy(numeroCamion);
            alertaTodosMuellesCerradosBuzonActiva = false;
            string msgEscalonado = $"<color=#38BDF8>[TMS RECARGA ESCALONADA]:</color> Muelle_Camion_0{numeroCamion} rehabilitado con nuevo remolque vacío.";
            DashboardUI.Instance?.RegistrarLog(msgEscalonado);
            Debug.Log(msgEscalonado);
            OnMuellesDisponiblesNuevamente?.Invoke();
            return true;
        }

        if (Application.isPlaying && rutinasRecargaEscalonada[numeroCamion] == null)
        {
            rutinasRecargaEscalonada[numeroCamion] = StartCoroutine(EsperarYRecargarMuelleEscalonado(numeroCamion));
        }
        return false;
    }

    private IEnumerator EsperarYRecargarMuelleEscalonado(int numeroCamion)
    {
        while (EstaMuelleOperativoSinBloqueoHumano(numeroCamion) &&
               (renovacionEnCurso[numeroCamion] || TieneReservasEnTransito(numeroCamion) || AlgunAMREnMuelle(numeroCamion)))
        {
            yield return new WaitForSeconds(0.25f);
        }

        rutinasRecargaEscalonada[numeroCamion] = null;
        if (!EstaMuelleOperativoSinBloqueoHumano(numeroCamion)) yield break;

        bool requiereRecarga = camionSellado[numeroCamion] ||
                               cerradoPorTopePeso[numeroCamion] ||
                               ObtenerCantidadEstibasCamion(numeroCamion) >= CapacidadMaximaPalletsCamion;
        if (!requiereRecarga) yield break;

        ReiniciarCamionParaNuevoConvoy(numeroCamion);
        alertaTodosMuellesCerradosBuzonActiva = false;
        string msgEscalonado = $"<color=#38BDF8>[TMS RECARGA ESCALONADA]:</color> Muelle_Camion_0{numeroCamion} rehabilitado tras despeje seguro con nuevo remolque vacío.";
        DashboardUI.Instance?.RegistrarLog(msgEscalonado);
        Debug.Log(msgEscalonado);
        OnMuellesDisponiblesNuevamente?.Invoke();
    }

    private IEnumerator SellarCamionYVerificarConvoy(int numeroCamion, float delay = 2.5f)
    {
        renovacionEnCurso[numeroCamion] = true;
        yield return new WaitForSeconds(delay);

        // Esperar hasta que ningún AMR permanezca sobre la rampa o cama de este camión (Z > -2.0f)
        while (AlgunAMREnMuelle(numeroCamion))
        {
            yield return new WaitForSeconds(0.25f);
        }

        // 1. Sellar visualmente las compuertas traseras del remolque lleno
        ConfigurarPuertaCamion(numeroCamion, true);
        camionSellado[numeroCamion] = true;

        // 2. Destruir los pallets internos tras sellar para optimizar GPU/RAM, manteniendo el conteo bloqueado
        int estibasSelladas = ObtenerCantidadEstibasCamion(numeroCamion);
        float pesoSellado = ObtenerPesoCamion(numeroCamion);
        List<GameObject> listaPallets = ObtenerListaPallets(numeroCamion);
        LimpiarListaPalletsCamion(listaPallets);
        slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
        renovacionEnCurso[numeroCamion] = false;

        string msgSellado = $"<color=#10B981>[TMS MÓDULO 5]:</color> Muelle_Camion_0{numeroCamion} completo ({estibasSelladas}/{CapacidadMaximaPalletsCamion} estibas 2x4 | {pesoSellado:F0}/{CapacidadMaximaCamionKg:F0} kg). Compuertas selladas; habilitando siguiente bahía.";
        DashboardUI.Instance?.RegistrarLog(msgSellado);
        Debug.Log(msgSellado);

        // 3. Si todos los muelles operativos (no clausurados) ya quedaron llenos y sellados, despachar el convoy sin bloqueo por bahía clausurada
        if (TodosLosMuellesOperativosEstanSellados(out _) && !reinicioConvoyEnCurso)
        {
            yield return StartCoroutine(EjecutarReinicioConvoyOperativo());
        }
    }

    private bool TodosLosMuellesOperativosEstanSellados(out int cantidadSellados)
    {
        cantidadSellados = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (muelleClausuradoPorIncidente[i] || inspeccionHumanaEnCurso[i])
            {
                continue;
            }
            if (!camionSellado[i])
            {
                return false;
            }
            cantidadSellados++;
        }
        return cantidadSellados >= 1;
    }

    private void EvaluarReinicioConvoySiCorresponde()
    {
        if (!reinicioConvoyEnCurso && Application.isPlaying && TodosLosMuellesOperativosEstanSellados(out _))
        {
            StartCoroutine(EjecutarReinicioConvoyOperativo());
        }
    }

    private void ReiniciarCamionParaNuevoConvoy(int numeroCamion)
    {
        ConfigurarPuertaCamion(numeroCamion, false);
        switch (numeroCamion)
        {
            case 1:
                matrizLIFOCamion1.Clear();
                pesoAcumuladoCamion1 = 0f;
                LimpiarListaPalletsCamion(palletsCamion1);
                break;
            case 2:
                matrizLIFOCamion2.Clear();
                pesoAcumuladoCamion2 = 0f;
                LimpiarListaPalletsCamion(palletsCamion2);
                break;
            case 3:
                matrizLIFOCamion3.Clear();
                pesoAcumuladoCamion3 = 0f;
                LimpiarListaPalletsCamion(palletsCamion3);
                break;
        }
        LimpiarEstadoSlotsCamion(numeroCamion);
        slotsReservados[numeroCamion] = 0;
        cerradoPorTopePeso[numeroCamion] = false;
        renovacionEnCurso[numeroCamion] = false;
        camionSellado[numeroCamion] = false;
    }

    private IEnumerator EjecutarReinicioConvoyOperativo()
    {
        reinicioConvoyEnCurso = true;
        yield return new WaitForSeconds(4.0f);

        int cantidadRenovados = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (!muelleClausuradoPorIncidente[i] && !inspeccionHumanaEnCurso[i] && camionSellado[i])
            {
                ReiniciarCamionParaNuevoConvoy(i);
                cantidadRenovados++;
            }
        }

        int primerAbierto = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (EstaMuelleAbiertoParaEntrega(i))
            {
                primerAbierto = i;
                break;
            }
        }
        if (primerAbierto > 0)
        {
            alertaTodosMuellesCerradosBuzonActiva = false;
        }
        camionTurnoActual = primerAbierto > 0 ? primerAbierto : 1;
        reinicioConvoyEnCurso = false;

        string msgConvoy = cantidadRenovados >= 3
            ? "<color=#38BDF8>[TMS MÓDULO 5]:</color> Convoy de 3 camiones despachado a ruta. 3 nuevos remolques vacíos posicionados en Muelles 01, 02 y 03."
            : $"<color=#38BDF8>[TMS MÓDULO 5]:</color> Convoy parcial ({cantidadRenovados} bahías operativas) despachado a ruta. Muelles habilitados listos para carga.";
        DashboardUI.Instance?.RegistrarLog(msgConvoy);
        Debug.Log(msgConvoy);
        OnMuellesDisponiblesNuevamente?.Invoke();
    }

    private bool AlgunAMREnMuelle(int numeroCamion)
    {
        Transform muelle = ObtenerTransformMuelle(numeroCamion);
        if (muelle == null) return false;

        float xMuelle = muelle.position.x;
        RobotAMR[] flota = Object.FindObjectsByType<RobotAMR>(FindObjectsSortMode.None);
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR amr = flota[i];
            if (amr == null || !amr.gameObject.activeInHierarchy) continue;

            if (amr.transform.position.z > -2.0f && Mathf.Abs(amr.transform.position.x - xMuelle) < 4.5f)
            {
                return true;
            }
        }
        return false;
    }
}
