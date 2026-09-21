using UnityEngine;
using System.IO;
using System.Collections.Generic;

/* Definimos la estructura exacta de un pedido en el JSON, Sysytem.Serializable nos permite convertir el JSON en variables de C#*/
[System.Serializable]
public class Pedido
{
    /*Variables idénticas en nombre y tipo de dato a las del JSON para facilitar el trabajo entre sistemas*/
    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;
}

// Envolvemos la lista para que JsonUtility de Unity pueda leer el arreglo
[System.Serializable]
public class Manifiesto
{
    public List<Pedido> pedidos;
}

public class GestorJSON : MonoBehaviour
{
    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;

    void Start()
    {
        CargarManifiesto();
    }

    void CargarManifiesto()
    {
        /* Ruta dinámica que funciona en el Editor y en el ejecutable final (.exe), streamingAssetsPath es una proteccion contra el Empaquetado(Inmunidad) Unity tiene
        prohibido alterar, comprimir o encriptar lo que hay allí dentro*/
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        /*antes de leer el archivo, verifica si el archivo existe, si no existe, lanza un error y detiene el programa*/
        if (File.Exists(ruta))
        {
            /*Si existe, lee el contenido del archivo*/
            string contenidoJson = File.ReadAllText(ruta);
            /* Magia del desacoplamiento: El texto se convierte en variables de C#*/
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>Éxito:</color> Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("Éxito: Manifiesto cargado. Total de pallets a despachar: " + manifiestoActual.pedidos.Count);
            // Si hay pedidos y el robot está conectado, le enviamos la primera orden (Índice 0)
            if (manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
            {
                robotEjecutor.AsignarMision(manifiestoActual.pedidos[0]);
            }
        }
        else
        {
            Debug.LogError("Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("Error crítico: No se encontró el Manifiesto de Carga.");
        }
    }
}