using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProyectoBodega.Tests.EditMode
{
    public class TermodinamicaLiFePO4Tests
    {
        private readonly List<GameObject> objetosTemporales = new List<GameObject>();
        private RobotAMR amr;

        [SetUp]
        public void SetUp()
        {
            GameObject goAmr = CrearObjetoTemporal("Test_RobotAMR_Termo");
            amr = goAmr.AddComponent<RobotAMR>();
            amr.idRobot = "AMR-TEST";
            amr.bateriaActual = 80.0f;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = objetosTemporales.Count - 1; i >= 0; i--)
            {
                if (objetosTemporales[i] != null)
                {
                    Object.DestroyImmediate(objetosTemporales[i]);
                }
            }
            objetosTemporales.Clear();
        }

        private GameObject CrearObjetoTemporal(string nombre)
        {
            GameObject go = new GameObject(nombre);
            objetosTemporales.Add(go);
            return go;
        }

        [Test]
        public void EvaluarViabilidadLiFePO4_Frontera850KgVs920Kg_DiferenciaViableDeInviable()
        {
            const float bateriaInicial = 21.6f;
            const float distanciaIda = 100f;
            const float distanciaVuelta = 100f;

            bool viable850 = RobotAMR.EvaluarViabilidadLiFePO4(
                bateriaInicial,
                distanciaIda,
                distanciaVuelta,
                850f,
                amr.coeficienteVacio,
                amr.coeficienteCarga,
                out float restante850);

            bool viable920 = RobotAMR.EvaluarViabilidadLiFePO4(
                bateriaInicial,
                distanciaIda,
                distanciaVuelta,
                920f,
                amr.coeficienteVacio,
                amr.coeficienteCarga,
                out float restante920);

            Assert.IsTrue(viable850, "Carga nominal 850 kg debe resultar viable en la frontera.");
            Assert.AreEqual(20.03695f, restante850, 1e-3f);
            Assert.GreaterOrEqual(restante850, RobotAMR.UmbralSeguridadLiFePO4);

            Assert.IsFalse(viable920, "Carga crítica 920 kg debe violar el umbral de seguridad 20.0%.");
            Assert.AreEqual(19.92264f, restante920, 1e-3f);
            Assert.Less(restante920, RobotAMR.UmbralSeguridadLiFePO4);
        }

        [Test]
        public void CalcularConsumoTramo_VacioYCargado_RespetaFormulaExacta()
        {
            float gastoVacio = RobotAMR.CalcularConsumoTramo(100f, 850f, amr.coeficienteVacio, amr.coeficienteCarga, false);
            float gastoCargado920 = RobotAMR.CalcularConsumoTramo(100f, 920f, amr.coeficienteVacio, amr.coeficienteCarga, true, 1.15f);

            Assert.AreEqual(0.175f, gastoVacio, 1e-5f);
            Assert.AreEqual(1.50236f, gastoCargado920, 1e-5f);
        }

        [Test]
        public void EjecutarOverride_AplicaPenalizacionTermicaMas30PorCiento()
        {
            float coefOriginal = amr.coeficienteCarga;
            amr.estadoActual = RobotAMR.EstadoRobot.Interrumpido;
            amr.enPausaPreventivaBateria20 = true;
            amr.estadoPreInterrupcionBateria = RobotAMR.EstadoRobot.Transportando;

            amr.EjecutarOverride();

            Assert.AreEqual(coefOriginal * 1.3f, amr.coeficienteCarga, 1e-6f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Transportando, amr.estadoActual);
            Assert.IsFalse(amr.enPausaPreventivaBateria20);
        }

        [Test]
        public void AplicarConsumoYVerificarCorteBMS_AlCeroPorCiento_InmovilizaUnidadEnInterrumpido()
        {
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr.bateriaActual = 0.5f;

            LogAssert.Expect(LogType.Error, "[FALLO CATASTRÓFICO] Batería agotada al 0% en pasillo. NavMeshAgent inmovilizado.");
            bool corteDisparado = amr.AplicarConsumoYVerificarCorteBMS(1.2f);

            Assert.IsTrue(corteDisparado);
            Assert.AreEqual(0f, amr.bateriaActual, 1e-5f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);
        }

        [Test]
        public void ForzarApagadoCriticoBMS_ApagaUnidadYRetornaPedidoHuerfano()
        {
            GameObject destino = CrearObjetoTemporal("Rack_Destino_Test");
            Pedido pedido = new Pedido
            {
                id_pallet = "PAL-8493",
                nombre_producto = "Barril 50L",
                codigo_sku = "OKT-BAR-50L",
                peso_kg = 920f,
                coordenada_bodega = "P2-C4-F1"
            };

            amr.AsignarMision(pedido, destino.transform, 0);
            Assert.AreEqual(RobotAMR.EstadoRobot.En_Transito, amr.estadoActual);

            Pedido huerfano = amr.ForzarApagadoCriticoBMS();

            Assert.IsNotNull(huerfano);
            Assert.AreEqual("PAL-8493", huerfano.id_pallet);
            Assert.AreEqual(2.4f, amr.bateriaActual, 1e-3f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Apagado, amr.estadoActual);
            Assert.IsNull(amr.PedidoActual);
        }

        [Test]
        public void EvaluarViabilidadEntregaEscalon6_MuelleCerradoYEnergiaInsuficiente_EscalaA3()
        {
            bool viableAbierto = RobotAMR.EvaluarViabilidadEntregaEscalon6(
                5.6f,
                muelleAbierto: true,
                distanciaRestanteMuelle: 20f,
                pesoKg: 850f,
                coefVacio: amr.coeficienteVacio,
                coefCarga: amr.coeficienteCarga,
                out float restanteAbierto,
                out string motivoAbierto);

            Assert.IsTrue(viableAbierto, motivoAbierto);
            Assert.Greater(restanteAbierto, RobotAMR.UmbralApagadoInminente3 + RobotAMR.ReservaSeguridadEscalon6);

            bool viableCerrado = RobotAMR.EvaluarViabilidadEntregaEscalon6(
                5.6f,
                muelleAbierto: false,
                distanciaRestanteMuelle: 20f,
                pesoKg: 850f,
                coefVacio: amr.coeficienteVacio,
                coefCarga: amr.coeficienteCarga,
                out _,
                out string motivoCerrado);

            Assert.IsFalse(viableCerrado);
            StringAssert.Contains("cerrado", motivoCerrado.ToLowerInvariant());

            bool viableSinEnergia = RobotAMR.EvaluarViabilidadEntregaEscalon6(
                3.6f,
                muelleAbierto: true,
                distanciaRestanteMuelle: 40f,
                pesoKg: 920f,
                coefVacio: amr.coeficienteVacio,
                coefCarga: amr.coeficienteCarga,
                out float restanteBajo,
                out _);

            Assert.IsFalse(viableSinEnergia);
            Assert.LessOrEqual(restanteBajo, RobotAMR.UmbralApagadoInminente3 + RobotAMR.ReservaSeguridadEscalon6);
        }

        [Test]
        public void ForzarApagadoCriticoBMS_ConPalletFisico_SueltaPalletAlFrenteSinDestruirlo()
        {
            GameObject destino = CrearObjetoTemporal("Rack_Suelta_Test");
            Pedido pedido = new Pedido
            {
                id_pallet = "PAL-DROP-01",
                nombre_producto = "Aguila Lata",
                codigo_sku = "AGU-RUB-330",
                peso_kg = 850f,
                coordenada_bodega = "P1-C1-F1"
            };

            amr.transform.position = new Vector3(10f, 0f, -20f);
            amr.transform.rotation = Quaternion.identity;
            amr.AsignarMision(pedido, destino.transform, 0);

            GameObject palletMontado = new GameObject("Pallet_PAL-DROP-01");
            objetosTemporales.Add(palletMontado);
            palletMontado.transform.SetParent(amr.transform, false);
            palletMontado.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            // Inyectar referencia privada serializada/interna vía reflexión solo para simular carga ya extraída
            var campoPallet = typeof(RobotAMR).GetField("palletActualGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(campoPallet);
            campoPallet.SetValue(amr, palletMontado);

            amr.estadoActual = RobotAMR.EstadoRobot.Transportando;
            Pedido huerfano = amr.ForzarApagadoCriticoBMS(out RobotAMR.PalletEnPasillo paquete);

            Assert.IsNotNull(huerfano);
            Assert.AreEqual("PAL-DROP-01", huerfano.id_pallet);
            Assert.IsNotNull(paquete);
            Assert.IsTrue(palletMontado != null, "El GameObject del pallet físico NO debe destruirse al apagarse el AMR.");
            Assert.AreSame(palletMontado, paquete.palletGO);
            Assert.IsNull(palletMontado.transform.parent);
            Assert.AreEqual(10f, palletMontado.transform.position.x, 0.25f);
            Assert.AreEqual(-20f + RobotAMR.DistanciaSueltaFrontalPallet, palletMontado.transform.position.z, 0.25f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Apagado, amr.estadoActual);
            Assert.AreEqual(RobotAMR.TipoFallaAMR.DescargaCritica3, amr.tipoFallaActual);
        }

        [Test]
        public void BifurcacionDanadoVsDescargadoYContinuidad_CierreSoloConCeroAMRs()
        {
            GameObject goEve = CrearObjetoTemporal("Test_EVE_Continuidad");
            RobotManagerEVE eve = goEve.AddComponent<RobotManagerEVE>();

            GameObject goAmr2 = CrearObjetoTemporal("Test_RobotAMR_02");
            RobotAMR amr2 = goAmr2.AddComponent<RobotAMR>();
            amr2.idRobot = "AMR-02";
            amr2.bateriaActual = 80f;

            GameObject goAmr3 = CrearObjetoTemporal("Test_RobotAMR_03");
            RobotAMR amr3 = goAmr3.AddComponent<RobotAMR>();
            amr3.idRobot = "AMR-03";
            amr3.bateriaActual = 80f;

            eve.robotAMR_01 = amr;
            eve.robotAMR_02 = amr2;
            eve.robotAMR_03 = amr3;
            eve.ReiniciarContadoresIncidentesTurno();
            eve.turnoIniciado = true;

            // Incidente 1 (Rama B: Hardware Dañado en AMR-01) -> bloquea admisión de pedidos nuevos, NO detiene el turno
            eve.ReportarFallaHardwareAMR(amr);
            Assert.AreEqual(RobotAMR.EstadoRobot.FueraDeServicio_Danado, amr.estadoActual);
            Assert.IsFalse(amr.EsRecuperableEnTurno);
            Assert.AreEqual(1, eve.contadorFallasHardwareTurno);
            Assert.IsTrue(eve.admisionNuevosPedidosBloqueada);
            Assert.IsFalse(eve.turnoEnFaseCritica);
            Assert.IsFalse(eve.operacionDetenidaPorSeguridad);
            Assert.IsFalse(eve.SolicitarAutorizacionReincorporacion(amr));

            // AMR dañado rechaza nuevas asignaciones y recarga
            GameObject rack = CrearObjetoTemporal("Rack_Test_HW");
            amr.AsignarMision(new Pedido { id_pallet = "PAL-NO", peso_kg = 850f, coordenada_bodega = "P1-C1-F1" }, rack.transform, 0);
            amr.EnviarACargaInductiva(Vector3.zero);
            Assert.AreEqual(RobotAMR.EstadoRobot.FueraDeServicio_Danado, amr.estadoActual);

            // Incidente 2 (Descarga <=3% en AMR-02, recuperable vía Pad, quedando AMR-03 operativo) ->
            // Ley de Continuidad: sigue sin detener el turno activo y mantiene bloqueados pedidos nuevos
            amr2.transform.position = new Vector3(10f, 0f, -20f);
            amr2.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            eve.EjecutarColapsoYRescateInmediato(amr2);

            Assert.AreEqual(1, eve.contadorDescargasCriticasTurno);
            Assert.AreEqual(2, eve.TotalIncidentesCriticosTurno);
            Assert.IsTrue(eve.admisionNuevosPedidosBloqueada);
            Assert.IsFalse(eve.turnoEnFaseCritica);
            Assert.IsFalse(eve.operacionDetenidaPorSeguridad);
            Assert.IsTrue(eve.SolicitarAutorizacionReincorporacion(amr2));

            // Incidente 3 y Cierre Total: cuando los 3 AMRs quedan dañados/irrecuperables (0 AMRs operativos/recuperables)
            eve.ReportarFallaHardwareAMR(amr2);
            Assert.IsFalse(eve.operacionDetenidaPorSeguridad, "Aún queda AMR-03 operativo; el turno debe continuar.");
            eve.ReportarFallaHardwareAMR(amr3);

            Assert.AreEqual(0, eve.ContarAMRsRecuperablesEnTurno());
            Assert.IsTrue(eve.turnoEnFaseCritica);
            Assert.IsTrue(eve.operacionDetenidaPorSeguridad);
            Assert.IsTrue(eve.turnoCerrado);
            Assert.IsFalse(eve.SolicitarAutorizacionReincorporacion(amr2));
        }

        [Test]
        public void AbortarMisionPorSeguridad_ConPalletEnPasillo_DepositaPalletSinDestruirlo()
        {
            GameObject destino = CrearObjetoTemporal("Rack_Aborto_Test");
            Pedido pedido = new Pedido
            {
                id_pallet = "PAL-ABORT-01",
                nombre_producto = "Aguila Lata",
                codigo_sku = "AGU-RUB-330",
                peso_kg = 850f,
                coordenada_bodega = "P1-C1-F1"
            };

            amr.transform.position = new Vector3(8f, 0f, -20f);
            amr.transform.rotation = Quaternion.identity;
            amr.AsignarMision(pedido, destino.transform, 0);

            GameObject palletMontado = new GameObject("Pallet_PAL-ABORT-01");
            objetosTemporales.Add(palletMontado);
            palletMontado.transform.SetParent(amr.transform, false);
            palletMontado.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            var campoPallet = typeof(RobotAMR).GetField("palletActualGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(campoPallet);
            campoPallet.SetValue(amr, palletMontado);

            amr.estadoActual = RobotAMR.EstadoRobot.Transportando;
            amr.bateriaActual = 18.5f;
            amr.AbortarMisionPorSeguridad();

            Assert.IsTrue(palletMontado != null, "Al abortar con carga en pasillo no debe destruirse el GameObject del pallet.");
            Assert.IsNull(palletMontado.transform.parent);
            Assert.AreEqual(-20f + RobotAMR.DistanciaSueltaFrontalPallet, palletMontado.transform.position.z, 0.25f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Inactivo, amr.estadoActual);
        }

        [Test]
        public void RecargaInductivaDosHoras_SinAceleracionArtificial_ConservaFactorTurno()
        {
            Assert.AreEqual(7200f, RobotAMR.DuracionRecargaRealSegundos, 1e-3f);
            Assert.AreEqual(7200f, amr.tiempoRecargaRealSegundos, 1e-3f);
            Assert.AreEqual(60f / 7200f, amr.ObtenerTasaRecargaRealPorSegundo(), 1e-6f);
            Assert.AreEqual(80f, 20f + (amr.ObtenerTasaRecargaRealPorSegundo() * 7200f), 1e-3f);
            Assert.AreEqual(160f, RobotAMR.FactorCompresionTurno, 1e-3f);
        }

        [Test]
        public void VarianzaPesoIndustrial_Rango850A1250_ImpactaVelocidadYConsumo()
        {
            Assert.AreEqual(1000f, Pedido.PesoReferenciaBaseKg, 1e-3f);

            bool vioLigero = false;
            bool vioPesado = false;
            for (int i = 0; i < 12; i++)
            {
                float peso = RobotManagerEVE.GenerarPesoIndustrialPedido(i, 37);
                Assert.GreaterOrEqual(peso, Pedido.PesoMinimoIndustrialKg);
                Assert.LessOrEqual(peso, Pedido.PesoMaximoIndustrialKg);
                if (peso < 950f) vioLigero = true;
                if (peso > 1100f) vioPesado = true;
            }
            Assert.IsTrue(vioLigero);
            Assert.IsTrue(vioPesado);

            float vel850 = amr.ObtenerVelocidadTransporteParaPeso(850f);
            float vel1000 = amr.ObtenerVelocidadTransporteParaPeso(1000f);
            float vel1250 = amr.ObtenerVelocidadTransporteParaPeso(1250f);
            Assert.Greater(vel850, vel1000);
            Assert.Greater(vel1000, vel1250);

            float gasto850 = RobotAMR.CalcularConsumoTramo(100f, 850f, amr.coeficienteVacio, amr.coeficienteCarga, true, 1.0f);
            float gasto1000 = RobotAMR.CalcularConsumoTramo(100f, 1000f, amr.coeficienteVacio, amr.coeficienteCarga, true, 1.0f);
            float gasto1250 = RobotAMR.CalcularConsumoTramo(100f, 1250f, amr.coeficienteVacio, amr.coeficienteCarga, true, 1.0f);
            Assert.Greater(gasto1000, gasto850);
            Assert.Greater(gasto1250, gasto1000);
        }

        [Test]
        public void PotenciaReducidaBajo20PorCiento_ReduceVelocidadYAceleracionAl50PorCiento()
        {
            amr.bateriaActual = 50.0f;
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            Assert.IsFalse(amr.EnPotenciaReducidaBateria);
            float velSanaTransito = amr.ObtenerVelocidadNominal();
            float accSanaTransito = amr.ObtenerAceleracionNominal();

            amr.bateriaActual = 19.5f;
            Assert.IsTrue(amr.EnPotenciaReducidaBateria);
            Assert.AreEqual(velSanaTransito * 0.5f, amr.ObtenerVelocidadNominal(), 1e-4f);
            Assert.AreEqual(accSanaTransito * 0.5f, amr.ObtenerAceleracionNominal(), 1e-4f);

            // También aplica en Recargando rumbo al Pad
            amr.estadoActual = RobotAMR.EstadoRobot.Recargando;
            Assert.AreEqual(velSanaTransito * 0.5f, amr.ObtenerVelocidadNominal(), 1e-4f);
            Assert.AreEqual(accSanaTransito * 0.5f, amr.ObtenerAceleracionNominal(), 1e-4f);

            // Y en Transportando con pedido base (1000 kg -> 0.65 m/s * 0.5 = 0.325 m/s bajo tope real 1.25 m/s)
            GameObject destino = CrearObjetoTemporal("Rack_Potencia_Test");
            amr.bateriaActual = 50.0f;
            amr.estadoActual = RobotAMR.EstadoRobot.Inactivo;
            amr.AsignarMision(new Pedido { id_pallet = "PAL-P20", peso_kg = 1000f, coordenada_bodega = "P1-C1-F1" }, destino.transform, 0);
            amr.estadoActual = RobotAMR.EstadoRobot.Transportando;
            float velSanaCarga = amr.ObtenerVelocidadNominal();
            Assert.AreEqual(0.65f, velSanaCarga, 1e-3f);

            amr.bateriaActual = 19.0f;
            Assert.AreEqual(0.325f, amr.ObtenerVelocidadNominal(), 1e-3f);
            Assert.AreEqual(8.0f, amr.ObtenerAceleracionNominal(), 1e-3f);
        }

        [Test]
        public void CalibracionKMF1500PCB_6000Wh_Velocidad125MsYElevacion3Kw_RespetaConversionWhAPorcentaje()
        {
            Assert.AreEqual(1.25f, RobotAMR.VelocidadMaximaNominalKMF, 1e-4f);
            Assert.AreEqual(1.25f, amr.VelocidadBase, 1e-4f);
            Assert.AreEqual(6000f, RobotAMR.CapacidadPackWh, 1e-3f);
            Assert.AreEqual(0.75f, RobotAMR.PotenciaMediaOperativaKw, 1e-3f);
            Assert.AreEqual(2.6f, RobotAMR.PotenciaTraccionNominalKw, 1e-3f);
            Assert.AreEqual(3.0f, RobotAMR.PotenciaElevacionNominalKw, 1e-3f);

            Assert.AreEqual(1.0f, RobotAMR.ConvertirWhAPorcentajeSoC(60.0f), 1e-5f);
            Assert.AreEqual(60.0f, RobotAMR.ConvertirPorcentajeSoCAWh(1.0f), 1e-4f);

            float gastoWhCargado920 = RobotAMR.CalcularConsumoTramoWh(100f, 920f, amr.coeficienteVacio, amr.coeficienteCarga, true, 1.15f);
            Assert.AreEqual(90.1416f, gastoWhCargado920, 1e-3f);

            // Elevación nominal 3.0 kW en pedido base 1,000 kg durante 12 s = 10 Wh = 0.166667% SoC
            float elevacionWh = RobotAMR.CalcularConsumoElevacionWh(12.0f, 1000f);
            Assert.AreEqual(10.0f, elevacionWh, 1e-3f);
            Assert.AreEqual(10.0f / 60.0f, RobotAMR.CalcularConsumoElevacionPct(12.0f, 1000f), 1e-5f);
        }

        [Test]
        public void FisicaRampas_SubidaConCargaPenalizaVelocidadYAumentaConsumo_BajadaControlada()
        {
            Assert.AreEqual(1.0f, RobotAMR.CalcularFactorVelocidadRampa(false, true, true, 1000f), 1e-4f);
            Assert.AreEqual(1.0f, RobotAMR.CalcularFactorEsfuerzoConsumoRampa(false, true, true, 1000f), 1e-4f);

            float velSub850 = RobotAMR.CalcularFactorVelocidadRampa(true, true, true, 850f);
            float velSub1000 = RobotAMR.CalcularFactorVelocidadRampa(true, true, true, 1000f);
            float velSub1250 = RobotAMR.CalcularFactorVelocidadRampa(true, true, true, 1250f);
            Assert.AreEqual(0.78f, velSub850, 1e-3f);
            Assert.AreEqual(0.72f, velSub1000, 1e-3f);
            Assert.AreEqual(0.62f, velSub1250, 1e-3f);

            float consSub850 = RobotAMR.CalcularFactorEsfuerzoConsumoRampa(true, true, true, 850f);
            float consSub1000 = RobotAMR.CalcularFactorEsfuerzoConsumoRampa(true, true, true, 1000f);
            float consSub1250 = RobotAMR.CalcularFactorEsfuerzoConsumoRampa(true, true, true, 1250f);
            Assert.AreEqual(1.45f, consSub850, 1e-3f);
            Assert.AreEqual(1.60f, consSub1000, 1e-3f);
            Assert.AreEqual(1.85f, consSub1250, 1e-3f);

            // Bajada moderada (sin carga 0.65f, menor esfuerzo 0.80f)
            Assert.AreEqual(RobotAMR.FactorVelocidadDescensoRampa, RobotAMR.CalcularFactorVelocidadRampa(true, false, false, 1000f), 1e-3f);
            Assert.AreEqual(0.80f, RobotAMR.CalcularFactorEsfuerzoConsumoRampa(true, false, false, 1000f), 1e-3f);
        }
    }
}
