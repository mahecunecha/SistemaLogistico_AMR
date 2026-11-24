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
        public void PrioridadSinSesgoBateriaBajo20_CargadoTienePrioridadSobreDescargado()
        {
            amr.idRobot = "AMR-01";
            amr.bateriaActual = 80.0f;
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr.transform.position = new Vector3(10f, 0f, -22f);

            GameObject goAmr2 = CrearObjetoTemporal("Test_RobotAMR_02");
            RobotAMR amr2 = goAmr2.AddComponent<RobotAMR>();
            amr2.idRobot = "AMR-02";
            amr2.bateriaActual = 18.0f; // < 20% conserva reducción de potencia/velocidad, pero NO cede prioridad por batería
            amr2.estadoActual = RobotAMR.EstadoRobot.Transportando;
            amr2.transform.position = new Vector3(10f, 0f, -20f);

            Assert.IsTrue(amr2.EnPotenciaReducidaBateria);
            Assert.IsFalse(amr.EnPotenciaReducidaBateria);
            Assert.AreEqual(8, amr2.ObtenerPrioridadVDA(amr2));
            Assert.AreEqual(5, amr.ObtenerPrioridadVDA(amr));
            Assert.IsFalse(amr2.DebeCederPasoA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));

            // Cargado (Transportando) también tiene prioridad sobre Recargando sin importar batería ni índice
            amr.estadoActual = RobotAMR.EstadoRobot.Recargando;
            Assert.AreEqual(5, amr.ObtenerPrioridadVDA(amr));
            Assert.IsFalse(amr2.DebeCederPasoA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));

            // Guarda de evacuación de muelle conservada (Z > -12 y X > 18) incluso descargado frente a cargado
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr.transform.position = new Vector3(22f, 0f, -6f);
            amr2.transform.position = new Vector3(22f, 0f, -14f);
            Assert.IsFalse(amr.DebeCederPasoA(amr2));
            Assert.IsTrue(amr2.DebeCederPasoA(amr));

            // Guarda de maniobra fija (Entregando) conservada sobre Transportando
            amr.estadoActual = RobotAMR.EstadoRobot.Entregando;
            amr.transform.position = new Vector3(22f, 0f, 4f);
            amr2.transform.position = new Vector3(22f, 0f, -14f);
            Assert.IsFalse(amr.DebeCederPasoA(amr2));
            Assert.IsTrue(amr2.DebeCederPasoA(amr));
        }

        [Test]
        public void PrioridadMismaCarga_AdelanteTienePrioridadSobreAtrasSinBloqueoEstatico()
        {
            // Caso 1: Ambos cargados (Transportando), el de adelante tiene < 20% e índice mayor -> mantiene prioridad
            amr.idRobot = "AMR-01";
            amr.bateriaActual = 80.0f;
            amr.estadoActual = RobotAMR.EstadoRobot.Transportando;
            amr.transform.position = new Vector3(10f, 0f, -22.0f);
            amr.transform.rotation = Quaternion.identity;

            GameObject goAmr2 = CrearObjetoTemporal("Test_RobotAMR_02");
            RobotAMR amr2 = goAmr2.AddComponent<RobotAMR>();
            amr2.idRobot = "AMR-02";
            amr2.bateriaActual = 18.0f;
            amr2.estadoActual = RobotAMR.EstadoRobot.Transportando;
            amr2.transform.position = new Vector3(10f, 0f, -20.0f);
            amr2.transform.rotation = Quaternion.identity;

            Assert.AreEqual(1, RobotAMR.CompararPosicionAdelanteAtras(
                new Vector2(10f, -20.0f), new Vector2(0f, 1f),
                new Vector2(10f, -22.0f), new Vector2(0f, 1f)));
            Assert.AreEqual(-1, RobotAMR.CompararPosicionAdelanteAtras(
                new Vector2(10f, -22.0f), new Vector2(0f, 1f),
                new Vector2(10f, -20.0f), new Vector2(0f, 1f)));

            Assert.IsFalse(amr2.DebeCederPasoA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));

            // Incluso a dist < 1.35m (1.10m), el líder NO frena en seco y el seguidor sí guarda distancia
            bool liderFrena = RobotAMR.EvaluarConflictoBurbujaVDA(
                new Vector2(10f, -20.0f), new Vector2(0f, 1f),
                new Vector2(10f, -21.1f), new Vector2(0f, 1f),
                2.50f);
            bool seguidorCede = RobotAMR.EvaluarConflictoBurbujaVDA(
                new Vector2(10f, -21.1f), new Vector2(0f, 1f),
                new Vector2(10f, -20.0f), new Vector2(0f, 1f),
                2.50f);
            Assert.IsFalse(liderFrena);
            Assert.IsTrue(seguidorCede);

            // Al abrirse la brecha por encima del radio de salida (2.85m >= 2.80m), el seguidor reanuda sin quedar congelado
            bool seguidorSigueBloqueado = RobotAMR.EvaluarConflictoBurbujaVDA(
                new Vector2(10f, -22.85f), new Vector2(0f, 1f),
                new Vector2(10f, -20.0f), new Vector2(0f, 1f),
                2.80f);
            Assert.IsFalse(seguidorSigueBloqueado);

            // Caso 2: Ambos descargados (Recargando adelante vs En_Transito atrás) -> gana quien va adelante
            amr.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            amr2.estadoActual = RobotAMR.EstadoRobot.Recargando;
            Assert.AreEqual(amr.ObtenerPrioridadVDA(amr), amr2.ObtenerPrioridadVDA(amr2));
            Assert.IsFalse(amr2.DebeCederPasoA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));

            // Ambos en En_Transito: también gana quien va adelante
            amr2.estadoActual = RobotAMR.EstadoRobot.En_Transito;
            Assert.IsFalse(amr2.DebeCederPasoA(amr));
            Assert.IsTrue(amr.DebeCederPasoA(amr2));

            // Caso 3: Empate geométrico frente a frente con misma carga -> desempate determinista por índice (AMR-01 > AMR-02)
            amr.transform.position = new Vector3(10f, 0f, -22.0f);
            amr.transform.rotation = Quaternion.identity; // +Z
            amr2.transform.position = new Vector3(10f, 0f, -20.0f);
            amr2.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // -Z

            Assert.AreEqual(0, RobotAMR.CompararPosicionAdelanteAtras(
                new Vector2(10f, -22.0f), new Vector2(0f, 1f),
                new Vector2(10f, -20.0f), new Vector2(0f, -1f)));
            Assert.IsFalse(amr.DebeCederPasoA(amr2));
            Assert.IsTrue(amr2.DebeCederPasoA(amr));
        }
    }
}
