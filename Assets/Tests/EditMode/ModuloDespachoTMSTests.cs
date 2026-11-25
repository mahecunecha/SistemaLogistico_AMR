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
        public void CalcularOffsetsLIFO_Matriz2x4YClamping_RetornaCoordenadasExactas()
        {
            // Fila 0 (Fondo): Slots 1 (Izq) y 2 (Der)
            Assert.AreEqual(-0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(1), 1e-4f);
            Assert.AreEqual(1.80f, ModuloDespachoTMS.CalcularZOffsetLIFO(1), 1e-4f);
            Assert.AreEqual(0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(2), 1e-4f);
            Assert.AreEqual(1.80f, ModuloDespachoTMS.CalcularZOffsetLIFO(2), 1e-4f);

            // Fila 1: Slots 3 (Izq) y 4 (Der)
            Assert.AreEqual(-0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(3), 1e-4f);
            Assert.AreEqual(0.40f, ModuloDespachoTMS.CalcularZOffsetLIFO(3), 1e-4f);
            Assert.AreEqual(0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(4), 1e-4f);
            Assert.AreEqual(0.40f, ModuloDespachoTMS.CalcularZOffsetLIFO(4), 1e-4f);

            // Fila 2: Slots 5 (Izq) y 6 (Der)
            Assert.AreEqual(-0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(5), 1e-4f);
            Assert.AreEqual(-1.00f, ModuloDespachoTMS.CalcularZOffsetLIFO(5), 1e-4f);
            Assert.AreEqual(0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(6), 1e-4f);
            Assert.AreEqual(-1.00f, ModuloDespachoTMS.CalcularZOffsetLIFO(6), 1e-4f);

            // Fila 3 (Frente/Rampa): Slots 7 (Izq) y 8 (Der)
            Assert.AreEqual(-0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(7), 1e-4f);
            Assert.AreEqual(-2.40f, ModuloDespachoTMS.CalcularZOffsetLIFO(7), 1e-4f);
            Assert.AreEqual(0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(8), 1e-4f);
            Assert.AreEqual(-2.40f, ModuloDespachoTMS.CalcularZOffsetLIFO(8), 1e-4f);

            // Clamping fuera de rango [1..8]
            Assert.AreEqual(-0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(0), 1e-4f);
            Assert.AreEqual(1.80f, ModuloDespachoTMS.CalcularZOffsetLIFO(0), 1e-4f);
            Assert.AreEqual(0.65f, ModuloDespachoTMS.CalcularXOffsetLIFO(12), 1e-4f);
            Assert.AreEqual(-2.40f, ModuloDespachoTMS.CalcularZOffsetLIFO(12), 1e-4f);
        }

        [Test]
        public void RegistrarEstibaEnCamion_LimiteOchoPallets2x4YAcumulacionPeso_RotaYSumaExacto()
        {
            Assert.AreEqual(8, ModuloDespachoTMS.CapacidadMaximaPalletsCamion);

            float pesoEsperado = 0f;
            for (int i = 1; i <= 8; i++)
            {
                float peso = (i % 2 == 0) ? 920f : 850f;
                pesoEsperado += peso;
                Pedido pedido = CrearPedido($"PAL-00{i}", peso);

                tms.ConsultarMuelleDestino(pedido, out int camion, out int slot);
                Assert.AreEqual(1, camion);
                Assert.AreEqual(i, slot);
                tms.RegistrarEstibaEnCamion(camion, pedido, null, slot);
            }

            Assert.AreEqual(8, tms.MatrizLIFOCamion1.Count);
            Assert.AreEqual(pesoEsperado, tms.PesoCamion1, 1e-3f);
            Assert.AreEqual(0f, tms.ObtenerPesoReservadoCamion(1), 1e-3f);
            Assert.AreEqual(2, tms.CamionTurnoActual);
            Assert.Greater(tms.HistorialEstibasTurno.Count, 0);
            StringAssert.IsMatch(@"^\d{2}:\d{2}:\d{2}$", tms.HistorialEstibasTurno[0].Timestamp);
            StringAssert.DoesNotContain("AM", tms.HistorialEstibasTurno[0].Timestamp);
            StringAssert.DoesNotContain("PM", tms.HistorialEstibasTurno[0].Timestamp);

            // El 9.º pallet debe rotar al Muelle_Camion_02 (Slot #1)
            Pedido p9 = CrearPedido("PAL-009", 850f);
            tms.ConsultarMuelleDestino(p9, out int c9, out int s9);
            Assert.AreEqual(2, c9);
            Assert.AreEqual(1, s9);
        }

        [Test]
        public void ConsultarMuelleDestino_CompuertaTonelaje12000KgYReservasEnTransito_RotaASiguienteCamionOperativo()
        {
            // Cargar Camión 1 con 1 estiba entregada (5,500 kg) + 1 estiba reservada en tránsito (5,500 kg) = 11,000 kg proyectados
            Pedido pEntregado = CrearPedido("PAL-H1", 5500f);
            tms.ConsultarMuelleDestino(pEntregado, out int c1, out int s1);
            Assert.AreEqual(1, c1);
            Assert.AreEqual(1, s1);
            tms.RegistrarEstibaEnCamion(c1, pEntregado, null, s1);

            Pedido pEnTransito = CrearPedido("PAL-H2", 5500f);
            tms.ConsultarMuelleDestino(pEnTransito, out int c2, out int s2);
            Assert.AreEqual(1, c2);
            Assert.AreEqual(2, s2);
            Assert.AreEqual(5500f, tms.ObtenerPesoCamion(1), 1e-3f);
            Assert.AreEqual(5500f, tms.ObtenerPesoReservadoCamion(1), 1e-3f);
            Assert.AreEqual(11000f, tms.ObtenerPesoProyectadoCamion(1), 1e-3f);

            // Un pedido de 1,500 kg llevaría Camión 1 a 12,500 kg proyectados (> 12,000 kg) -> debe rotar a Camión 2
            tms.ConsultarMuelleDestino(CrearPedido("PAL-EXCESO", 1500f), out int camionAsignado, out int slotAsignado);
            Assert.AreEqual(2, camionAsignado);
            Assert.AreEqual(1, slotAsignado);
            Assert.LessOrEqual(tms.ObtenerPesoProyectadoCamion(1), ModuloDespachoTMS.CapacidadMaximaCamionKg);

            // Si se entrega la reserva en tránsito del Camión 1 y entra un pedido exacto de 1,000 kg -> entra en Camión 1 (12,000 kg exactos)
            tms.RegistrarEstibaEnCamion(c2, pEnTransito, null, s2);
            Assert.AreEqual(11000f, tms.ObtenerPesoCamion(1), 1e-3f);
            Assert.AreEqual(0f, tms.ObtenerPesoReservadoCamion(1), 1e-3f);

            // Camión 2 ya tiene 1,500 kg reservados; probemos llenar Camión 2 hasta 11,000 kg y verificar 12,000 kg exactos
            tms.RegistrarEstibaEnCamion(2, CrearPedido("PAL-H3", 5500f), null, 2);
            tms.RegistrarEstibaEnCamion(2, CrearPedido("PAL-H4", 4000f), null, 3);
            Assert.AreEqual(11000f, tms.ObtenerPesoProyectadoCamion(2), 1e-3f);

            tms.ConsultarMuelleDestino(CrearPedido("PAL-LIMITE", 1000f), out int camionLimite, out int slotLimite);
            Assert.AreEqual(2, camionLimite);
            Assert.AreEqual(4, slotLimite);
            Assert.AreEqual(12000f, tms.ObtenerPesoProyectadoCamion(2), 1e-3f);
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

        [Test]
        public void PrimeraEntregaEnMuelle3_RecargaEscalonadaMuelles1Y2_RespetaMuelleBloqueadoYDisparaEvento()
        {
            tms.muelleCamion01 = CrearObjetoTemporal("Muelle_Camion_01").transform;
            tms.muelleCamion02 = CrearObjetoTemporal("Muelle_Camion_02").transform;
            tms.muelleCamion03 = CrearObjetoTemporal("Muelle_Camion_03").transform;

            int eventosReapertura = 0;
            tms.OnMuellesDisponiblesNuevamente += () => eventosReapertura++;

            // Llenar Muelles 1 y 2 (16 pallets = 2 camiones completos sellados)
            for (int c = 1; c <= 2; c++)
            {
                for (int s = 1; s <= ModuloDespachoTMS.CapacidadMaximaPalletsCamion; s++)
                {
                    Pedido p = CrearPedido($"PAL-M{c}-{s}", 850f);
                    tms.ConsultarMuelleDestino(p, out int cam, out int slot);
                    Assert.AreEqual(c, cam);
                    tms.RegistrarEstibaEnCamion(cam, p, null, slot);
                }
                Assert.IsTrue(tms.EstaCamionSellado(c));
                Assert.IsFalse(tms.EstaMuelleAbiertoParaEntrega(c));
            }

            // Al entregarse el 1.er pedido en Muelle 3, Muelles 1 y 2 deben recargarse automáticamente
            Pedido pM3 = CrearPedido("PAL-M3-01", 900f);
            tms.ConsultarMuelleDestino(pM3, out int cam3, out int slot3);
            Assert.AreEqual(3, cam3);
            Assert.AreEqual(1, slot3);
            tms.RegistrarEstibaEnCamion(cam3, pM3, null, slot3);

            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(2));
            Assert.AreEqual(0, tms.ObtenerCantidadEstibasCamion(1));
            Assert.AreEqual(0, tms.ObtenerCantidadEstibasCamion(2));
            Assert.AreEqual(0f, tms.ObtenerPesoCamion(1), 1e-3f);
            Assert.AreEqual(0f, tms.ObtenerPesoCamion(2), 1e-3f);
            Assert.GreaterOrEqual(eventosReapertura, 2);

            // Si Muelle 1 está clausurado por incidente, 1.ª entrega en Muelle 3 solo recarga Muelle 2 y respeta clausura de Muelle 1
            tms.ReiniciarEstadoMuelles();
            tms.ClausurarMuellePorIncidenteAMR(1, "Bloqueo humano M1", out _);
            for (int s = 1; s <= ModuloDespachoTMS.CapacidadMaximaPalletsCamion; s++)
            {
                Pedido p = CrearPedido($"PAL-M2B-{s}", 850f);
                tms.ConsultarMuelleDestino(p, out int cam, out int slot);
                Assert.AreEqual(2, cam);
                tms.RegistrarEstibaEnCamion(cam, p, null, slot);
            }
            Assert.IsTrue(tms.EstaCamionSellado(2));

            Pedido pM3B = CrearPedido("PAL-M3B-01", 900f);
            tms.ConsultarMuelleDestino(pM3B, out int cam3B, out int slot3B);
            Assert.AreEqual(3, cam3B);
            tms.RegistrarEstibaEnCamion(cam3B, pM3B, null, slot3B);

            Assert.IsTrue(tms.EstaMuelleClausuradoPorIncidente(1));
            Assert.IsFalse(tms.EstaMuelleAbiertoParaEntrega(1));
            Assert.IsTrue(tms.EstaMuelleAbiertoParaEntrega(2));
            Assert.AreEqual(0, tms.ObtenerCantidadEstibasCamion(2));
        }
    }
}
