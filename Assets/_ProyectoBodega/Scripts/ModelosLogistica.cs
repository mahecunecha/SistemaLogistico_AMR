using System.Collections.Generic;

[System.Serializable]
public class Pedido
{
    public const float PesoMinimoIndustrialKg = 850f;
    public const float PesoReferenciaBaseKg = 1000f;
    public const float PesoMaximoIndustrialKg = 1250f;

    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;

    public Pedido ClonarConPeso(float nuevoPesoKg, string nuevoIdPallet = null)
    {
        return new Pedido
        {
            id_pallet = !string.IsNullOrEmpty(nuevoIdPallet) ? nuevoIdPallet : id_pallet,
            nombre_producto = nombre_producto,
            codigo_sku = codigo_sku,
            peso_kg = nuevoPesoKg,
            coordenada_bodega = coordenada_bodega
        };
    }
}

[System.Serializable]
public class Manifiesto
{
    public string id_lote;
    public int total_pallets;
    public float peso_total_kg;
    public List<Pedido> pedidos;
}
