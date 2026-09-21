using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DashboardUI : MonoBehaviour
{
    /*Al usar static, le estamos diciendo a Unity: "Esta variable no le pertenece a un objeto individual,
    le pertenece a la clase globalmente"
    Por convención mundial en C#, cuando se hace un Singleton, a la variable que guarda la copia única se le llama Instance.*/
    public static DashboardUI Instance { get; private set; }

    [Header("UI Referencias")]
    [Tooltip("Asigna aquí el TextMeshProUGUI. Si usas Text heredado, asígnalo abajo.")]
    [SerializeField] private Component textoLogTMP;
    [Tooltip("Asigna aquí el Text heredado si no usas TMPro.")]
    [SerializeField] private Text textoLogLegacy;

    private Queue<string> logQueue = new Queue<string>();
    private const int MaxLineas = 5;

    void Awake()
    {
        /*Aquí le estamos diciendo: "Si ya existe una instancia de esta clase en la escena,
         y no soy yo mismo, destrúyeme". Esto evita que Unity cree copias o duplicados.*/
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegistrarLog(string mensaje)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}]";
        string logFinal = $"{timestamp} {mensaje}";

        logQueue.Enqueue(logFinal);

        if (logQueue.Count > MaxLineas)
        {
            logQueue.Dequeue();
        }

        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        string contenido = string.Join("\n", logQueue);
        
        if (textoLogTMP != null)
        {
            var prop = textoLogTMP.GetType().GetProperty("text");
            if (prop != null)
            {
                prop.SetValue(textoLogTMP, contenido);
            }
        }
        else if (textoLogLegacy != null)
        {
            textoLogLegacy.text = contenido;
        }
    }
}
