using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProyectoBodega.Tests.EditMode
{
    public class RobotAMRFsmLiDARTests
    {
        private readonly List<GameObject> objetosTemporales = new List<GameObject>();
        private RobotAMR amr;
        private Transform destinoValido;

        [SetUp]
        public void SetUp()
        {
            GameObject goAmr = CrearObjetoTemporal("Test_RobotAMR_FSM");
            amr = goAmr.AddComponent<RobotAMR>();
            amr.idRobot = "AMR-FSM";
            amr.bateriaActual = 80.0f;

            GameObject goDestino = CrearObjetoTemporal("Test_Rack_FSM");
            goDestino.transform.position = new Vector3(10f, 0f, -20f);
            destinoValido = goDestino.transform;
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

        private static Pedido CrearPedido(string id = "PAL-FSM-01")
        {
            return new Pedido
            {
                id_pallet = id,
                nombre_producto = "Aguila Lata",
                codigo_sku = "AGU-RUB-330",
                peso_kg = 850f,
                coordenada_bodega = "P1-C1-F1"
            };
        }

        [Test]
        public void CicloFSMYParadaLiDAR_TransicionaYReanudaLimpiamente()
        {
            var historialEstados = new List<RobotAMR.EstadoRobot>();
            amr.OnEstadoCambiado += historialEstados.Add;

            Assert.AreEqual(RobotAMR.EstadoRobot.Inactivo, amr.estadoActual);

            amr.AsignarMision(CrearPedido(), destinoValido, 0);
            Assert.AreEqual(RobotAMR.EstadoRobot.En_Transito, amr.estadoActual);

            bool paradaActivada = amr.ActivarParadaSeguridadLiDAR();
            Assert.IsTrue(paradaActivada);
            Assert.IsTrue(amr.EnParadaSeguridadLiDAR);
            Assert.AreEqual(RobotAMR.EstadoRobot.En_Transito, amr.EstadoPrevioBloqueo);
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);

            amr.ReanudarTrasDespejeLiDAR();
            Assert.IsFalse(amr.EnParadaSeguridadLiDAR);
            Assert.AreEqual(RobotAMR.EstadoRobot.En_Transito, amr.estadoActual);

            CollectionAssert.AreEqual(
                new[]
                {
                    RobotAMR.EstadoRobot.En_Transito,
                    RobotAMR.EstadoRobot.Interrumpido,
                    RobotAMR.EstadoRobot.En_Transito
                },
                historialEstados);
        }

        [Test]
        public void AsignarMision_EnEstadosBloqueadosODestinoNulo_RechazaSinCambiarEstado()
        {
            Pedido pedidoRechazado = CrearPedido("PAL-RECH");

            amr.estadoActual = RobotAMR.EstadoRobot.Interrumpido;
            amr.AsignarMision(pedidoRechazado, destinoValido, 1);
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);
            Assert.IsNull(amr.PedidoActual);

            amr.estadoActual = RobotAMR.EstadoRobot.Recargando;
            amr.AsignarMision(pedidoRechazado, destinoValido, 1);
            Assert.AreEqual(RobotAMR.EstadoRobot.Recargando, amr.estadoActual);
            Assert.IsNull(amr.PedidoActual);

            amr.estadoActual = RobotAMR.EstadoRobot.Apagado;
            amr.AsignarMision(pedidoRechazado, destinoValido, 1);
            Assert.AreEqual(RobotAMR.EstadoRobot.Apagado, amr.estadoActual);
            Assert.IsNull(amr.PedidoActual);

            // Destino físico nulo debe abortar de forma segura estando Inactivo
            amr.estadoActual = RobotAMR.EstadoRobot.Inactivo;
            LogAssert.Expect(LogType.Error, new Regex("Destino físico nulo"));
            amr.AsignarMision(pedidoRechazado, null, 1);
            Assert.AreEqual(RobotAMR.EstadoRobot.Inactivo, amr.estadoActual);
            Assert.IsNull(amr.PedidoActual);
        }

        [Test]
        public void NoInterferencia_LiDARNoReanudaInterrupcionBateriaNiActivaEnReposo()
        {
            // Una interrupción por batería NO debe ser reanudada por ReanudarTrasDespejeLiDAR()
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr.InducirInterrupcionPreventivaBateria(19.5f);
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);
            Assert.IsFalse(amr.EnParadaSeguridadLiDAR);

            amr.ReanudarTrasDespejeLiDAR();
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);

            // LiDAR no se activa en reposo (Inactivo) ni durante recarga
            amr.estadoActual = RobotAMR.EstadoRobot.Inactivo;
            Assert.IsFalse(amr.ActivarParadaSeguridadLiDAR());
            Assert.AreEqual(RobotAMR.EstadoRobot.Inactivo, amr.estadoActual);

            amr.estadoActual = RobotAMR.EstadoRobot.Recargando;
            Assert.IsFalse(amr.ActivarParadaSeguridadLiDAR());
            Assert.AreEqual(RobotAMR.EstadoRobot.Recargando, amr.estadoActual);
        }

        [Test]
        public void PrioridadBateriaBajo20YRestriccion_CedePasoASanoEIncluyeAlcanceTrasero()
        {
            amr.idRobot = "AMR-01";
            amr.bateriaActual = 18.0f; // < 20% -> potencia/velocidad restringida
            amr.estadoActual = RobotAMR.EstadoRobot.Transportando;
            amr.transform.position = new Vector3(10f, 0f, -20f);

            GameObject goAmr2 = CrearObjetoTemporal("Test_RobotAMR_02");
            RobotAMR amr2 = goAmr2.AddComponent<RobotAMR>();
            amr2.idRobot = "AMR-02";
            amr2.bateriaActual = 76.0f; // Sano sin restricción
            amr2.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr2.transform.position = new Vector3(10f, 0f, -22.2f);

            Assert.IsTrue(amr.TieneRestriccionPotenciaOVelocidad);
            Assert.IsFalse(amr2.TieneRestriccionPotenciaOVelocidad);
            Assert.Greater(amr2.ObtenerPrioridadVDA(amr2), amr.ObtenerPrioridadVDA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));
            Assert.IsFalse(amr2.DebeCederPasoA(amr));

            // Alcance trasero dentro del radio de entrada (2.20m < 2.50m) cuando AMR-02 avanza en el mismo sentido (+Z)
            bool convergePorDetras = RobotAMR.ConvergeOAlcanzaPorDetras(
                new Vector2(10f, -20f),
                new Vector2(0f, 1f),
                true,
                18.0f,
                new Vector2(10f, -22.2f),
                new Vector2(0f, 1f),
                false,
                76.0f,
                2.50f);
            Assert.IsTrue(convergePorDetras);

            // Si AMR-02 se aleja en sentido opuesto (-Z), no fuerza ceda el paso trasero
            bool alejandosePorDetras = RobotAMR.ConvergeOAlcanzaPorDetras(
                new Vector2(10f, -20f),
                new Vector2(0f, 1f),
                true,
                18.0f,
                new Vector2(10f, -22.2f),
                new Vector2(0f, -1f),
                false,
                76.0f,
                2.50f);
            Assert.IsFalse(alejandosePorDetras);

            // Guarda de evacuación de muelle conservada (Z > -12 y X > 18)
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr.transform.position = new Vector3(22f, 0f, -6f);
            amr2.transform.position = new Vector3(15f, 0f, -20f);
            Assert.IsFalse(amr.DebeCederPasoA(amr2));
            Assert.IsTrue(amr2.DebeCederPasoA(amr));
        }
    }
}
