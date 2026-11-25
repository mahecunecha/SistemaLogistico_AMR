using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProyectoBodega.Tests.EditMode
{
    public class AdmisionSLATests
    {
        private readonly List<GameObject> objetosTemporales = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            RelojSimuladorTurno.ReiniciarRelojTurno(0.0f);
            for (int i = objetosTemporales.Count - 1; i >= 0; i--)
            {
                if (objetosTemporales[i] != null)
                {
                    Object.DestroyImmediate(objetosTemporales[i]);
                }
            }
            objetosTemporales.Clear();
        }

        private RobotManagerEVE CrearEVE()
        {
            GameObject go = new GameObject("Test_RobotManagerEVE_SLA");
            objetosTemporales.Add(go);
            RobotManagerEVE eve = go.AddComponent<RobotManagerEVE>();
            eve.horaTurnoActual = 3.0f;
            eve.palletsEnColaWMS = 0;
            eve.flotaEscaladaDual = false;
            return eve;
        }

        private static Manifiesto CrearManifiestoLote(string idLote, int totalPallets)
        {
            return new Manifiesto
            {
                id_lote = idLote,
                total_pallets = totalPallets,
                peso_total_kg = totalPallets * 850f,
                pedidos = new List<Pedido>
                {
                    new Pedido
                    {
                        id_pallet = "PAL-8491",
                        nombre_producto = "Aguila Lata",
                        codigo_sku = "AGU-RUB-330",
                        peso_kg = 850f,
                        coordenada_bodega = "P1-C1-F1"
                    }
                }
            };
        }

        [Test]
        public void EvaluarIngestaAutonoma_TresUmbralesIndividuales_30_260_450()
        {
            RobotManagerEVE eve30 = CrearEVE();
            eve30.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-ERP-030", 30));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Aprobado1AMR, eve30.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(30, eve30.palletsEnColaWMS);
            Assert.IsFalse(eve30.flotaEscaladaDual);
            Assert.AreEqual(1, eve30.UltimaEvaluacionSLA.AMRRequeridos);

            RobotManagerEVE eve260 = CrearEVE();
            eve260.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-ERP-260", 260));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.EscaladoDual2AMRs, eve260.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(260, eve260.palletsEnColaWMS);
            Assert.IsTrue(eve260.flotaEscaladaDual);
            Assert.AreEqual(2, eve260.UltimaEvaluacionSLA.AMRRequeridos);

            RobotManagerEVE eve450 = CrearEVE();
            eve450.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-ERP-450", 450));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Rechazado, eve450.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(0, eve450.palletsEnColaWMS);
            Assert.IsFalse(eve450.flotaEscaladaDual);
            Assert.AreEqual(3, eve450.UltimaEvaluacionSLA.AMRRequeridos);
            Assert.IsNotEmpty(eve450.UltimaEvaluacionSLA.FeedbackTecnico);
            StringAssert.Contains("3 AMRs en piso", eve450.UltimaEvaluacionSLA.FeedbackTecnico);
        }

        [Test]
        public void EvaluarIngestaAutonoma_AcumulacionEnMismoTurno_EscalaYRechazaConservandoCola()
        {
            RobotManagerEVE eve = CrearEVE();

            eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-1", 30));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Aprobado1AMR, eve.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(30, eve.palletsEnColaWMS);
            Assert.IsFalse(eve.flotaEscaladaDual);

            eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-2", 260));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.EscaladoDual2AMRs, eve.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(290, eve.palletsEnColaWMS);
            Assert.IsTrue(eve.flotaEscaladaDual);

            eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-3", 200));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Rechazado, eve.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(290, eve.palletsEnColaWMS);
            Assert.IsTrue(eve.flotaEscaladaDual);
            Assert.IsNotEmpty(eve.UltimaEvaluacionSLA.FeedbackTecnico);
        }

        [Test]
        public void CalcularEvaluacionSLA_HorasRealesRestantesYBloqueoPorFaltaSoporte()
        {
            var evalInicio = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 0.0f);
            Assert.AreEqual(8.0f, evalInicio.HorasRestantes, 1e-4f);
            Assert.AreEqual(320f, evalInicio.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(640f, evalInicio.Capacidad2AMRs, 1e-3f);
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Aprobado1AMR, evalInicio.Decision);

            var evalMedia = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 3.5f);
            Assert.AreEqual(4.5f, evalMedia.HorasRestantes, 1e-4f);
            Assert.AreEqual(180f, evalMedia.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(360f, evalMedia.Capacidad2AMRs, 1e-3f);

            var evalFinal = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 7.5f);
            Assert.AreEqual(0.5f, evalFinal.HorasRestantes, 1e-4f);
            Assert.AreEqual(20f, evalFinal.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(40f, evalFinal.Capacidad2AMRs, 1e-3f);
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.EscaladoDual2AMRs, evalFinal.Decision);

            var evalExcedeFinal = RobotManagerEVE.CalcularEvaluacionSLA(0, 50, 7.5f);
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Rechazado, evalExcedeFinal.Decision);

            var evalBloqueoSoporte = RobotManagerEVE.CalcularEvaluacionSLA(
                10,
                20,
                1.0f,
                false,
                1,
                bloqueadoPorFaltaSoporte: true,
                motivoBloqueoSoporte: "Sin AMR de soporte disponible");
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Rechazado, evalBloqueoSoporte.Decision);
            StringAssert.Contains("BLOQUEO DE ADMISIÓN", evalBloqueoSoporte.FeedbackTecnico);
        }

        [Test]
        public void RelojSimuladorTurno_FormatoNocturnoYHorasRestantes_IndependienteSO()
        {
            RelojSimuladorTurno.ReiniciarRelojTurno();
            Assert.AreEqual("18:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("08:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.AreEqual(8.0f, RelojSimuladorTurno.HorasRestantesHastaCierre, 1e-4f);
            Assert.IsFalse(RelojSimuladorTurno.TurnoSuperoCierre);
            StringAssert.DoesNotContain("AM", RelojSimuladorTurno.HoraActualFormateada);
            StringAssert.DoesNotContain("PM", RelojSimuladorTurno.HoraActualFormateada);

            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(4.0f);
            Assert.AreEqual("22:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("04:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.AreEqual(4.0f, RelojSimuladorTurno.HorasRestantesHastaCierre, 1e-4f);
            Assert.IsFalse(RelojSimuladorTurno.TurnoSuperoCierre);

            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(6.0f);
            Assert.AreEqual("00:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("02:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.AreEqual(2.0f, RelojSimuladorTurno.HorasRestantesHastaCierre, 1e-4f);
            Assert.IsFalse(RelojSimuladorTurno.TurnoSuperoCierre);

            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(7.5f);
            Assert.AreEqual("01:30:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("00:30:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.AreEqual(0.5f, RelojSimuladorTurno.HorasRestantesHastaCierre, 1e-4f);
            Assert.IsFalse(RelojSimuladorTurno.TurnoSuperoCierre);

            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(8.0f);
            Assert.AreEqual("02:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("00:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.AreEqual(0.0f, RelojSimuladorTurno.HorasRestantesHastaCierre, 1e-4f);
            Assert.IsTrue(RelojSimuladorTurno.TurnoSuperoCierre);
        }

        [Test]
        public void RelojSimuladorTurno_AcumuladorDeterministaSegundos_Avance1a1SinSaltoComprimido()
        {
            RelojSimuladorTurno.ReiniciarRelojTurno();
            Assert.AreEqual(0.0, RelojSimuladorTurno.SegundosTranscurridos, 1e-9);
            Assert.AreEqual(0L, RelojSimuladorTurno.SegundosTranscurridosEnteros);
            Assert.AreEqual(28800L, RelojSimuladorTurno.SegundosRestantesEnteros);
            Assert.AreEqual("18:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("08:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            StringAssert.DoesNotContain("AM", RelojSimuladorTurno.HoraActualFormateada);
            StringAssert.DoesNotContain("PM", RelojSimuladorTurno.HoraActualFormateada);

            for (int i = 0; i < 600; i++)
            {
                Assert.IsTrue(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, 1.0 / 60.0));
            }

            Assert.AreEqual(10L, RelojSimuladorTurno.SegundosTranscurridosEnteros);
            Assert.AreEqual("18:00:10", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("07:59:50", RelojSimuladorTurno.TiempoRestanteFormateado);

            Assert.IsTrue(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, 23.0));
            Assert.AreEqual(33L, RelojSimuladorTurno.SegundosTranscurridosEnteros);
            Assert.AreEqual("18:00:33", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("07:59:27", RelojSimuladorTurno.TiempoRestanteFormateado);
        }

        [Test]
        public void RelojSimuladorTurno_ControlEstrictoStandbyPausaYCierre8Horas()
        {
            RelojSimuladorTurno.ReiniciarRelojTurno();

            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(false, false, 120.0));
            Assert.AreEqual("18:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("08:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);

            Assert.IsTrue(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, 3664.0));
            Assert.AreEqual("19:01:04", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("06:58:56", RelojSimuladorTurno.TiempoRestanteFormateado);

            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, true, 600.0));
            Assert.AreEqual("19:01:04", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("06:58:56", RelojSimuladorTurno.TiempoRestanteFormateado);

            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, double.NaN));
            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, double.PositiveInfinity));
            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, -15.0));
            Assert.AreEqual("19:01:04", RelojSimuladorTurno.HoraActualFormateada);

            Assert.IsTrue(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, 30000.0));
            Assert.AreEqual(28800.0, RelojSimuladorTurno.SegundosTranscurridos, 1e-9);
            Assert.AreEqual("02:00:00", RelojSimuladorTurno.HoraActualFormateada);
            Assert.AreEqual("00:00:00", RelojSimuladorTurno.TiempoRestanteFormateado);
            Assert.IsTrue(RelojSimuladorTurno.TurnoSuperoCierre);
            Assert.IsFalse(RelojSimuladorTurno.AvanzarTickSiTurnoActivo(true, false, 10.0));
        }

        [Test]
        public void EvaluarIngestaAutonoma_FlotaDosAMRsSinSoporteTercero_CalculaCapacidadRealDe2AMRs()
        {
            GameObject goEve = new GameObject("Test_EVE_2AMRs");
            objetosTemporales.Add(goEve);
            RobotManagerEVE eve = goEve.AddComponent<RobotManagerEVE>();
            eve.horaTurnoActual = 3.0f; // Restan 5.0h -> 2 AMRs = 400 pallets (sin restar el 2o AMR como reserva)
            eve.palletsEnColaWMS = 0;
            eve.flotaEscaladaDual = false;

            GameObject goR1 = new GameObject("Test_AMR_01");
            objetosTemporales.Add(goR1);
            RobotAMR r1 = goR1.AddComponent<RobotAMR>();
            r1.idRobot = "AMR-01";
            r1.estadoActual = RobotAMR.EstadoRobot.Inactivo;

            GameObject goR2 = new GameObject("Test_AMR_02");
            objetosTemporales.Add(goR2);
            RobotAMR r2 = goR2.AddComponent<RobotAMR>();
            r2.idRobot = "AMR-02";
            r2.estadoActual = RobotAMR.EstadoRobot.Inactivo;

            eve.robotAMR_01 = r1;
            eve.robotAMR_02 = r2;
            eve.robotAMR_03 = null; // Sin 3er AMR dedicado de soporte

            eve.ObtenerEstadoFlotaReal(out int amrsOperativosPiso, out bool tieneSoporteDedicado);
            Assert.AreEqual(2, amrsOperativosPiso);
            Assert.IsFalse(tieneSoporteDedicado);
            Assert.IsFalse(eve.admisionNuevosPedidosBloqueada);

            // Con 260 pallets (> 200 de 1 AMR, <= 400 de 2 AMRs) debe aprobar con capacidad real de 2 AMRs
            eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-2AMR-260", 260));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.EscaladoDual2AMRs, eve.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(400f, eve.UltimaEvaluacionSLA.Capacidad2AMRs, 1e-2f);
            Assert.AreEqual(260, eve.palletsEnColaWMS);
        }

        [Test]
        public void CapacidadDisponibleYRechazoJSON_ActualizaCupoYDetallePesoTiempoBuzon()
        {
            RobotManagerEVE eve = CrearEVE();
            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(3.0f); // Restan 5.0h -> techo 400 pallets
            eve.palletsEnColaWMS = 100;

            bool puede = eve.PuedeRecibirNuevosPedidos(out int cupoDisponible, out _);
            Assert.IsTrue(puede);
            Assert.AreEqual(300, cupoDisponible);

            eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-EXCESO", 320));
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Rechazado, eve.UltimaEvaluacionSLA.Decision);
            Assert.AreEqual(300, eve.UltimaEvaluacionSLA.CupoDisponibleActual);
            StringAssert.Contains("300 pallets", eve.UltimaEvaluacionSLA.FeedbackTecnico);
            StringAssert.Contains("850–1250 kg", eve.UltimaEvaluacionSLA.FeedbackTecnico);
            StringAssert.Contains("1000 kg", eve.UltimaEvaluacionSLA.FeedbackTecnico);
            StringAssert.Contains("AMRs en piso", eve.UltimaEvaluacionSLA.FeedbackTecnico);

            // Saturar la cola al 100% -> cupo = 0 y botón bloqueado
            eve.palletsEnColaWMS = 400;
            Assert.IsFalse(eve.PuedeRecibirNuevosPedidos(out int cupoSaturado, out _));
            Assert.AreEqual(0, cupoSaturado);

            // Bloqueo por caída de soporte aunque haya margen horario -> cupo = 0
            eve.palletsEnColaWMS = 50;
            eve.ActivarBloqueoAdmisionNuevosPedidos("Soporte perdido en prueba");
            Assert.IsFalse(eve.PuedeRecibirNuevosPedidos(out int cupoBloqueado, out _));
            Assert.AreEqual(0, cupoBloqueado);
        }

        [Test]
        public void EficienciaUnitaria_Lote30Base1000Kg_MantieneUnSoloAMR()
        {
            float eta1Ideal = RobotManagerEVE.CalcularEficienciaUnitariaAMR(1000f, 80f, 30, 5.0f, 1);
            Assert.AreEqual(100.0f, eta1Ideal, 1e-2f);
            Assert.GreaterOrEqual(eta1Ideal, RobotManagerEVE.UmbralEficienciaSegundoAMR);

            // Cargas ligeras (850 / 870 kg) respetan techo estricto de 100.0%
            float etaLigero850 = RobotManagerEVE.CalcularEficienciaUnitariaAMR(850f, 80f, 30, 5.0f, 1);
            float etaLigero870 = RobotManagerEVE.CalcularEficienciaUnitariaAMR(870f, 80f, 30, 5.0f, 1);
            Assert.AreEqual(100.0f, etaLigero850, 1e-3f);
            Assert.AreEqual(100.0f, etaLigero870, 1e-3f);

            // Override / modo degradado reduce eficiencia exactamente al 50%
            float etaDegradado = RobotManagerEVE.CalcularEficienciaUnitariaAMR(1000f, 80f, 30, 5.0f, 1, RobotAMR.EstadoRobot.Transportando, modoDegradacion: true);
            Assert.AreEqual(50.0f, etaDegradado, 1e-3f);

            RobotManagerEVE eve = CrearEVE();
            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(3.0f); // Restan 5.0h
            Manifiesto lote30Base = new Manifiesto
            {
                id_lote = "LOTE-30-BASE",
                total_pallets = 30,
                peso_total_kg = 30000f,
                pedidos = new List<Pedido>
                {
                    new Pedido { id_pallet = "PAL-1000", nombre_producto = "Base", codigo_sku = "SKU-1000", peso_kg = 1000f, coordenada_bodega = "P1-C1-F1" }
                }
            };

            eve.EvaluarIngestaAutonoma(lote30Base);
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.Aprobado1AMR, eve.UltimaEvaluacionSLA.Decision);
            Assert.IsFalse(eve.flotaEscaladaDual);
            Assert.GreaterOrEqual(eve.EficienciaUnitariaAMR01, RobotManagerEVE.UmbralEficienciaSegundoAMR);
        }

        [Test]
        public void TelemetriaAMR_FormatearETASeguro_NuncaRetornaInfinitoNiNaN()
        {
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(float.PositiveInfinity, 3.5f, false));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(float.NegativeInfinity, 3.5f, false));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(float.NaN, 3.5f, false));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(25f, float.NaN, false));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(25f, 1.5f, true));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(25f, 0.05f, false));
            Assert.AreEqual(TelemetriaAMR.TextoETACalculando, TelemetriaAMR.FormatearETASeguro(0.2f, 1.5f, false));

            string valido = TelemetriaAMR.FormatearETASeguro(45f, 1.5f, false);
            Assert.AreEqual("ETA: 30.0s", valido);
            StringAssert.DoesNotContain("Infinito", valido);
            StringAssert.DoesNotContain("NaN", valido);
        }

        [Test]
        public void EficienciaUnitaria_Lote260OCargaPesada1250Kg_ActivaSegundoAMR()
        {
            float eta1Lote260 = RobotManagerEVE.CalcularEficienciaUnitariaAMR(1000f, 80f, 260, 5.0f, 1);
            Assert.Less(eta1Lote260, RobotManagerEVE.UmbralEficienciaSegundoAMR);

            float eta1Pesado1250 = RobotManagerEVE.CalcularEficienciaUnitariaAMR(1250f, 80f, 30, 5.0f, 1);
            Assert.Less(eta1Pesado1250, RobotManagerEVE.UmbralEficienciaSegundoAMR);

            RobotManagerEVE eve = CrearEVE();
            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(3.0f); // Restan 5.0h
            Manifiesto lote30Pesado = new Manifiesto
            {
                id_lote = "LOTE-30-PESADO",
                total_pallets = 30,
                peso_total_kg = 37500f,
                pedidos = new List<Pedido>
                {
                    new Pedido { id_pallet = "PAL-1250", nombre_producto = "Barril Pesado", codigo_sku = "OKT-BAR-50L", peso_kg = 1250f, coordenada_bodega = "P2-C4-F1" }
                }
            };

            eve.EvaluarIngestaAutonoma(lote30Pesado);
            Assert.AreEqual(RobotManagerEVE.DecisionAdmisionSLA.EscaladoDual2AMRs, eve.UltimaEvaluacionSLA.Decision);
            Assert.IsTrue(eve.flotaEscaladaDual);
            Assert.AreEqual(2, eve.UltimaEvaluacionSLA.AMRRequeridos);
        }

        [Test]
        public void EficienciaPromedioDual_Bajo78PorCiento_GeneraDiagnosticoBuzonSinDetenerTurno()
        {
            RobotManagerEVE eve = CrearEVE();
            RelojSimuladorTurno.EstablecerHoraSimuladorInterno(3.0f); // Restan 5.0h
            eve.turnoIniciado = true;
            eve.ReiniciarContadoresIncidentesTurno();
            eve.palletsEnColaWMS = 60;
            eve.flotaEscaladaDual = true;

            GameObject goR1 = new GameObject("Test_AMR_01_Dual");
            objetosTemporales.Add(goR1);
            RobotAMR r1 = goR1.AddComponent<RobotAMR>();
            r1.idRobot = "AMR-01";
            r1.bateriaActual = 30.0f;
            r1.estadoActual = RobotAMR.EstadoRobot.Transportando;

            GameObject goR2 = new GameObject("Test_AMR_02_Dual");
            objetosTemporales.Add(goR2);
            RobotAMR r2 = goR2.AddComponent<RobotAMR>();
            r2.idRobot = "AMR-02";
            r2.bateriaActual = 30.0f;
            r2.estadoActual = RobotAMR.EstadoRobot.Transportando;

            eve.robotAMR_01 = r1;
            eve.robotAMR_02 = r2;

            // Inyectar pedido pesado en ambos AMRs (~1250 kg con 30% SoC -> ~60.35% < 78.0%)
            eve.EvaluarEficienciaOperativaYEscalamiento(new Pedido { id_pallet = "PAL-P1", peso_kg = 1250f }, permitirDespachoInmediato: false);

            Assert.Less(eve.EficienciaPromedioDual, RobotManagerEVE.UmbralEficienciaDualBuzon);
            Assert.IsTrue(eve.AlertaEficienciaDualActiva);
            Assert.IsNotEmpty(eve.UltimoDiagnosticoEficienciaDual);
            StringAssert.Contains("AMR-01", eve.UltimoDiagnosticoEficienciaDual);
            StringAssert.Contains("AMR-02", eve.UltimoDiagnosticoEficienciaDual);
            StringAssert.Contains("AMR-03", eve.UltimoDiagnosticoEficienciaDual);
            Assert.IsFalse(eve.operacionDetenidaPorSeguridad);
            Assert.IsFalse(eve.turnoEnFaseCritica);
            Assert.IsTrue(eve.turnoIniciado);
        }

        [Test]
        public void BuzonSupervisorYContingenciaMuelle_NumeraHistorialYNoSobrescribeConPopupSLA()
        {
            DashboardUI uiPrevia = DashboardUI.Instance;
            ModuloDespachoTMS tmsPrevio = ModuloDespachoTMS.Instance;
            var propUi = typeof(DashboardUI).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var propTms = typeof(ModuloDespachoTMS).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            try
            {
                GameObject goUi = new GameObject("Test_DashboardUI_Buzon");
                objetosTemporales.Add(goUi);
                DashboardUI ui = goUi.AddComponent<DashboardUI>();
                propUi?.SetValue(null, ui);

                GameObject goTms = new GameObject("Test_TMS_Buzon");
                objetosTemporales.Add(goTms);
                ModuloDespachoTMS tms = goTms.AddComponent<ModuloDespachoTMS>();
                propTms?.SetValue(null, tms);
                tms.muelleCamion01 = new GameObject("Test_M1").transform;
                tms.muelleCamion02 = new GameObject("Test_M2").transform;
                tms.muelleCamion03 = new GameObject("Test_M3").transform;
                objetosTemporales.Add(tms.muelleCamion01.gameObject);
                objetosTemporales.Add(tms.muelleCamion02.gameObject);
                objetosTemporales.Add(tms.muelleCamion03.gameObject);

                tms.ClausurarMuellePorIncidenteAMR(1, "Obstrucción prueba muelle 01", out int sig1);
                Assert.AreEqual(2, sig1);
                Assert.AreEqual(1, ui.HistorialFeedbackSupervisor.Count);
                StringAssert.Contains("Muelle_Camion_02", ui.HistorialFeedbackSupervisor[0]);

                tms.ClausurarMuellePorIncidenteAMR(2, "Obstrucción prueba muelle 02", out int sig2);
                Assert.AreEqual(3, sig2);
                Assert.AreEqual(2, ui.HistorialFeedbackSupervisor.Count);

                tms.ClausurarMuellePorIncidenteAMR(3, "Obstrucción prueba muelle 03", out int sig3);
                Assert.AreEqual(0, sig3);
                Assert.AreEqual(3, ui.HistorialFeedbackSupervisor.Count);
                StringAssert.Contains("BLOQUEO TOTAL DE MUELLES", ui.HistorialFeedbackSupervisor[2]);

                string listado = ui.ConstruirTextoHistorialBuzonNumerado();
                StringAssert.Contains("#1", listado);
                StringAssert.Contains("#2", listado);
                StringAssert.Contains("#3", listado);

                ui.AbrirVentanaBuzonSupervisor();
                Assert.IsNotNull(ui.panelBuzonSupervisor);
                Assert.IsTrue(ui.panelBuzonSupervisor.activeSelf);
                Assert.IsNotNull(ui.panelBuzonSupervisor.transform.Find("Scroll_Buzon_Supervisor/Viewport/Content_Buzon_Supervisor/Txt_Listado_Buzon_Supervisor"));

                ui.MostrarPopUpEjecutivoSLA("AVISO TRANSITORIO", "No debe cerrar ni sobrescribir el Buzón", 2.0f);
                Assert.IsTrue(ui.panelBuzonSupervisor.activeSelf);
                object listadoComp = ui.GetType().GetField("txtListadoBuzonSupervisor")?.GetValue(ui);
                Assert.IsNotNull(listadoComp);
                string textoRenderizado = listadoComp.GetType().GetProperty("text")?.GetValue(listadoComp) as string;
                StringAssert.Contains("#1", textoRenderizado);
                StringAssert.Contains("#3", textoRenderizado);
            }
            finally
            {
                propUi?.SetValue(null, uiPrevia);
                propTms?.SetValue(null, tmsPrevio);
            }
        }

        [Test]
        public void CierreYReinicioDeTurno_BloqueaDespachosMuestraPopupSeCierraTurnoEIluminaBotonReiniciar()
        {
            DashboardUI uiPrevia = DashboardUI.Instance;
            ModuloDespachoTMS tmsPrevio = ModuloDespachoTMS.Instance;
            var propUi = typeof(DashboardUI).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var propTms = typeof(ModuloDespachoTMS).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            try
            {
                GameObject goUi = CrearObjetoTemporal("Test_DashboardUI_Cierre");
                DashboardUI ui = goUi.AddComponent<DashboardUI>();
                propUi?.SetValue(null, ui);
                ui.ConfigurarBarraOperativaInferior();

                GameObject goTms = CrearObjetoTemporal("Test_TMS_Cierre");
                ModuloDespachoTMS tms = goTms.AddComponent<ModuloDespachoTMS>();
                propTms?.SetValue(null, tms);
                tms.muelleCamion01 = CrearObjetoTemporal("M1_Cierre").transform;
                tms.muelleCamion02 = CrearObjetoTemporal("M2_Cierre").transform;
                tms.muelleCamion03 = CrearObjetoTemporal("M3_Cierre").transform;

                RobotManagerEVE eve = CrearEVE();
                eve.gestorTMS = tms;
                RobotAMR r1 = CrearObjetoTemporal("AMR01_Cierre").AddComponent<RobotAMR>();
                r1.idRobot = "AMR-01";
                r1.bateriaActual = 45.0f;
                r1.estadoActual = RobotAMR.EstadoRobot.Inactivo;
                eve.robotAMR_01 = r1;

                eve.IniciarTurno();
                ui.ConfigurarBarraOperativaInferior();
                Assert.IsFalse(eve.turnoCerrado);
                Assert.IsFalse(ui.IluminacionReiniciarTurnoActiva);

                eve.EvaluarIngestaAutonoma(CrearManifiestoLote("LOTE-CIERRE", 10));
                Assert.AreEqual(10, eve.palletsEnColaWMS);

                // Al llegar al cierre de jornada (02:00:00 / 8h), EVE cierra turno, detiene admisión, abre pop-up y alumbra botón
                RelojSimuladorTurno.EstablecerHoraSimuladorInterno(8.0f);
                Assert.IsTrue(eve.EvaluarCierreTurnoSiCorresponde());
                Assert.IsTrue(eve.turnoCerrado);
                Assert.IsFalse(eve.turnoIniciado);
                Assert.IsFalse(eve.PuedeRecibirNuevosPedidos(out _, out _));
                Assert.IsTrue(ui.PopupCierreTurnoActivo);
                object tituloComp = typeof(DashboardUI).GetField("txtTituloPopupSLA")?.GetValue(ui);
                Assert.IsNotNull(tituloComp);
                string tituloPopup = tituloComp.GetType().GetProperty("text")?.GetValue(tituloComp) as string;
                StringAssert.Contains("Se cierra turno", tituloPopup);
                Assert.IsTrue(ui.IluminacionReiniciarTurnoActiva);
                Assert.IsNotNull(ui.imgHaloReiniciarTurno);
                Assert.IsTrue(ui.imgHaloReiniciarTurno.gameObject.activeSelf);

                // Pulsar Reiniciar Turno limpia estado, apaga iluminación/pop-up y restaura reloj y flota
                ui.OnClickReiniciarTurno();
                Assert.IsFalse(eve.turnoCerrado);
                Assert.IsTrue(eve.turnoIniciado);
                Assert.AreEqual(0, eve.palletsEnColaWMS);
                Assert.IsFalse(ui.PopupCierreTurnoActivo);
                Assert.IsFalse(ui.IluminacionReiniciarTurnoActiva);
                Assert.IsFalse(ui.imgHaloReiniciarTurno.gameObject.activeSelf);
                Assert.AreEqual(80.0f, r1.bateriaActual, 1e-3f);
                Assert.AreEqual(RobotAMR.EstadoRobot.Inactivo, r1.estadoActual);
                Assert.AreEqual("18:00:00", RelojSimuladorTurno.HoraActualFormateada);
            }
            finally
            {
                propUi?.SetValue(null, uiPrevia);
                propTms?.SetValue(null, tmsPrevio);
            }
        }

        private GameObject CrearObjetoTemporal(string nombre)
        {
            GameObject go = new GameObject(nombre);
            objetosTemporales.Add(go);
            return go;
        }
    }
}
