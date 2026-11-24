using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Content_Filas dentro del ScrollRect
    [SerializeField] private GameObject filaPrefab;     // Prefab de fila operativa
    [SerializeField] private ScrollRect scrollManifiesto;

    private GameObject filaResumenGO;
    private TextMeshProUGUI[] columnasResumenLote;
    private int contadorCorrelativoPallet = 8491;
    private int filasOperativasInstanciadas = 0;

    private void ResolverContenedorYScroll()
    {
        if (contenedorFilas == null || contenedorFilas == transform || contenedorFilas.name == "Manifiesto")
        {
            Transform content = transform.Find("Scroll_Manifiesto/Viewport/Content_Filas");
            if (content != null)
            {
                contenedorFilas = content;
            }
            else if (contenedorFilas == null)
            {
                contenedorFilas = transform;
            }
        }

        if (scrollManifiesto == null)
        {
            scrollManifiesto = GetComponentInChildren<ScrollRect>(true);
        }
    }

    public void LimpiarTablaStandby()
    {
        ResolverContenedorYScroll();

        for (int i = contenedorFilas.childCount - 1; i >= 0; i--)
        {
            Transform child = contenedorFilas.GetChild(i);
            if (child.name == "Header_Manifiesto" || child.name == "Fila_Titulos")
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        filaResumenGO = null;
        columnasResumenLote = null;
        contadorCorrelativoPallet = 8491;
        filasOperativasInstanciadas = 0;

        if (DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
        {
            DashboardUI.Instance.textosEstadoManifiesto.Clear();
        }
    }

    // Mantiene compatibilidad sin destruir filas previas durante el turno
    public void PoblarTabla(List<Pedido> pedidosJSON, int totalPalletsLote = 0)
    {
        int nuevosPallets = totalPalletsLote > 0 ? totalPalletsLote : (pedidosJSON != null ? pedidosJSON.Count : 0);
        AgregarLoteAManifiesto(pedidosJSON, nuevosPallets, nuevosPallets);
    }

    /// <summary>
    /// Agrega un lote nuevo de forma acumulativa en orden FIFO sin destruir el historial de pedidos anteriores.
    /// </summary>
    public void AgregarLoteAManifiesto(List<Pedido> plantillaBase, int nuevosPallets, int totalPalletsEnColaWMS)
    {
        ResolverContenedorYScroll();
        if (filaPrefab == null || nuevosPallets <= 0) return;

        int filasAAgregar = Mathf.Min(3, nuevosPallets);
        for (int i = 0; i < filasAAgregar; i++)
        {
            Pedido plantilla = (plantillaBase != null && plantillaBase.Count > 0)
                ? plantillaBase[i % plantillaBase.Count]
                : null;
            InstanciarFilaOperativa(plantilla);
        }

        ActualizarResumenColaEnVivo(totalPalletsEnColaWMS);
        ForzarLayoutYScroll(indiceObjetivo: Mathf.Max(0, filasOperativasInstanciadas - 1));
    }

    /// <summary>
    /// Garantiza que exista una fila operativa instantiated para el índice secuencial de misión y enfoca el scroll en ella.
    /// </summary>
    public int AsegurarFilaOperativaParaMision(int indiceSecuencial, Pedido plantilla, int palletsRestantesEnCola)
    {
        ResolverContenedorYScroll();
        if (DashboardUI.Instance == null || DashboardUI.Instance.textosEstadoManifiesto == null)
        {
            return indiceSecuencial;
        }

        int guard = 0;
        while (indiceSecuencial >= DashboardUI.Instance.textosEstadoManifiesto.Count && guard++ < 8)
        {
            InstanciarFilaOperativa(plantilla);
        }

        ActualizarPesoFilaOperativa(indiceSecuencial, plantilla);
        ActualizarResumenColaEnVivo(palletsRestantesEnCola);
        ForzarLayoutYScroll(indiceObjetivo: indiceSecuencial);
        return indiceSecuencial;
    }

    private void ActualizarPesoFilaOperativa(int indiceSecuencial, Pedido plantilla)
    {
        if (contenedorFilas == null || plantilla == null || plantilla.peso_kg <= 0f || indiceSecuencial < 0)
        {
            return;
        }

        int indiceFilaDato = 0;
        for (int i = 0; i < contenedorFilas.childCount; i++)
        {
            Transform child = contenedorFilas.GetChild(i);
            if (child == null) continue;
            if (child.name == "Header_Manifiesto" || child.name == "Fila_Titulos" || child.name == "Fila_Resumen_Lote" || child.gameObject == filaResumenGO)
            {
                continue;
            }

            if (indiceFilaDato == indiceSecuencial)
            {
                TextMeshProUGUI[] cols = child.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (cols != null && cols.Length >= 5 && cols[3] != null)
                {
                    cols[3].text = plantilla.peso_kg.ToString("F0") + " kg";
                }
                return;
            }

            indiceFilaDato++;
        }
    }

    private void InstanciarFilaOperativa(Pedido plantilla)
    {
        if (filaPrefab == null || contenedorFilas == null) return;

        int numeroPallet = contadorCorrelativoPallet++;
        GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);
        nuevaFila.name = $"Fila_PAL_{numeroPallet}";
        AsegurarAlturaFila(nuevaFila);

        TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();
        if (columnas.Length >= 5)
        {
            columnas[0].text = $"PAL-{numeroPallet}";
            columnas[1].text = plantilla != null && !string.IsNullOrEmpty(plantilla.nombre_producto) ? plantilla.nombre_producto : "Carga Industrial";
            columnas[2].text = plantilla != null && !string.IsNullOrEmpty(plantilla.codigo_sku) ? plantilla.codigo_sku : "SKU-STD";
            float peso = plantilla != null && plantilla.peso_kg > 0f ? plantilla.peso_kg : Pedido.PesoReferenciaBaseKg;
            columnas[3].text = peso.ToString("F0") + " kg";

            for (int c = 0; c < 4; c++)
            {
                if (columnas[c] == null) continue;
                columnas[c].color = Color.white;
                columnas[c].fontSize = 11f;
            }

            columnas[0].alignment = TextAlignmentOptions.MidlineLeft;
            columnas[1].alignment = TextAlignmentOptions.MidlineLeft;
            columnas[1].overflowMode = TextOverflowModes.Ellipsis;
            columnas[2].alignment = TextAlignmentOptions.Center;
            columnas[3].alignment = TextAlignmentOptions.MidlineRight;

            columnas[4].text = "PENDIENTE";
            columnas[4].color = new Color(0.94f, 0.27f, 0.27f, 1f);
            columnas[4].fontSize = 11f;
            columnas[4].alignment = TextAlignmentOptions.Center;

            if (DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
            {
                DashboardUI.Instance.textosEstadoManifiesto.Add(columnas[4]);
            }
        }

        filasOperativasInstanciadas++;
        if (filaResumenGO != null)
        {
            filaResumenGO.transform.SetAsLastSibling();
        }
        RenderizarVista();
    }

    private void AsegurarAlturaFila(GameObject fila)
    {
        if (fila == null) return;
        LayoutElement le = fila.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = fila.AddComponent<LayoutElement>();
            RectTransform rt = fila.GetComponent<RectTransform>();
            float alto = rt != null && rt.sizeDelta.y > 10f ? rt.sizeDelta.y : 44f;
            le.minHeight = alto;
            le.preferredHeight = alto;
            le.flexibleHeight = 0f;
        }
    }

    private int ContarFilasPendientesOEnTransito()
    {
        if (DashboardUI.Instance == null || DashboardUI.Instance.textosEstadoManifiesto == null) return 0;

        int activas = 0;
        var lista = DashboardUI.Instance.textosEstadoManifiesto;
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] == null) continue;
            string estado = lista[i].text;
            if (estado == "PENDIENTE" || estado == "EN TRÁNSITO")
            {
                activas++;
            }
        }
        return activas;
    }

    public void ActualizarResumenColaEnVivo(int palletsRestantesEnCola)
    {
        ResolverContenedorYScroll();
        int sinInstanciar = Mathf.Max(0, palletsRestantesEnCola - ContarFilasPendientesOEnTransito());

        if (sinInstanciar > 0)
        {
            if (filaResumenGO == null && filaPrefab != null && contenedorFilas != null)
            {
                filaResumenGO = Instantiate(filaPrefab, contenedorFilas);
                filaResumenGO.name = "Fila_Resumen_Lote";
                AsegurarAlturaFila(filaResumenGO);
                columnasResumenLote = filaResumenGO.GetComponentsInChildren<TextMeshProUGUI>();
            }

            if (filaResumenGO != null)
            {
                filaResumenGO.SetActive(true);
                filaResumenGO.transform.SetAsLastSibling();
            }

            if (columnasResumenLote != null && columnasResumenLote.Length >= 5)
            {
                columnasResumenLote[0].text = $"+{sinInstanciar} EN COLA WMS";
                columnasResumenLote[1].text = "Lote ERP";
                columnasResumenLote[2].text = "CONSOLIDADO";
                columnasResumenLote[3].text = $"{sinInstanciar * Pedido.PesoReferenciaBaseKg:F0} kg";
                for (int c = 0; c < 4; c++)
                {
                    if (columnasResumenLote[c] == null) continue;
                    columnasResumenLote[c].color = Color.white;
                    columnasResumenLote[c].fontSize = 11f;
                }
                columnasResumenLote[0].alignment = TextAlignmentOptions.MidlineLeft;
                columnasResumenLote[1].alignment = TextAlignmentOptions.MidlineLeft;
                columnasResumenLote[1].overflowMode = TextOverflowModes.Ellipsis;
                columnasResumenLote[2].alignment = TextAlignmentOptions.Center;
                columnasResumenLote[3].alignment = TextAlignmentOptions.MidlineRight;
                columnasResumenLote[4].text = "COLA WMS";
                columnasResumenLote[4].color = new Color(0.22f, 0.74f, 0.97f, 1f);
                columnasResumenLote[4].fontSize = 11f;
                columnasResumenLote[4].alignment = TextAlignmentOptions.Center;
            }
        }
        else if (filaResumenGO != null)
        {
            filaResumenGO.SetActive(false);
        }

        RenderizarVista();
    }

    public void RenderizarVista()
    {
        ResolverContenedorYScroll();
        if (contenedorFilas == null) return;

        int indiceFilaDato = 0;
        for (int i = 0; i < contenedorFilas.childCount; i++)
        {
            Transform child = contenedorFilas.GetChild(i);
            if (child == null || !child.gameObject.activeSelf) continue;
            if (child.name == "Header_Manifiesto" || child.name == "Fila_Titulos") continue;

            Image fondo = child.GetComponent<Image>();
            if (fondo == null)
            {
                fondo = child.gameObject.AddComponent<Image>();
                fondo.raycastTarget = false;
            }

            if (child.gameObject == filaResumenGO || child.name == "Fila_Resumen_Lote")
            {
                fondo.color = new Color(0.07f, 0.10f, 0.16f, 0.92f);
                continue;
            }

            TextMeshProUGUI[] cols = child.GetComponentsInChildren<TextMeshProUGUI>(true);
            string estado = (cols != null && cols.Length >= 5 && cols[4] != null) ? cols[4].text : string.Empty;
            bool enEjecucion = estado == "EN TRÁNSITO" ||
                               estado == "TRANSPORTANDO" ||
                               estado == "EXTRAYENDO" ||
                               estado == "ENTREGANDO";

            if (enEjecucion)
            {
                fondo.color = new Color(0.08f, 0.26f, 0.22f, 0.88f);
            }
            else if (indiceFilaDato % 2 == 0)
            {
                fondo.color = new Color(0.10f, 0.14f, 0.21f, 0.78f);
            }
            else
            {
                fondo.color = new Color(0.14f, 0.19f, 0.27f, 0.78f);
            }

            indiceFilaDato++;
        }
    }

    private void ForzarLayoutYScroll(int indiceObjetivo)
    {
        Canvas.ForceUpdateCanvases();
        if (contenedorFilas is RectTransform contentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (scrollManifiesto != null && DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
        {
            int total = Mathf.Max(1, DashboardUI.Instance.textosEstadoManifiesto.Count);
            if (total <= 1)
            {
                scrollManifiesto.verticalNormalizedPosition = 1f;
            }
            else
            {
                float norm = 1f - Mathf.Clamp01((float)indiceObjetivo / (total - 1));
                scrollManifiesto.verticalNormalizedPosition = norm;
            }
        }
    }
}