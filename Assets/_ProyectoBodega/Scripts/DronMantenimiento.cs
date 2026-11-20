using System;
using System.Collections;
using UnityEngine;

public class DronMantenimiento : MonoBehaviour
{
    [Header("Topografía Base Aérea")]
    public Vector3 posicionBaseTecho = new Vector3(-32.0f, 10.0f, -38.0f);
    public Quaternion rotacionBase = Quaternion.identity;

    [Header("Parámetros de Intervención")]
    public float alturaAcopleSobreObstaculo = 1.76f;

    [Header("Envolvente Espacial y Doble Radio (ISO 3691-4)")]
    public float radioEnvolventeVacio = 1.35f;
    public float radioEnvolventeConCarga = 1.65f;
    public float offsetCargaY = -0.60f;
    public float cotaGaliboSeguroY = 3.00f;
    public LayerMask capaColisionAerea = 1 << 0; // Default (Tráilers, Estanterías AS/RS, Estructura)
    public bool tieneCargaAcoplada = false;

    [Header("Chupa Neumática Industrial Articulada")]
    public Transform chupaRaiz;
    public Transform bridaSuperior;
    public Transform cilindroCamisa;
    public Transform vastagoTelescopico;
    public Transform campanaVentosa;
    public Transform anilloLedVacio;
    public MeshRenderer rendererAnilloLed;
    public Material matLedRojoReposo;
    public Material matLedVerdeSuccion;

    [Header("Componentes Visuales")]
    public DronePropellers controlHelices;

    [Header("Estado Operativo")]
    public bool ocupado = false;

    void Awake()
    {
        if (controlHelices == null)
        {
            controlHelices = GetComponentInChildren<DronePropellers>();
        }

        ResolverReferenciasChupa();

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
            controlHelices.rotationAxis = new Vector3(0f, 0f, 1f);
        }

        tieneCargaAcoplada = false;
        AplicarExtensionChupa(0.20f);
        ConfigurarEstadoLedSuccion(false);
    }

    private void ResolverReferenciasChupa()
    {
        if (chupaRaiz == null)
        {
            chupaRaiz = transform.Find("Chupa_Neumatica_Industrial");
        }

        if (chupaRaiz != null)
        {
            if (bridaSuperior == null) bridaSuperior = chupaRaiz.Find("Brida_Superior");
            if (cilindroCamisa == null) cilindroCamisa = chupaRaiz.Find("Cilindro_Camisa");
            if (vastagoTelescopico == null) vastagoTelescopico = chupaRaiz.Find("Vastago_Telescopico");
            if (campanaVentosa == null) campanaVentosa = chupaRaiz.Find("Campana_Ventosa");
            if (anilloLedVacio == null && campanaVentosa != null)
            {
                anilloLedVacio = campanaVentosa.Find("Anillo_LED_Vacio");
            }
            if (anilloLedVacio != null)
            {
                anilloLedVacio.localPosition = Vector3.zero;
            }
            if (rendererAnilloLed == null && anilloLedVacio != null)
            {
                rendererAnilloLed = anilloLedVacio.GetComponent<MeshRenderer>();
            }
        }
    }

    public void ConfigurarEstadoLedSuccion(bool succionActiva)
    {
        ResolverReferenciasChupa();
        if (rendererAnilloLed == null) return;

        Material matObjetivo = succionActiva ? matLedVerdeSuccion : matLedRojoReposo;
        if (matObjetivo != null)
        {
            rendererAnilloLed.sharedMaterial = matObjetivo;
        }
    }

    public void AplicarExtensionChupa(float longitudVastago)
    {
        ResolverReferenciasChupa();
        float scaleY = Mathf.Clamp(longitudVastago, 0.20f, 1.40f) * 0.5f;

        if (vastagoTelescopico != null)
        {
            vastagoTelescopico.localScale = new Vector3(0.075f, scaleY, 0.075f);
            vastagoTelescopico.localPosition = new Vector3(0f, -0.18f - scaleY, 0f);
        }

        if (campanaVentosa != null)
        {
            campanaVentosa.localScale = new Vector3(0.34f, 0.035f, 0.34f);
            campanaVentosa.localPosition = new Vector3(0f, -0.18f - (2f * scaleY) - 0.035f, 0f);
        }
    }

    public void PosicionarChupaPorCotaBaseMundo(float yBaseInferiorCampanaMundo)
    {
        ResolverReferenciasChupa();

        float semiAltoCampana = campanaVentosa != null
            ? Mathf.Max(0.01f, Mathf.Abs(campanaVentosa.lossyScale.y))
            : 0.035f;
        float yChupaRaiz = chupaRaiz != null ? chupaRaiz.position.y : (transform.position.y - 0.02f);
        float yAnclajeSuperior = cilindroCamisa != null ? cilindroCamisa.position.y : (yChupaRaiz - 0.18f);

        float yCentroCampana = yBaseInferiorCampanaMundo + semiAltoCampana;
        float yTopeCampana = yBaseInferiorCampanaMundo + (2f * semiAltoCampana);

        if (campanaVentosa != null)
        {
            campanaVentosa.position = new Vector3(transform.position.x, yCentroCampana, transform.position.z);
        }

        float longitudVastago = Mathf.Max(0.08f, yAnclajeSuperior - yTopeCampana);
        float scaleYVastago = longitudVastago * 0.5f;
        if (vastagoTelescopico != null)
        {
            vastagoTelescopico.localScale = new Vector3(0.075f, scaleYVastago, 0.075f);
            vastagoTelescopico.position = new Vector3(transform.position.x, yAnclajeSuperior - scaleYVastago, transform.position.z);
        }
    }

    private void SincronizarChupaYCarga(float yBaseCampanaMundo, GameObject carga, float alturaVisualSobrePivote)
    {
        PosicionarChupaPorCotaBaseMundo(yBaseCampanaMundo);
        if (carga != null)
        {
            carga.transform.position = new Vector3(
                transform.position.x,
                yBaseCampanaMundo - alturaVisualSobrePivote,
                transform.position.z
            );
        }
    }

    private static bool EsTransformDeHUD(Transform t)
    {
        Transform actual = t;
        while (actual != null)
        {
            if (actual.GetComponent<Canvas>() != null || actual.name.Contains("HUD"))
            {
                return true;
            }
            actual = actual.parent;
        }
        return false;
    }

    /// <summary>
    /// Devuelve la cota Y superior real de la malla visible del objetivo (ignorando HUDs y partículas).
    /// Solo recurre a colliders activos si no hubiera ningún Renderer visible.
    /// </summary>
    public static float CalcularTopeSuperiorVisual(GameObject objetivo)
    {
        if (objetivo == null) return 0f;

        bool encontradoVisual = false;
        float maxYVisual = float.MinValue;

        Renderer[] renderers = objetivo.GetComponentsInChildren<Renderer>(false);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || !r.enabled || r is ParticleSystemRenderer) continue;
            if (EsTransformDeHUD(r.transform)) continue;

            if (!encontradoVisual || r.bounds.max.y > maxYVisual)
            {
                maxYVisual = r.bounds.max.y;
                encontradoVisual = true;
            }
        }

        if (encontradoVisual)
        {
            return maxYVisual;
        }

        bool encontradoCollider = false;
        float maxYCollider = float.MinValue;
        Collider[] colliders = objetivo.GetComponentsInChildren<Collider>(false);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null || !col.enabled || col.isTrigger) continue;
            if (EsTransformDeHUD(col.transform)) continue;

            if (!encontradoCollider || col.bounds.max.y > maxYCollider)
            {
                maxYCollider = col.bounds.max.y;
                encontradoCollider = true;
            }
        }

        return encontradoCollider ? maxYCollider : (objetivo.transform.position.y + 0.32f);
    }

    public static float CalcularTopeSuperiorObjetivo(GameObject objetivo)
    {
        return CalcularTopeSuperiorVisual(objetivo);
    }

    private bool EsColliderIgnorable(Collider col, GameObject objetivoIgnorado)
    {
        if (col == null || !col.enabled || col.isTrigger) return true;
        if (col.transform == transform || col.transform.IsChildOf(transform)) return true;

        int capa = col.gameObject.layer;
        if (capa == 6 || capa == 7 || capa == 8) return true;

        if (objetivoIgnorado != null && (col.gameObject == objetivoIgnorado || col.transform.IsChildOf(objetivoIgnorado.transform)))
        {
            return true;
        }
        if (col.bounds.max.y <= 0.45f || col.name.Contains("Suelo") || col.name.Contains("Floor"))
        {
            return true;
        }
        return false;
    }

    public bool ValidarMovimientoAereoSeguro(Vector3 origen, Vector3 destino, bool conCarga, GameObject objetivoIgnorado, out float altitudCruceroSegura)
    {
        int mascaraSegura = capaColisionAerea.value & ~((1 << 6) | (1 << 7) | (1 << 8));
        altitudCruceroSegura = Mathf.Max(10.0f, posicionBaseTecho.y);

        bool esDescenso = destino.y < 6.5f;
        if (esDescenso && destino.z > -2.64f)
        {
            return false;
        }

        float radioEvaluacion = esDescenso ? 0.55f : (conCarga ? radioEnvolventeConCarga : radioEnvolventeVacio);
        Vector3 offset = (!esDescenso && conCarga) ? new Vector3(0f, offsetCargaY, 0f) : Vector3.zero;
        Vector3 pOrigen = origen + offset;
        float yDestinoEval = esDescenso ? Mathf.Max(destino.y, 1.65f) : destino.y;
        Vector3 pDestino = new Vector3(destino.x, yDestinoEval, destino.z) + offset;

        if (esDescenso && pDestino.y > pOrigen.y)
        {
            pDestino.y = pOrigen.y;
        }

        Vector3 delta = pDestino - pOrigen;
        float dist = delta.magnitude;
        bool colisionDetectada = false;

        if (dist > 0.001f)
        {
            RaycastHit[] hits = Physics.SphereCastAll(pOrigen, radioEvaluacion, delta / dist, dist, mascaraSegura, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i].collider;
                if (EsColliderIgnorable(col, objetivoIgnorado)) continue;

                colisionDetectada = true;
                float margenInferior = (!esDescenso && conCarga) ? -offsetCargaY : 0f;
                altitudCruceroSegura = Mathf.Max(altitudCruceroSegura, col.bounds.max.y + margenInferior + radioEvaluacion + 0.50f);
            }
        }

        Collider[] solapados = Physics.OverlapSphere(pDestino, radioEvaluacion, mascaraSegura, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < solapados.Length; i++)
        {
            Collider col = solapados[i];
            if (EsColliderIgnorable(col, objetivoIgnorado)) continue;

            colisionDetectada = true;
            float margenInferior = (!esDescenso && conCarga) ? -offsetCargaY : 0f;
            altitudCruceroSegura = Mathf.Max(altitudCruceroSegura, col.bounds.max.y + margenInferior + radioEvaluacion + 0.50f);
        }

        if (esDescenso && colisionDetectada)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Corrutina de despeje autónomo con perfil de vuelo ortogonal en "U", contacto físico real (0.00 m)
    /// entre Campana_Ventosa y la malla visible del objetivo, y liberación verificada con Y &gt;= 3.00 m.
    /// </summary>
    public IEnumerator EjecutarMisionDespeje(GameObject obstaculo, Action onDespejado = null, float duracionTotal = 12.0f)
    {
        if (obstaculo == null) yield break;

        ocupado = true;
        tieneCargaAcoplada = false;
        ConfigurarEstadoLedSuccion(false);
        AplicarExtensionChupa(0.20f);

        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        Vector3 posObs = obstaculo.transform.position;
        float topeVisual = CalcularTopeSuperiorVisual(obstaculo);
        float alturaVisualSobrePivote = Mathf.Max(0.10f, topeVisual - posObs.y);
        Collider colObstaculo = obstaculo.GetComponent<Collider>();

        float yCrucero = Mathf.Max(10.0f, posicionBaseTecho.y);
        Vector3 posCenit = new Vector3(posObs.x, yCrucero, posObs.z);
        ValidarMovimientoAereoSeguro(transform.position, posCenit, false, obstaculo, out float altitudCruceroCalculada);
        yCrucero = Mathf.Max(yCrucero, altitudCruceroCalculada);
        posCenit.y = yCrucero;

        Vector3 posAcople = new Vector3(posObs.x, topeVisual + 1.15f, posObs.z);

        // -------------------------------------------------------------
        // ETAPA 1A (2.0s): Tránsito horizontal en techo hasta posCenit
        // -------------------------------------------------------------
        yield return StartCoroutine(MoverTramoDron(transform.position, posCenit, 2.0f, true));

        // Verificación espacial activa antes de descender (con Fail-Safe anti-congelamiento)
        if (!ValidarMovimientoAereoSeguro(posCenit, posAcople, false, obstaculo, out _))
        {
            Debug.LogWarning($"<color=orange>[DRON MANTENIMIENTO]:</color> Descenso bloqueado por envolvente aérea o zona protegida en {posObs}. Aplicando fail-safe de liberación.");
            DashboardUI.Instance?.RegistrarLog("<color=#F59E0B>[DRON FAIL-SAFE]:</color> Zona aérea restringida detectada sobre obstáculo. Despeje preventivo aplicado para liberar AMR.");

            if (obstaculo != null)
            {
                obstaculo.transform.SetParent(null, true);
                if (colObstaculo != null) colObstaculo.enabled = true;
                obstaculo.SetActive(false);
            }

            onDespejado?.Invoke();

            yield return StartCoroutine(MoverTramoDron(transform.position, posicionBaseTecho, 1.5f, true));
            transform.position = posicionBaseTecho;
            transform.rotation = rotacionBase;
            tieneCargaAcoplada = false;
            ConfigurarEstadoLedSuccion(false);
            AplicarExtensionChupa(0.20f);
            ocupado = false;
            yield break;
        }

        // -------------------------------------------------------------
        // ETAPA 1B (2.2s): Descenso vertical puro y extensión telescópica hasta contacto 0.00 m
        // -------------------------------------------------------------
        const float durEtapa1B = 2.2f;
        float t1b = 0f;
        while (t1b < durEtapa1B)
        {
            t1b += Time.deltaTime;
            float factor = Mathf.Clamp01(t1b / durEtapa1B);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posCenit.y, posAcople.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;

            if (factor >= 0.5f)
            {
                float tChupa = (factor - 0.5f) * 2f;
                float yBaseRetraida = transform.position.y - 0.47f;
                PosicionarChupaPorCotaBaseMundo(Mathf.Lerp(yBaseRetraida, topeVisual, tChupa));
            }

            yield return null;
        }
        transform.position = posAcople;
        transform.rotation = rotacionBase;
        PosicionarChupaPorCotaBaseMundo(topeVisual);

        // -------------------------------------------------------------
        // ETAPA 2 (1.3s): Contacto confirmado -> LED verde, desactivar collider, retracción soldada e izaje inicial
        // -------------------------------------------------------------
        const float durEtapa2 = 1.3f;
        ConfigurarEstadoLedSuccion(true);
        if (colObstaculo != null)
        {
            colObstaculo.enabled = false;
        }

        obstaculo.transform.SetParent(transform, true);
        tieneCargaAcoplada = true;

        Vector3 posAcopleInicio = transform.position;
        Vector3 posAcopleElevado = posAcopleInicio + Vector3.up * 0.80f;

        float t2 = 0f;
        while (t2 < durEtapa2)
        {
            t2 += Time.deltaTime;
            float factor = Mathf.Clamp01(t2 / durEtapa2);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posAcopleInicio, posAcopleElevado, smooth);
            transform.rotation = rotacionBase;

            float desfaseBase = Mathf.Lerp(-1.15f, -0.62f, smooth);
            SincronizarChupaYCarga(transform.position.y + desfaseBase, obstaculo, alturaVisualSobrePivote);

            yield return null;
        }
        transform.position = posAcopleElevado;
        transform.rotation = rotacionBase;
        SincronizarChupaYCarga(transform.position.y - 0.62f, obstaculo, alturaVisualSobrePivote);

        // -------------------------------------------------------------
        // ETAPA 3A (3.5s): Ascenso vertical puro con carga hasta techo; dispara onDespejado solo con Y >= 3.00m
        // -------------------------------------------------------------
        ValidarMovimientoAereoSeguro(transform.position, posCenit, true, obstaculo, out float altitudCarga);
        float yCruceroCarga = Mathf.Max(yCrucero, altitudCarga);
        Vector3 posCenitCarga = new Vector3(posObs.x, yCruceroCarga, posObs.z);

        const float durEtapa3A = 3.5f;
        Vector3 posInicio3A = transform.position;
        bool despejeNotificado = false;
        float t3a = 0f;
        while (t3a < durEtapa3A)
        {
            t3a += Time.deltaTime;
            float factor = Mathf.Clamp01(t3a / durEtapa3A);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posInicio3A.y, posCenitCarga.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;
            SincronizarChupaYCarga(transform.position.y - 0.62f, obstaculo, alturaVisualSobrePivote);

            if (!despejeNotificado && obstaculo != null && obstaculo.transform.position.y >= cotaGaliboSeguroY)
            {
                despejeNotificado = true;
                onDespejado?.Invoke();
            }

            yield return null;
        }
        transform.position = posCenitCarga;
        transform.rotation = rotacionBase;
        SincronizarChupaYCarga(transform.position.y - 0.62f, obstaculo, alturaVisualSobrePivote);

        if (!despejeNotificado)
        {
            despejeNotificado = true;
            onDespejado?.Invoke();
        }

        // -------------------------------------------------------------
        // ETAPA 3B (2.5s): Tránsito horizontal en techo con carga hasta base aérea
        // -------------------------------------------------------------
        ValidarMovimientoAereoSeguro(posCenitCarga, new Vector3(posicionBaseTecho.x, yCruceroCarga, posicionBaseTecho.z), true, obstaculo, out float altitudRetorno);
        float yRetorno = Mathf.Max(yCruceroCarga, altitudRetorno);
        if (yRetorno > yCruceroCarga + 0.05f)
        {
            yield return StartCoroutine(MoverTramoDron(transform.position, new Vector3(posObs.x, yRetorno, posObs.z), 0.5f, false));
        }
        yield return StartCoroutine(MoverTramoDron(transform.position, new Vector3(posicionBaseTecho.x, yRetorno, posicionBaseTecho.z), 2.5f, true));
        if (Mathf.Abs(transform.position.y - posicionBaseTecho.y) > 0.05f)
        {
            yield return StartCoroutine(MoverTramoDron(transform.position, posicionBaseTecho, 0.5f, false));
        }

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // FINALIZACIÓN: Desemparentar obstáculo, restaurar collider, LED rojo y retraer chupa
        // -------------------------------------------------------------
        if (obstaculo != null)
        {
            obstaculo.transform.SetParent(null, true);
            if (colObstaculo != null)
            {
                colObstaculo.enabled = true;
            }
            obstaculo.SetActive(false);
        }

        tieneCargaAcoplada = false;
        ConfigurarEstadoLedSuccion(false);
        AplicarExtensionChupa(0.20f);
        ocupado = false;
    }

    /// <summary>
    /// Corrutina de rescate aéreo de 5 etapas para un AMR colapsado (&lt; 3% BMS) usando la chupa industrial articulada.
    /// </summary>
    public IEnumerator EjecutarRescateAMRAbortado(RobotAMR amrApagado, Vector3 posPadDestino, Action onRescateCompletado = null, Action onElevacionCompletada = null)
    {
        if (amrApagado == null) yield break;

        while (ocupado)
        {
            yield return null;
        }

        ocupado = true;
        tieneCargaAcoplada = false;
        ConfigurarEstadoLedSuccion(false);
        AplicarExtensionChupa(0.20f);

        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        Vector3 posAMR = amrApagado.transform.position;
        float topeVisualAMR = CalcularTopeSuperiorVisual(amrApagado.gameObject);
        float alturaVisualAMR = Mathf.Max(0.20f, topeVisualAMR - posAMR.y);

        float yCrucero = Mathf.Max(10.0f, posicionBaseTecho.y);
        Vector3 posCenitAMR = new Vector3(posAMR.x, yCrucero, posAMR.z);
        Vector3 posAcopleAMR = new Vector3(posAMR.x, topeVisualAMR + 1.15f, posAMR.z);
        Vector3 posElevadoAMR = new Vector3(posAMR.x, Mathf.Max(8.5f, yCrucero - 1.0f), posAMR.z);

        // Etapa 1A (2.0s): Vuelo a crucero hasta el cenit del AMR apagado
        yield return StartCoroutine(MoverTramoDron(transform.position, posCenitAMR, 2.0f, true));

        // Etapa 1B (2.0s): Descenso vertical puro y extensión telescópica hasta contacto 0.00 m sobre chasis
        const float durDescenso1B = 2.0f;
        float t1b = 0f;
        while (t1b < durDescenso1B)
        {
            t1b += Time.deltaTime;
            float factor = Mathf.Clamp01(t1b / durDescenso1B);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posCenitAMR, posAcopleAMR, smooth);
            transform.rotation = rotacionBase;

            if (factor >= 0.5f)
            {
                float tChupa = (factor - 0.5f) * 2f;
                PosicionarChupaPorCotaBaseMundo(Mathf.Lerp(transform.position.y - 0.47f, topeVisualAMR, tChupa));
            }
            yield return null;
        }
        transform.position = posAcopleAMR;
        PosicionarChupaPorCotaBaseMundo(topeVisualAMR);

        // Etapa 2 (2.5s): Succión neumática (LED verde), retracción parcial e izaje vertical a 0.00 m de separación
        ConfigurarEstadoLedSuccion(true);
        amrApagado.transform.SetParent(transform, true);
        tieneCargaAcoplada = true;

        const float durEtapa2 = 2.5f;
        float t2 = 0f;
        while (t2 < durEtapa2)
        {
            t2 += Time.deltaTime;
            float factor = Mathf.Clamp01(t2 / durEtapa2);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posAcopleAMR, posElevadoAMR, smooth);
            transform.rotation = rotacionBase;

            float desfaseBase = Mathf.Lerp(-1.15f, -0.62f, smooth);
            SincronizarChupaYCarga(transform.position.y + desfaseBase, amrApagado.gameObject, alturaVisualAMR);
            yield return null;
        }
        transform.position = posElevadoAMR;
        SincronizarChupaYCarga(transform.position.y - 0.62f, amrApagado.gameObject, alturaVisualAMR);
        onElevacionCompletada?.Invoke();

        // Etapa 3 (3.0s): Tránsito horizontal aéreo hasta el cenit de posPadDestino respetando gálibo de carga
        Vector3 posCenitPad = new Vector3(posPadDestino.x, posElevadoAMR.y, posPadDestino.z);
        ValidarMovimientoAereoSeguro(posElevadoAMR, posCenitPad, true, amrApagado.gameObject, out float ySeguraRescate);
        if (ySeguraRescate > posElevadoAMR.y + 0.05f)
        {
            posElevadoAMR.y = ySeguraRescate;
            posCenitPad.y = ySeguraRescate;
            yield return StartCoroutine(MoverTramoDron(transform.position, posElevadoAMR, 0.6f, false));
        }
        yield return StartCoroutine(MoverTramoDron(transform.position, posCenitPad, 3.0f, true));

        // Etapa 4 (2.0s): Descenso vertical hasta posar el AMR suavemente en posPadDestino y liberar succión
        float topeVisualEnPad = posPadDestino.y + alturaVisualAMR;
        Vector3 posDescensoPad = new Vector3(posPadDestino.x, topeVisualEnPad + 1.15f, posPadDestino.z);

        const float durEtapa4 = 2.0f;
        float t4 = 0f;
        while (t4 < durEtapa4)
        {
            t4 += Time.deltaTime;
            float factor = Mathf.Clamp01(t4 / durEtapa4);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posCenitPad, posDescensoPad, smooth);
            transform.rotation = rotacionBase;

            float desfaseBase = factor >= 0.6f
                ? Mathf.Lerp(-0.62f, -1.15f, (factor - 0.6f) / 0.4f)
                : -0.62f;
            SincronizarChupaYCarga(transform.position.y + desfaseBase, amrApagado.gameObject, alturaVisualAMR);
            yield return null;
        }
        transform.position = posDescensoPad;
        SincronizarChupaYCarga(topeVisualEnPad, amrApagado.gameObject, alturaVisualAMR);

        amrApagado.transform.SetParent(null, true);
        amrApagado.FinalizarRescateEnPad(posPadDestino);
        tieneCargaAcoplada = false;
        ConfigurarEstadoLedSuccion(false);
        AplicarExtensionChupa(0.20f);
        onRescateCompletado?.Invoke();

        // Etapa 5 (2.0s): Ascenso a Y >= 10.0m y retorno a posicionBaseTecho
        Vector3 posCenitPadTecho = new Vector3(posPadDestino.x, yCrucero, posPadDestino.z);
        yield return StartCoroutine(MoverTramoDron(posDescensoPad, posCenitPadTecho, 0.7f, false));
        yield return StartCoroutine(MoverTramoDron(posCenitPadTecho, posicionBaseTecho, 1.3f, true));

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;
        ocupado = false;
    }

    private IEnumerator MoverTramoDron(Vector3 origen, Vector3 destino, float duracion, bool rotarHaciaDestino)
    {
        float dur = Mathf.Max(0.05f, duracion);
        Vector3 dirPlana = destino - origen;
        dirPlana.y = 0f;
        Quaternion rotMirar = dirPlana.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dirPlana) : rotacionBase;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float factor = Mathf.Clamp01(t / dur);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(origen, destino, smooth);
            if (rotarHaciaDestino)
            {
                Quaternion rotObj = factor < 0.75f ? rotMirar : rotacionBase;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotObj, Time.deltaTime * 5f);
            }
            else
            {
                transform.rotation = rotacionBase;
            }
            yield return null;
        }

        transform.position = destino;
        transform.rotation = rotacionBase;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioEnvolventeVacio);

        Gizmos.color = new Color(1f, 0.55f, 0f, 0.95f);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0f, offsetCargaY, 0f), radioEnvolventeConCarga);
    }
}
