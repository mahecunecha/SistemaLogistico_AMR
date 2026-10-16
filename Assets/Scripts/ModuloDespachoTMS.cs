using System.Collections.Generic;
using UnityEngine;

public class ModuloDespachoTMS : MonoBehaviour
{
    public static ModuloDespachoTMS Instance { get; private set; }

    [Header("Bahías de Carga / Muelles (Sector Este)")]
    public Transform muelleCamion01;
    public Transform muelleCamion02;
    public Transform muelleCamion03;

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

    public Stack<Pedido> MatrizLIFOCamion1 => matrizLIFOCamion1;
    public Stack<Pedido> MatrizLIFOCamion2 => matrizLIFOCamion2;
    public Stack<Pedido> MatrizLIFOCamion3 => matrizLIFOCamion3;

    public float PesoCamion1 => pesoAcumuladoCamion1;
    public float PesoCamion2 => pesoAcumuladoCamion2;
    public float PesoCamion3 => pesoAcumuladoCamion3;

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
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Transform ConsultarMuelleDestino(Pedido pedido, out int numeroCamion, out int slotLIFOProyectado)
    {
        numeroCamion = 1;
        if (pedido != null)
        {
            // Regla de ruteo TMS Módulo 5:
            // Pedido 1 (AGU-RUB-330): Camión 1 (Ruta Nacional)
            // Pedido 2 (CLU-NEG-330): Camión 2 (Ruta Metropolitana)
            // Pedido 3 (OKT-BAR-50L, 920 kg): Camión 3 (Ruta Express / Carga Pesada)
            if (pedido.codigo_sku == "AGU-RUB-330" || pedido.id_pallet == "PAL-8491")
            {
                numeroCamion = 1;
            }
            else if (pedido.codigo_sku == "CLU-NEG-330" || pedido.id_pallet == "PAL-8492")
            {
                numeroCamion = 2;
            }
            else if (pedido.codigo_sku == "OKT-BAR-50L" || pedido.id_pallet == "PAL-8493" || pedido.peso_kg >= 900f)
            {
                numeroCamion = 3;
            }
        }

        switch (numeroCamion)
        {
            case 1:
                slotLIFOProyectado = matrizLIFOCamion1.Count + 1;
                break;
            case 2:
                slotLIFOProyectado = matrizLIFOCamion2.Count + 1;
                break;
            case 3:
                slotLIFOProyectado = matrizLIFOCamion3.Count + 1;
                break;
            default:
                slotLIFOProyectado = 1;
                break;
        }

        string idPallet = pedido != null ? pedido.id_pallet : "PAL-DESC";
        string logMensaje = $"<color=#38BDF8>[API MÓDULO 5 -> WMS]:</color> Ruta GPS validada. Asignando {idPallet} a Muelle_Camion_0{numeroCamion}";
        DashboardUI.Instance?.RegistrarLog(logMensaje);
        Debug.Log(logMensaje);

        return ObtenerTransformMuelle(numeroCamion);
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

    public void RegistrarEstibaEnCamion(int numeroCamion, Pedido pedido, GameObject palletFisico)
    {
        if (pedido == null) return;

        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        Stack<Pedido> pilaObjetivo;
        List<GameObject> listaPallets;
        float pesoAcumulado;

        switch (numeroCamion)
        {
            case 1:
                matrizLIFOCamion1.Push(pedido);
                pesoAcumuladoCamion1 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion1;
                listaPallets = palletsCamion1;
                pesoAcumulado = pesoAcumuladoCamion1;
                break;
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

            int indiceLIFO = pilaObjetivo.Count;
            float zOffset = (indiceLIFO - 2) * 1.6f;
            float ySuperficie = 3.23f; // Superficie superior exacta del camión

            Vector3 posicionLIFO = new Vector3(
                muelleTransform.position.x,
                ySuperficie,
                muelleTransform.position.z + zOffset
            );

            palletFisico.transform.position = posicionLIFO;
            palletFisico.transform.rotation = muelleTransform.rotation;
        }

        string logMatriz = $"<color=cyan>[MATRIZ LIFO CAMIÓN {numeroCamion}]:</color> Estiba {pedido.id_pallet} apilada en Posición #{pilaObjetivo.Count} (Peso acumulado: {pesoAcumulado:F0} kg / {CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMatriz);
        Debug.Log(logMatriz);
    }
}
