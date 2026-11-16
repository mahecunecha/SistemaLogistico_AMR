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

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
            controlHelices.rotationAxis = new Vector3(0f, 0f, 1f);
        }
    }

    /// <summary>
    /// Corrutina de despeje autónomo con perfil de vuelo ortogonal en "U" (12.0 segundos totales):
    /// - Etapa 1A (2.0s): Tránsito horizontal a cota de techo Y=10m desde la base hasta el cenit del obstáculo.
    /// - Etapa 1B (2.5s): Descenso vertical puro (X/Z fijos) desde el cenit hasta posAcople (+1.76m sobre base del obstáculo).
    /// - Etapa 2  (1.5s): Acople magnético (SetParent), desactivación de BoxCollider, elevación vertical inicial (+1.0m) y reanudación temprana WMS.
    /// - Etapa 3A (3.5s): Ascenso vertical puro con carga hasta el techo (posCenit Y=10m).
    /// - Etapa 3B (2.5s): Tránsito horizontal con carga hasta la base aérea (-32, 10, -38), desemparentar y ocultar.
    /// </summary>
    public IEnumerator EjecutarMisionDespeje(GameObject obstaculo, Action onDespejado = null, float duracionTotal = 12.0f)
    {
        if (obstaculo == null) yield break;

        ocupado = true;
        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        Vector3 posObs = obstaculo.transform.position;
        Vector3 posCenit = new Vector3(posObs.x, posicionBaseTecho.y, posObs.z);
        Vector3 posAcople = new Vector3(posObs.x, posObs.y + alturaAcopleSobreObstaculo, posObs.z);

        Collider colObstaculo = obstaculo.GetComponent<Collider>();

        // -------------------------------------------------------------
        // ETAPA 1A (2.0s): Tránsito horizontal en techo hasta posCenit
        // -------------------------------------------------------------
        const float durEtapa1A = 2.0f;
        Vector3 posInicio1A = transform.position;
        float dist1A = Vector3.Distance(posInicio1A, posCenit);
        float vel1A = dist1A > 0.001f ? (dist1A / durEtapa1A) : 5f;

        Vector3 dir1A = (posCenit - posInicio1A);
        dir1A.y = 0f;
        Quaternion rotMirarCenit = dir1A.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir1A) : rotacionBase;

        float t1a = 0f;
        while (t1a < durEtapa1A)
        {
            float dt = Time.deltaTime;
            t1a += dt;
            float factor = Mathf.Clamp01(t1a / durEtapa1A);

            transform.position = Vector3.MoveTowards(transform.position, posCenit, vel1A * dt);
            Quaternion rotObjetivo = factor < 0.75f ? rotMirarCenit : rotacionBase;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 5f);

            yield return null;
        }
        transform.position = posCenit;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 1B (2.5s): Descenso vertical puro (X/Z fijos) hacia posAcople
        // -------------------------------------------------------------
        const float durEtapa1B = 2.5f;
        float t1b = 0f;
        while (t1b < durEtapa1B)
        {
            float dt = Time.deltaTime;
            t1b += dt;
            float factor = Mathf.Clamp01(t1b / durEtapa1B);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posCenit.y, posAcople.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posAcople;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 2 (1.5s): Acople, desactivación de collider, elevación vertical 1.0m y reanudación temprana
        // -------------------------------------------------------------
        const float durEtapa2 = 1.5f;
        if (colObstaculo != null)
        {
            colObstaculo.enabled = false;
        }

        obstaculo.transform.SetParent(transform, true);

        Vector3 posAcopleInicio = transform.position;
        Vector3 posAcopleElevado = posAcopleInicio + Vector3.up * 1.0f;

        float t2 = 0f;
        while (t2 < durEtapa2)
        {
            float dt = Time.deltaTime;
            t2 += dt;
            float factor = Mathf.Clamp01(t2 / durEtapa2);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posAcopleInicio, posAcopleElevado, smooth);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posAcopleElevado;
        transform.rotation = rotacionBase;

        // Reanudación temprana del AMR: el obstáculo ya está elevado 1m y su collider apagado
        onDespejado?.Invoke();

        // -------------------------------------------------------------
        // ETAPA 3A (3.5s): Ascenso vertical puro con carga hasta el techo (posCenit)
        // -------------------------------------------------------------
        const float durEtapa3A = 3.5f;
        Vector3 posInicio3A = transform.position;
        float t3a = 0f;
        while (t3a < durEtapa3A)
        {
            float dt = Time.deltaTime;
            t3a += dt;
            float factor = Mathf.Clamp01(t3a / durEtapa3A);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posInicio3A.y, posCenit.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posCenit;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 3B (2.5s): Tránsito horizontal en techo hasta la base (-32, 10, -38)
        // -------------------------------------------------------------
        const float durEtapa3B = 2.5f;
        Vector3 posInicio3B = transform.position;
        float dist3B = Vector3.Distance(posInicio3B, posicionBaseTecho);
        float vel3B = dist3B > 0.001f ? (dist3B / durEtapa3B) : 5f;

        Vector3 dir3B = (posicionBaseTecho - posInicio3B);
        dir3B.y = 0f;
        Quaternion rotMirarBase = dir3B.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir3B) : rotacionBase;

        float t3b = 0f;
        while (t3b < durEtapa3B)
        {
            float dt = Time.deltaTime;
            t3b += dt;
            float factor = Mathf.Clamp01(t3b / durEtapa3B);

            transform.position = Vector3.MoveTowards(transform.position, posicionBaseTecho, vel3B * dt);
            Quaternion rotObjetivo = factor < 0.75f ? rotMirarBase : rotacionBase;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 5f);

            yield return null;
        }
        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // FINALIZACIÓN: Desemparentar obstáculo, restaurar collider y ocultar
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

        ocupado = false;
    }

    /// <summary>
    /// Corrutina de rescate aéreo de 5 etapas para un AMR colapsado (&lt; 3% BMS):
    /// - Etapa 1A (2.0s): Tránsito horizontal a cota Y=10.0m hasta el cenit del AMR apagado.
    /// - Etapa 1B (2.0s): Descenso vertical puro hasta acoplarse sobre el chasis (+1.15m).
    /// - Etapa 2  (2.5s): Acople magnético (SetParent) y elevación vertical hasta Y=8.5m.
    /// - Etapa 3  (3.0s): Tránsito horizontal aéreo a Y=8.5m hasta el cenit del Pad Inductivo destino.
    /// - Etapa 4  (2.0s): Descenso vertical hasta posar al AMR en posPadDestino, desemparentar y reactivar en Pad.
    /// - Etapa 5  (2.0s): Ascenso vertical a Y=10.0m y retorno a la base aérea (-32, 10, -38).
    /// </summary>
    public IEnumerator EjecutarRescateAMRAbortado(RobotAMR amrApagado, Vector3 posPadDestino, Action onRescateCompletado = null, Action onElevacionCompletada = null)
    {
        if (amrApagado == null) yield break;

        while (ocupado)
        {
            yield return null;
        }

        ocupado = true;
        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        Vector3 posAMR = amrApagado.transform.position;
        Vector3 posCenitAMR = new Vector3(posAMR.x, posicionBaseTecho.y, posAMR.z);
        Vector3 posAcopleAMR = new Vector3(posAMR.x, posAMR.y + 1.15f, posAMR.z);
        Vector3 posElevadoAMR = new Vector3(posAMR.x, 8.5f, posAMR.z);
        Vector3 posCenitPad85 = new Vector3(posPadDestino.x, 8.5f, posPadDestino.z);
        Vector3 posDescensoPad = new Vector3(posPadDestino.x, posPadDestino.y + 1.15f, posPadDestino.z);
        Vector3 posCenitPadTecho = new Vector3(posPadDestino.x, posicionBaseTecho.y, posPadDestino.z);

        // Etapa 1A (2.0s): Vuelo a Y = 10.0m hasta el cenit del AMR apagado
        yield return StartCoroutine(MoverTramoDron(transform.position, posCenitAMR, 2.0f, true));

        // Etapa 1B (2.0s): Descenso vertical puro hasta Y_amr + 1.15m
        yield return StartCoroutine(MoverTramoDron(posCenitAMR, posAcopleAMR, 2.0f, false));

        // Etapa 2 (2.5s): Emparentar AMR apagado y elevar verticalmente hasta Y = 8.5m
        amrApagado.transform.SetParent(transform, true);
        yield return StartCoroutine(MoverTramoDron(posAcopleAMR, posElevadoAMR, 2.5f, false));
        onElevacionCompletada?.Invoke();

        // Etapa 3 (3.0s): Tránsito horizontal a Y = 8.5m hasta el cenit de posPadDestino
        yield return StartCoroutine(MoverTramoDron(posElevadoAMR, posCenitPad85, 3.0f, true));

        // Etapa 4 (2.0s): Descenso vertical hasta posar al AMR en posPadDestino, desemparentar y finalizar rescate
        yield return StartCoroutine(MoverTramoDron(posCenitPad85, posDescensoPad, 2.0f, false));

        amrApagado.transform.SetParent(null, true);
        amrApagado.FinalizarRescateEnPad(posPadDestino);
        onRescateCompletado?.Invoke();

        // Etapa 5 (2.0s): Ascenso a Y = 10.0m y retorno a posicionBaseTecho
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
}
