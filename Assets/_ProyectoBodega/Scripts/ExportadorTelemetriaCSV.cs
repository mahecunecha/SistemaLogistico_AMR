using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Exporta la telemetría del turno nocturno, flota AMR, muelles TMS y bitácora SCADA a formato CSV (RFC 4180).
/// </summary>
public static class ExportadorTelemetriaCSV
{
    public static string ExportarReporteTurnoCSV(out int totalFilasExportadas)
    {
        totalFilasExportadas = 0;

        RobotManagerEVE eve = RobotManagerEVE.Instance != null
            ? RobotManagerEVE.Instance
            : UnityEngine.Object.FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);

        ModuloDespachoTMS tms = ModuloDespachoTMS.Instance != null
            ? ModuloDespachoTMS.Instance
            : UnityEngine.Object.FindFirstObjectByType<ModuloDespachoTMS>(FindObjectsInactive.Include);

        DashboardUI dashboard = DashboardUI.Instance != null
            ? DashboardUI.Instance
            : UnityEngine.Object.FindFirstObjectByType<DashboardUI>(FindObjectsInactive.Include);

        string carpetaDestino = Path.Combine(Application.persistentDataPath, "Reportes_SCADA");
        Directory.CreateDirectory(carpetaDestino);

        string nombreArchivo = $"telemetria_scada_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        var sb = new StringBuilder(8192);
        CultureInfo inv = CultureInfo.InvariantCulture;

        // Sección 1 — Resumen Operativo y SLA
        sb.AppendLine("SECCION,CLAVE,VALOR,DETALLE");

        string fechaAhora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", inv);
        AgregarFila(sb, ref totalFilasExportadas, "RESUMEN_OPERATIVO", "FechaExportacion", fechaAhora, "Local");

        string idLote = (eve != null && eve.manifiestoActual != null && !string.IsNullOrEmpty(eve.manifiestoActual.id_lote))
            ? eve.manifiestoActual.id_lote
            : "SIN_LOTE";
        int totalPalletsLote = (eve != null && eve.manifiestoActual != null) ? eve.manifiestoActual.total_pallets : 0;
        float pesoTotalLote = (eve != null && eve.manifiestoActual != null) ? eve.manifiestoActual.peso_total_kg : 0f;
        AgregarFila(
            sb,
            ref totalFilasExportadas,
            "RESUMEN_OPERATIVO",
            "LoteActivo",
            idLote,
            string.Format(inv, "TotalPallets={0}; PesoTotalKg={1:F1}", totalPalletsLote, pesoTotalLote));

        float horaTurno = eve != null ? eve.horaTurnoActual : 0f;
        AgregarFila(
            sb,
            ref totalFilasExportadas,
            "RESUMEN_OPERATIVO",
            "HoraTurno",
            horaTurno.ToString("F2", inv),
            "Escala 8.00 h");

        float velFlota = eve != null ? eve.VelocidadFlotaActual : 3.5f;
        AgregarFila(
            sb,
            ref totalFilasExportadas,
            "RESUMEN_OPERATIVO",
            "VelocidadFlota_mps",
            velFlota.ToString("F2", inv),
            "Rango 2.00..5.50 m/s");

        bool pausado = eve != null && eve.SimulacionPausada;
        AgregarFila(
            sb,
            ref totalFilasExportadas,
            "RESUMEN_OPERATIVO",
            "SimulacionPausada",
            pausado ? "TRUE" : "FALSE",
            string.Format(inv, "TimeScale={0:F2}", Time.timeScale));

        int enCola = eve != null ? eve.palletsEnColaWMS : 0;
        int completados = eve != null ? eve.palletsCompletadosTurno : 0;
        bool dual = eve != null && eve.flotaEscaladaDual;
        AgregarFila(sb, ref totalFilasExportadas, "RESUMEN_OPERATIVO", "PalletsEnColaWMS", enCola.ToString(inv), string.Empty);
        AgregarFila(sb, ref totalFilasExportadas, "RESUMEN_OPERATIVO", "PalletsCompletadosTurno", completados.ToString(inv), string.Empty);
        AgregarFila(sb, ref totalFilasExportadas, "RESUMEN_OPERATIVO", "FlotaEscaladaDual", dual ? "TRUE" : "FALSE", string.Empty);

        if (eve != null)
        {
            var sla = eve.UltimaEvaluacionSLA;
            AgregarFila(
                sb,
                ref totalFilasExportadas,
                "RESUMEN_OPERATIVO",
                "UltimaEvaluacionSLA",
                sla.Decision.ToString(),
                string.Format(
                    inv,
                    "ProyeccionCola={0}; HorasRestantes={1:F2}; AMRRequeridos={2}; ETAHoras={3:F2}",
                    sla.ProyeccionCola,
                    sla.HorasRestantes,
                    sla.AMRRequeridos,
                    sla.ETAHoras));

            bool dronOcupado = eve.dronMantenimiento != null && eve.dronMantenimiento.ocupado;
            bool dronCarga = eve.dronMantenimiento != null && eve.dronMantenimiento.tieneCargaAcoplada;
            AgregarFila(
                sb,
                ref totalFilasExportadas,
                "RESUMEN_OPERATIVO",
                "DronSoporteISO",
                dronOcupado ? "OCUPADO" : "STANDBY",
                $"CargaAcoplada={dronCarga}");
        }

        sb.AppendLine();

        // Sección 2 — Telemetría de Flota AMR (AMR-01, AMR-02, AMR-03)
        sb.AppendLine("FLOTA_AMR,ID_Robot,Estado_FSM,Bateria_SoC_Pct,Velocidad_Base_mps,Modo_Degradacion,Parada_LiDAR,Carga_Fisica,ID_Pallet,Muelle_Asignado,Slot_LIFO,Pos_X,Pos_Z");
        RobotAMR[] flota = new RobotAMR[]
        {
            eve != null ? eve.robotAMR_01 : null,
            eve != null ? eve.robotAMR_02 : null,
            eve != null ? eve.robotAMR_03 : null
        };

        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR amr = flota[i];
            if (amr == null) continue;

            string idPallet = amr.PedidoActual != null && !string.IsNullOrEmpty(amr.PedidoActual.id_pallet)
                ? amr.PedidoActual.id_pallet
                : "NINGUNO";

            AgregarFila(
                sb,
                ref totalFilasExportadas,
                "FLOTA_AMR",
                amr.idRobot,
                amr.estadoActual.ToString(),
                amr.bateriaActual.ToString("F2", inv),
                amr.VelocidadBase.ToString("F2", inv),
                amr.ModoDegradacionActivo ? "TRUE" : "FALSE",
                amr.EnParadaSeguridadLiDAR ? "TRUE" : "FALSE",
                amr.TieneCargaFisica ? "TRUE" : "FALSE",
                idPallet,
                amr.numCamionAsignado.ToString(inv),
                amr.slotLIFOAsignado.ToString(inv),
                amr.transform.position.x.ToString("F2", inv),
                amr.transform.position.z.ToString("F2", inv));
        }

        sb.AppendLine();

        // Sección 3 — Estado de Muelles TMS
        sb.AppendLine("MUELLES_TMS,Numero_Camion,Muelle_Activo,Estibas_Actuales,Capacidad_Max_Pallets,Peso_Acumulado_Kg,Capacidad_Max_Kg,Sellado,Inspeccion_Humana");
        if (tms != null)
        {
            for (int c = 1; c <= 3; c++)
            {
                AgregarFila(
                    sb,
                    ref totalFilasExportadas,
                    "MUELLES_TMS",
                    c.ToString(inv),
                    tms.CamionTurnoActual == c ? "TRUE" : "FALSE",
                    tms.ObtenerCantidadEstibasCamion(c).ToString(inv),
                    ModuloDespachoTMS.CapacidadMaximaPalletsCamion.ToString(inv),
                    tms.ObtenerPesoCamion(c).ToString("F1", inv),
                    ModuloDespachoTMS.CapacidadMaximaCamionKg.ToString("F1", inv),
                    tms.EstaCamionSellado(c) ? "TRUE" : "FALSE",
                    tms.EstaEnInspeccionHumana(c) ? "TRUE" : "FALSE");
            }
        }

        sb.AppendLine();

        // Sección 4 — Historial Acumulado de Estibas LIFO del Turno
        sb.AppendLine("HISTORIAL_ESTIBAS_LIFO,Timestamp,Numero_Camion,Slot_LIFO,ID_Pallet,Producto,SKU,Peso_Kg");
        if (tms != null && tms.HistorialEstibasTurno != null)
        {
            IReadOnlyList<ModuloDespachoTMS.RegistroEstibaTurno> estibas = tms.HistorialEstibasTurno;
            for (int i = 0; i < estibas.Count; i++)
            {
                var reg = estibas[i];
                AgregarFila(
                    sb,
                    ref totalFilasExportadas,
                    "HISTORIAL_ESTIBAS_LIFO",
                    reg.Timestamp ?? string.Empty,
                    reg.NumeroCamion.ToString(inv),
                    reg.SlotLIFO.ToString(inv),
                    reg.IdPallet ?? string.Empty,
                    reg.Producto ?? string.Empty,
                    reg.SKU ?? string.Empty,
                    reg.PesoKg.ToString("F1", inv));
            }
        }

        sb.AppendLine();

        // Sección 5 — Bitácora SCADA
        sb.AppendLine("BITACORA_SCADA,Indice,Evento");
        if (dashboard != null)
        {
            IReadOnlyList<string> lineas = dashboard.ObtenerLineasBitacoraLimpia();
            for (int i = 0; i < lineas.Count; i++)
            {
                AgregarFila(sb, ref totalFilasExportadas, "BITACORA_SCADA", (i + 1).ToString(inv), lineas[i]);
            }
        }

        File.WriteAllText(rutaCompleta, sb.ToString(), new UTF8Encoding(true));

        dashboard?.RegistrarLog($"<color=#10B981><b>[EXPORTACIÓN CSV]:</b></color> Reporte generado ({totalFilasExportadas} filas): {rutaCompleta}");
        dashboard?.MostrarPopUpEjecutivoSLA(
            "<color=#10B981>REPORTE CSV EXPORTADO</color>",
            $"Archivo: {nombreArchivo} ({totalFilasExportadas} filas)\nRuta: {rutaCompleta}",
            5.0f);
        GestorAudioSCADA.Instance?.ReproducirConfirmacionEntrega();

        return rutaCompleta;
    }

    private static void AgregarFila(StringBuilder sb, ref int contadorFilas, params string[] columnas)
    {
        for (int i = 0; i < columnas.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(EscaparCSV(columnas[i]));
        }
        sb.AppendLine();
        contadorFilas++;
    }

    private static string EscaparCSV(string campo)
    {
        if (string.IsNullOrEmpty(campo)) return string.Empty;

        bool requiereComillas = campo.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!requiereComillas) return campo;

        return "\"" + campo.Replace("\"", "\"\"") + "\"";
    }
}
