using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Aquí arrastrarás tu panel 'Manifiesto'
    [SerializeField] private GameObject filaPrefab;     // Aquí arrastrarás tu Prefab azul

    // Función pública que llamaremos cuando el JSON termine de leerse
    public void PoblarTabla(List<Pedido> pedidosJSON)
    {
        // 1. Limpieza de seguridad: Borrar filas viejas si el JSON se recarga
        // (Empezamos desde el índice 1 para no borrar la Fila_Titulos que está en el índice 0)
        for (int i = 1; i < contenedorFilas.childCount; i++)
        {
            Destroy(contenedorFilas.GetChild(i).gameObject);
        }

        // 2. Crear una fila nueva por cada pedido en el JSON
        foreach (Pedido pedido in pedidosJSON)
        {
            // Clonamos el prefab dentro del contenedor
            GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);

            // Obtenemos los 5 textos de esa nueva fila
            TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            // Asignamos la información del JSON a cada columna respectiva
            columnas[0].text = pedido.id_pallet;
            columnas[1].text = pedido.nombre_producto; // Asegúrate de que esta variable exista en tu clase Pedido
            columnas[2].text = pedido.codigo_sku;
            columnas[3].text = pedido.peso_kg.ToString() + " kg";
            
            // Lógica visual para el estado
            columnas[4].text = "PENDIENTE";
            columnas[4].color = Color.red; // Resalta en rojo para indicar que el robot aún no lo mueve
        }
    }
}