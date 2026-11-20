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
            eve.horaTurnoActual = 1.0f;
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
        public void CalcularEvaluacionSLA_ClampVentanaHoraria_Entre4_5Y5_0Horas()
        {
            var evalTecho = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 0.0f);
            Assert.AreEqual(5.0f, evalTecho.HorasRestantes, 1e-4f);
            Assert.AreEqual(200f, evalTecho.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(400f, evalTecho.Capacidad2AMRs, 1e-3f);

            var evalMedia = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 3.5f);
            Assert.AreEqual(4.5f, evalMedia.HorasRestantes, 1e-4f);
            Assert.AreEqual(180f, evalMedia.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(360f, evalMedia.Capacidad2AMRs, 1e-3f);

            var evalPiso = RobotManagerEVE.CalcularEvaluacionSLA(0, 30, 7.0f);
            Assert.AreEqual(4.5f, evalPiso.HorasRestantes, 1e-4f);
            Assert.AreEqual(180f, evalPiso.Capacidad1AMR, 1e-3f);
            Assert.AreEqual(360f, evalPiso.Capacidad2AMRs, 1e-3f);
        }
    }
}
