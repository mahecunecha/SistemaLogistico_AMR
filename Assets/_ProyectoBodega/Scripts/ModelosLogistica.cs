using System.Collections.Generic;

[System.Serializable]
public class Pedido
{
    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;
}

[System.Serializable]
public class Manifiesto
{
    public string id_lote;
    public int total_pallets;
    public float peso_total_kg;
    public List<Pedido> pedidos;
}
