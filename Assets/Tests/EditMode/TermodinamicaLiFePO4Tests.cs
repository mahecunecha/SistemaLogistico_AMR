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
    }
}
