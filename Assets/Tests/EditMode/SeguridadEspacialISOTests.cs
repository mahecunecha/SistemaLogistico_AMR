using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProyectoBodega.Tests.EditMode
{
    public class SeguridadEspacialISOTests
    {
        private readonly List<GameObject> objetosTemporales = new List<GameObject>();

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
        public void CumpleReglasGeometricasSpawnISO_CorredorValidoVsZonasProhibidas()
        {
            Vector3 posBaseCarga = new Vector3(-19.50f, 0f, -0.42f);

            // Corredor central válido: (5, 0, -20) mirando hacia -Z -> candidata en (5, 0, -23.1)
            bool corredorValido = RobotManagerEVE.CumpleReglasGeometricasSpawnISO(
                new Vector3(5f, 0f, -20f),
                Vector3.back,
                posBaseCarga,
                out Vector3 posCandidata);

            Assert.IsTrue(corredorValido);
            Assert.AreEqual(5f, posCandidata.x, 1e-3f);
            Assert.AreEqual(-23.10f, posCandidata.z, 1e-3f);

            // Rechazo en zona de muelles / rampas (z > -3.50)
            Assert.IsFalse(RobotManagerEVE.CumpleReglasGeometricasSpawnISO(
                new Vector3(22.68f, 0f, -5.0f),
                Vector3.forward,
                posBaseCarga,
                out _));

            // Rechazo en sala de carga trasera (z < ZUmbralPortico = -48.0f)
            Assert.IsFalse(RobotManagerEVE.CumpleReglasGeometricasSpawnISO(
                new Vector3(30.0f, 0f, -46.0f),
                Vector3.back,
                posBaseCarga,
                out _));

            // Rechazo en zona AS/RS (x < -16.8f)
            Assert.IsFalse(RobotManagerEVE.CumpleReglasGeometricasSpawnISO(
                new Vector3(-15.0f, 0f, -20.0f),
                Vector3.left,
                posBaseCarga,
                out _));

            // Rechazo por proximidad < 1.80 m a Base_Carga_Principal
            Vector3 baseEnCorredor = new Vector3(0.5f, 0f, -16.5f);
            Assert.IsFalse(RobotManagerEVE.CumpleReglasGeometricasSpawnISO(
                new Vector3(0.0f, 0f, -20.0f),
                Vector3.forward,
                baseEnCorredor,
                out _));
        }

        [Test]
        public void DronMantenimiento_ValidarMovimientoAereoSeguro_BloqueaDescensoSobreTrailers()
        {
            GameObject goDron = CrearObjetoTemporal("Test_DronMantenimiento");
            DronMantenimiento dron = goDron.AddComponent<DronMantenimiento>();

            bool seguro = dron.ValidarMovimientoAereoSeguro(
                new Vector3(22.68f, 10.0f, -1.50f),
                new Vector3(22.68f, 2.50f, -1.50f),
                false,
                null,
                out _);

            Assert.IsFalse(seguro, "El dron no debe autorizar descenso sobre zona de tráilers (z > -2.64 m).");
        }

        [Test]
        public void ValidarYAutorizarReanudacionISO_ExigeGaliboMinimoTresMetros()
        {
            GameObject goEve = CrearObjetoTemporal("Test_EVE_ISO");
            RobotManagerEVE eve = goEve.AddComponent<RobotManagerEVE>();

            GameObject goAmr = CrearObjetoTemporal("Test_AMR_ISO");
            goAmr.transform.position = new Vector3(5f, 0f, -20f);
            goAmr.transform.rotation = Quaternion.identity;
            RobotAMR amr = goAmr.AddComponent<RobotAMR>();
            amr.idRobot = "AMR-ISO";
            amr.bateriaActual = 80f;

            GameObject goDestino = CrearObjetoTemporal("Test_Destino_ISO");
            goDestino.transform.position = new Vector3(5f, 0f, -10f);

            Pedido pedido = new Pedido
            {
                id_pallet = "PAL-ISO-01",
                nombre_producto = "Producto ISO",
                codigo_sku = "AGU-RUB-330",
                peso_kg = 850f,
                coordenada_bodega = "P1-C1-F1"
            };

            amr.AsignarMision(pedido, goDestino.transform, 0);
            Assert.IsTrue(amr.ActivarParadaSeguridadLiDAR());
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);

            GameObject obstaculo = CrearObjetoTemporal("Test_Obstaculo_ISO");
            obstaculo.transform.position = new Vector3(5f, 2.99f, -17f);

            // Bajo 3.00 m -> no debe autorizar reanudación
            bool autorizadoBajo = eve.ValidarYAutorizarReanudacionISO(amr, obstaculo);
            Assert.IsFalse(autorizadoBajo);
            Assert.AreEqual(RobotAMR.EstadoRobot.Interrumpido, amr.estadoActual);
            Assert.IsTrue(amr.EnParadaSeguridadLiDAR);

            // A >= 3.00 m con carril frontal despejado -> debe autorizar y restaurar estado previo
            obstaculo.transform.position = new Vector3(5f, 3.00f, -17f);
            bool autorizadoSeguro = eve.ValidarYAutorizarReanudacionISO(amr, obstaculo);
            Assert.IsTrue(autorizadoSeguro);
            Assert.AreEqual(RobotAMR.EstadoRobot.En_Transito, amr.estadoActual);
            Assert.IsFalse(amr.EnParadaSeguridadLiDAR);
        }
    }
}
