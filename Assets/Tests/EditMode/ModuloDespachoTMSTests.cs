using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProyectoBodega.Tests.EditMode
{
    public class ModuloDespachoTMSTests
    {
        private readonly List<GameObject> objetosTemporales = new List<GameObject>();
        private ModuloDespachoTMS tms;

        [SetUp]
        public void SetUp()
        {
            GameObject goTms = CrearObjetoTemporal("Test_ModuloDespachoTMS");
            tms = goTms.AddComponent<ModuloDespachoTMS>();
            tms.ReiniciarEstadoMuelles();
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

        private static Pedido CrearPedido(string idPallet, float pesoKg)
        {
            return new Pedido
            {
                id_pallet = idPallet,
                nombre_producto = "Producto Test",
                codigo_sku = "SKU-TEST",
                peso_kg = pesoKg,
                coordenada_bodega = "P1-C1-F1"
            };
        }

        [Test]
        public void CalcularZOffsetLIFO_InversionYClamping_RetornaValoresExactos()
        {
            Assert.AreEqual(1.60f, ModuloDespachoTMS.CalcularZOffsetLIFO(1), 1e-4f);
            Assert.AreEqual(0.00f, ModuloDespachoTMS.CalcularZOffsetLIFO(2), 1e-4f);
            Assert.AreEqual(-1.60f, ModuloDespachoTMS.CalcularZOffsetLIFO(3), 1e-4f);

            // Clamping fuera de rango [1..3]
            Assert.AreEqual(1.60f, ModuloDespachoTMS.CalcularZOffsetLIFO(0), 1e-4f);
            Assert.AreEqual(-1.60f, ModuloDespachoTMS.CalcularZOffsetLIFO(5), 1e-4f);
        }

        [Test]
        public void RegistrarEstibaEnCamion_LimiteTresPalletsYAcumulacionPeso_RotaYSumaExacto()
        {
            Pedido p1 = CrearPedido("PAL-001", 850f);
            Pedido p2 = CrearPedido("PAL-002", 850f);
            Pedido p3 = CrearPedido("PAL-003", 920f);
            Pedido p4 = CrearPedido("PAL-004", 850f);

            tms.ConsultarMuelleDestino(p1, out int c1, out int s1);
            Assert.AreEqual(1, c1);
            Assert.AreEqual(1, s1);
            tms.RegistrarEstibaEnCamion(c1, p1, null, s1);

            tms.ConsultarMuelleDestino(p2, out int c2, out int s2);
            Assert.AreEqual(1, c2);
            Assert.AreEqual(2, s2);
            tms.RegistrarEstibaEnCamion(c2, p2, null, s2);

            tms.ConsultarMuelleDestino(p3, out int c3, out int s3);
            Assert.AreEqual(1, c3);
            Assert.AreEqual(3, s3);
            tms.RegistrarEstibaEnCamion(c3, p3, null, s3);

            Assert.AreEqual(3, tms.MatrizLIFOCamion1.Count);
            Assert.AreEqual(2620f, tms.PesoCamion1, 1e-3f);
            Assert.AreEqual(2, tms.CamionTurnoActual);
            Assert.Greater(tms.HistorialEstibasTurno.Count, 0);
            StringAssert.IsMatch(@"^\d{2}:\d{2}:\d{2}$", tms.HistorialEstibasTurno[0].Timestamp);
            StringAssert.DoesNotContain("AM", tms.HistorialEstibasTurno[0].Timestamp);
            StringAssert.DoesNotContain("PM", tms.HistorialEstibasTurno[0].Timestamp);

            tms.ConsultarMuelleDestino(p4, out int c4, out int s4);
            Assert.AreEqual(2, c4);
            Assert.AreEqual(1, s4);
        }

        [Test]
        public void ConsultarMuelleDestino_CompuertaTonelaje12000Kg_RotaASiguienteCamionOperativo()
        {
            // Cargar Camión 1 con 2 estibas pesadas (11,000 kg total; aún tendría 1 slot físico libre de 3)
            tms.RegistrarEstibaEnCamion(1, CrearPedido("PAL-H1", 5500f), null, 1);
            tms.RegistrarEstibaEnCamion(1, CrearPedido("PAL-H2", 5500f), null, 2);
            Assert.AreEqual(11000f, tms.ObtenerPesoCamion(1), 1e-3f);

            // Un pedido de 1,500 kg llevaría Camión 1 a 12,500 kg (> 12,000 kg) -> debe rotar a Camión 2
            tms.ConsultarMuelleDestino(CrearPedido("PAL-EXCESO", 1500f), out int camionAsignado, out int slotAsignado);
            Assert.AreEqual(2, camionAsignado);
            Assert.AreEqual(1, slotAsignado);
            Assert.LessOrEqual(tms.ObtenerPesoCamion(1), ModuloDespachoTMS.CapacidadMaximaCamionKg);

            // Un pedido exacto de 1,000 kg en Camión 2 previamente cargado a 11,000 kg sí entra (12,000 kg exactos)
            tms.RegistrarEstibaEnCamion(2, CrearPedido("PAL-H3", 5500f), null, 1);
            tms.RegistrarEstibaEnCamion(2, CrearPedido("PAL-H4", 5500f), null, 2);
            tms.ConsultarMuelleDestino(CrearPedido("PAL-LIMITE", 1000f), out int camionLimite, out int slotLimite);
            Assert.AreEqual(2, camionLimite);
            Assert.AreEqual(3, slotLimite);
        }

        [Test]
        public void ReportarInspeccionHumanaMuelle_RotacionEnCascada_EvitaMuellesBloqueados()
        {
            tms.ReportarInspeccionHumanaMuelle(1, out int sigTrasM1);
            Assert.IsTrue(tms.EstaEnInspeccionHumana(1));
            Assert.AreEqual(2, sigTrasM1);
            Assert.AreEqual(2, tms.CamionTurnoActual);

            tms.ConsultarMuelleDestino(CrearPedido("PAL-R1", 850f), out int asignadoTrasM1, out int _);
            Assert.AreEqual(2, asignadoTrasM1);

            tms.ReportarInspeccionHumanaMuelle(2, out int sigTrasM2);
            Assert.IsTrue(tms.EstaEnInspeccionHumana(2));
            Assert.AreEqual(3, sigTrasM2);
            Assert.AreEqual(3, tms.CamionTurnoActual);

            tms.ConsultarMuelleDestino(CrearPedido("PAL-R2", 850f), out int asignadoTrasM2, out int _);
            Assert.AreEqual(3, asignadoTrasM2);
        }

        [Test]
        public void EstaMuelleAbiertoParaEntrega_SelladoEInspeccion_BloqueaAsignacionSiTodosCerrados()
        {
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(2));
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(3));
            Assert.IsTrue(tms.TieneAlgunMuelleAbierto());

            tms.EstablecerCamionSellado(1, true);
            Assert.IsFalse(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.IsTrue(tms.TieneAlgunMuelleAbierto());

            tms.ReportarInspeccionHumanaMuelle(2, out _);
            Assert.IsFalse(tms.EstaMuelleAbiertoParaEntrega(2));
            Assert.IsTrue(tms.TieneAlgunMuelleAbierto());

            // Con 1 sellado y 2 en inspección, el único abierto es el 3
            tms.ConsultarMuelleDestino(CrearPedido("PAL-OPEN3", 850f), out int camionAbierto, out int slotAbierto);
            Assert.AreEqual(3, camionAbierto);
            Assert.AreEqual(1, slotAbierto);

            // Al sellar también el 3, ningún muelle queda abierto -> no debe asignar silenciosamente uno cerrado
            tms.EstablecerCamionSellado(3, true);
            Assert.IsFalse(tms.TieneAlgunMuelleAbierto());
            Transform muelleNulo = tms.ConsultarMuelleDestino(CrearPedido("PAL-NONE", 850f), out int camionBloq, out int slotBloq);
            Assert.IsNull(muelleNulo);
            Assert.AreEqual(0, camionBloq);
            Assert.AreEqual(0, slotBloq);
        }

        [Test]
        public void ClausurarMuellePorIncidenteAMR_RedirigeAOperativosYReabreConDespejeManual()
        {
            GameObject goM1 = CrearObjetoTemporal("Muelle_Camion_01");
            GameObject goM2 = CrearObjetoTemporal("Muelle_Camion_02");
            GameObject goM3 = CrearObjetoTemporal("Muelle_Camion_03");
            tms.muelleCamion01 = goM1.transform;
            tms.muelleCamion02 = goM2.transform;
            tms.muelleCamion03 = goM3.transform;

            int sig = tms.ClausurarMuellePorIncidenteAMR(1, "Falla HW de AMR-01 en bahía", out int siguiente);
            Assert.AreEqual(2, sig);
            Assert.AreEqual(2, siguiente);
            Assert.IsTrue(tms.EstaMuelleClausuradoPorIncidente(1));
            Assert.IsFalse(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.AreEqual(2, tms.ContarMuellesAbiertosParaEntrega());

            tms.ConsultarMuelleDestino(CrearPedido("PAL-2OF3", 850f), out int asignado, out int slot);
            Assert.AreEqual(2, asignado);
            Assert.AreEqual(1, slot);

            bool reabierto = tms.ConfirmarDespejeHumanoMuelle(1);
            Assert.IsTrue(reabierto);
            Assert.IsFalse(tms.EstaMuelleClausuradoPorIncidente(1));
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.AreEqual(3, tms.ContarMuellesAbiertosParaEntrega());
        }
    }
}
