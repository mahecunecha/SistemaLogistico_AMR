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

    [Header("Compuertas Traseras (Sellado 3/3)")]
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
    public const int CapacidadMaximaPalletsCamion = 3;
    private const float EspaciadoSlotLIFOMetros = 1.60f;

    // Índices 1..3 correspondientes a Muelle_Camion_01..03
    private readonly int[] slotsReservados = new int[4];
    private readonly bool[] renovacionEnCurso = new bool[4];
    private readonly bool[] camionSellado = new bool[4];
    private bool reinicioConvoyEnCurso = false;
    private int camionTurnoActual = 1;

    [Header("Protocolo de Inspección Humana en Muelle")]
    public float duracionInspeccionHumana = 45f;
    private readonly bool[] inspeccionHumanaEnCurso = new bool[4];
    private readonly Coroutine[] rutinasInspeccionHumana = new Coroutine[4];

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

    public static float CalcularZOffsetLIFO(int slotLIFO)
    {
        int slotClamped = Mathf.Clamp(slotLIFO, 1, CapacidadMaximaPalletsCamion);
        return (2 - slotClamped) * EspaciadoSlotLIFOMetros;
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
            renovacionEnCurso[i] = false;
            camionSellado[i] = false;
            inspeccionHumanaEnCurso[i] = false;
            rutinasInspeccionHumana[i] = null;
        }
        reinicioConvoyEnCurso = false;
        camionTurnoActual = 1;
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
        float pesoPedido = pedido != null ? pedido.peso_kg : 0f;

        // Rotación secuencial 1 -> 2 -> 3 respetando máximo 3 pallets (físicos o reservados), tonelaje máximo, sellado e inspección humana
        for (int offset = 0; offset < 3; offset++)
        {
            int candidato = ((camionTurnoActual - 1 + offset) % 3) + 1;
            int ocupados = Mathf.Max(slotsReservados[candidato], ObtenerListaPallets(candidato).Count);
            bool dentroCapacidadPeso = ObtenerPesoCamion(candidato) + pesoPedido <= CapacidadMaximaCamionKg;
            if (!renovacionEnCurso[candidato] && !camionSellado[candidato] && !inspeccionHumanaEnCurso[candidato] &&
                ocupados < CapacidadMaximaPalletsCamion && dentroCapacidadPeso)
            {
                numeroCamion = candidato;
                break;
            }
        }

        if (numeroCamion == 0)
        {
            // Fallback seguro si todos estuvieran momentáneamente al límite 3/3 en simultáneo
            numeroCamion = ((camionTurnoActual - 1) % 3) + 1;
        }

        int baseOcupados = Mathf.Max(slotsReservados[numeroCamion], ObtenerListaPallets(numeroCamion).Count);
        int nuevaReserva = Mathf.Clamp(baseOcupados + 1, 1, CapacidadMaximaPalletsCamion);
        slotsReservados[numeroCamion] = nuevaReserva;
        slotLIFOProyectado = nuevaReserva;

        if (slotsReservados[numeroCamion] >= CapacidadMaximaPalletsCamion)
        {
            camionTurnoActual = (numeroCamion % 3) + 1;
        }
        else
        {
            camionTurnoActual = numeroCamion;
        }

        string idPallet = pedido != null ? pedido.id_pallet : "PAL-DESC";
        string logMensaje = $"<color=#38BDF8>[API MÓDULO 5 -> WMS]:</color> Ruta GPS validada. Asignando {idPallet} a Muelle_Camion_0{numeroCamion} (Slot LIFO #{slotLIFOProyectado}/{CapacidadMaximaPalletsCamion})";
        DashboardUI.Instance?.RegistrarLog(logMensaje);
        Debug.Log(logMensaje);

        return ObtenerTransformMuelle(numeroCamion);
    }

    public int ReportarInspeccionHumanaMuelle(int numeroCamionBloqueado, out int siguienteCamionOperativo)
    {
        numeroCamionBloqueado = Mathf.Clamp(numeroCamionBloqueado, 1, 3);
        inspeccionHumanaEnCurso[numeroCamionBloqueado] = true;

        if (Application.isPlaying)
        {
            if (rutinasInspeccionHumana[numeroCamionBloqueado] != null)
            {
                StopCoroutine(rutinasInspeccionHumana[numeroCamionBloqueado]);
            }
            rutinasInspeccionHumana[numeroCamionBloqueado] = StartCoroutine(AutoLiberarInspeccionHumana(numeroCamionBloqueado));
        }

        siguienteCamionOperativo = 0;

        for (int offset = 1; offset <= 2; offset++)
        {
            int candidato = ((numeroCamionBloqueado - 1 + offset) % 3) + 1;
            int ocupados = Mathf.Max(slotsReservados[candidato], ObtenerListaPallets(candidato).Count);
            if (!renovacionEnCurso[candidato] && !camionSellado[candidato] && !inspeccionHumanaEnCurso[candidato] && ocupados < CapacidadMaximaPalletsCamion)
            {
                siguienteCamionOperativo = candidato;
                break;
            }
        }

        if (siguienteCamionOperativo == 0)
        {
            for (int offset = 1; offset <= 2; offset++)
            {
                int candidato = ((numeroCamionBloqueado - 1 + offset) % 3) + 1;
                if (!inspeccionHumanaEnCurso[candidato])
                {
                    siguienteCamionOperativo = candidato;
                    break;
                }
            }
        }

        if (siguienteCamionOperativo == 0)
        {
            siguienteCamionOperativo = (numeroCamionBloqueado % 3) + 1;
        }

        camionTurnoActual = siguienteCamionOperativo;
        return siguienteCamionOperativo;
    }

    private IEnumerator AutoLiberarInspeccionHumana(int numeroCamion)
    {
        yield return new WaitForSeconds(Mathf.Max(1f, duracionInspeccionHumana));
        inspeccionHumanaEnCurso[numeroCamion] = false;
        rutinasInspeccionHumana[numeroCamion] = null;

        string msgDespeje = $"<color=#10B981>[TMS - INSPECCIÓN HUMANA]:</color> Operario en piso despejó Muelle_Camion_0{numeroCamion}. Bahía habilitada nuevamente.";
        DashboardUI.Instance?.RegistrarLog(msgDespeje);
        Debug.Log(msgDespeje);
    }

    public void CancelarReservaSlot(int numeroCamion, int slotReservado)
    {
        if (numeroCamion < 1 || numeroCamion > 3 || slotReservado <= 0) return;

        int entregados = ObtenerListaPallets(numeroCamion).Count;
        slotsReservados[numeroCamion] = Mathf.Max(entregados, slotsReservados[numeroCamion] - 1);
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
        return Mathf.Clamp(Mathf.Max(ObtenerListaPallets(numeroCamion).Count, slotsReservados[numeroCamion]) + 1, 1, CapacidadMaximaPalletsCamion);
    }

    public Vector3 ObtenerCoordenadaSlotLIFO(int numeroCamion, int slotLIFOProyectado)
    {
        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        if (muelleTransform == null) return Vector3.zero;

        // Inversión LIFO física (Flatbed):
        // Slot 1 al fondo (Z = +1.60m), Slot 2 al centro (Z = 0m), Slot 3 cerca de rampa (Z = -1.60m)
        float zOffset = CalcularZOffsetLIFO(slotLIFOProyectado);

        Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
        float ySuperficie = 1.813f;
        float zCentroCarga = muelleTransform.position.z;
        float xCentroCarga = muelleTransform.position.x;

        if (sueloFalso != null)
        {
            Collider colSuelo = sueloFalso.GetComponent<Collider>();
            ySuperficie = colSuelo != null ? colSuelo.bounds.max.y : (sueloFalso.position.y + 0.066f);
            zCentroCarga = sueloFalso.position.z;
            xCentroCarga = sueloFalso.position.x;
        }

        return new Vector3(xCentroCarga, ySuperficie, zCentroCarga + zOffset);
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

        if (palletFisico != null && muelleTransform != null)
        {
            listaPallets.Add(palletFisico);
            palletFisico.transform.SetParent(null, true);

            int indiceLIFO = slotAsignado > 0
                ? Mathf.Clamp(slotAsignado, 1, CapacidadMaximaPalletsCamion)
                : Mathf.Clamp(pilaObjetivo.Count, 1, CapacidadMaximaPalletsCamion);
            float zOffset = CalcularZOffsetLIFO(indiceLIFO);

            Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
            float ySuperficie = 1.813f;
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
                xCentroCarga,
                ySuperficie,
                zCentroCarga + zOffset
            );

            palletFisico.transform.position = posicionLIFO;
            palletFisico.transform.rotation = muelleTransform.rotation;
        }

        int estibasActuales = Mathf.Max(listaPallets.Count, pilaObjetivo.Count);
        slotsReservados[numeroCamion] = Mathf.Max(slotsReservados[numeroCamion], estibasActuales);

        int slotRegistrado = slotAsignado > 0
            ? Mathf.Clamp(slotAsignado, 1, CapacidadMaximaPalletsCamion)
            : Mathf.Clamp(estibasActuales, 1, CapacidadMaximaPalletsCamion);

        historialEstibasTurno.Add(new RegistroEstibaTurno
        {
            Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            NumeroCamion = numeroCamion,
            SlotLIFO = slotRegistrado,
            IdPallet = !string.IsNullOrEmpty(pedido.id_pallet) ? pedido.id_pallet : "PAL-DESC",
            Producto = !string.IsNullOrEmpty(pedido.nombre_producto) ? pedido.nombre_producto : "Carga Industrial",
            SKU = !string.IsNullOrEmpty(pedido.codigo_sku) ? pedido.codigo_sku : "SKU-STD",
            PesoKg = pedido.peso_kg
        });

        string logMatriz = $"<color=cyan>[MATRIZ LIFO CAMIÓN {numeroCamion}]:</color> Estiba {pedido.id_pallet} apilada en Posición #{estibasActuales}/{CapacidadMaximaPalletsCamion} (Peso acumulado: {pesoAcumulado:F0} kg / {CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMatriz);
        Debug.Log(logMatriz);
        GestorAudioSCADA.Instance?.ReproducirConfirmacionEntrega();

        if (estibasActuales >= CapacidadMaximaPalletsCamion && !renovacionEnCurso[numeroCamion] && !camionSellado[numeroCamion])
        {
            slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
            if (Application.isPlaying)
            {
                StartCoroutine(SellarCamionYVerificarConvoy(numeroCamion, 2.5f));
            }
        }
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

        // 2. Destruir los 3 pallets internos tras sellar para optimizar GPU/RAM, manteniendo slotsReservados en 3
        List<GameObject> listaPallets = ObtenerListaPallets(numeroCamion);
        LimpiarListaPalletsCamion(listaPallets);
        slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
        renovacionEnCurso[numeroCamion] = false;

        string msgSellado = $"<color=#10B981>[TMS MÓDULO 5]:</color> Muelle_Camion_0{numeroCamion} completo (3/3 estibas LIFO). Compuertas selladas; habilitando siguiente bahía.";
        DashboardUI.Instance?.RegistrarLog(msgSellado);
        Debug.Log(msgSellado);

        // 3. Si los 3 camiones ya quedaron llenos y sellados, despachar el convoy completo tras 4.0s
        if (camionSellado[1] && camionSellado[2] && camionSellado[3] && !reinicioConvoyEnCurso)
        {
            reinicioConvoyEnCurso = true;
            yield return new WaitForSeconds(4.0f);

            AbrirTodasLasCompuertas();

            matrizLIFOCamion1.Clear();
            matrizLIFOCamion2.Clear();
            matrizLIFOCamion3.Clear();
            pesoAcumuladoCamion1 = 0f;
            pesoAcumuladoCamion2 = 0f;
            pesoAcumuladoCamion3 = 0f;

            for (int i = 1; i <= 3; i++)
            {
                slotsReservados[i] = 0;
                renovacionEnCurso[i] = false;
                camionSellado[i] = false;
            }
            camionTurnoActual = 1;
            reinicioConvoyEnCurso = false;

            string msgConvoy = "<color=#38BDF8>[TMS MÓDULO 5]:</color> Convoy de 3 camiones despachado a ruta. 3 nuevos remolques vacíos posicionados en Muelles 01, 02 y 03.";
            DashboardUI.Instance?.RegistrarLog(msgConvoy);
            Debug.Log(msgConvoy);
        }
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
            if (amr.estadoActual == RobotAMR.EstadoRobot.Apagado) continue;

            if (amr.transform.position.z > -2.0f && Mathf.Abs(amr.transform.position.x - xMuelle) < 4.5f)
            {
                return true;
            }
        }
        return false;
    }
}
