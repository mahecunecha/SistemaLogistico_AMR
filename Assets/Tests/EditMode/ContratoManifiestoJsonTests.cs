using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace ProyectoBodega.Tests.EditMode
{
    public class ContratoManifiestoJsonTests
    {
        [Test]
        public void TryParsearYValidarManifiesto_ContratosStreamingAssets_DeserializaCamposCompletos()
        {
            string rutaBase = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
            Assert.IsTrue(File.Exists(rutaBase), "Falta manifiesto.json en StreamingAssets.");

            bool okBase = RobotManagerEVE.TryParsearYValidarManifiesto(
                File.ReadAllText(rutaBase),
                null,
                out Manifiesto baseJson,
                out string errBase);

            Assert.IsTrue(okBase, errBase);
            Assert.IsNotNull(baseJson);
            Assert.IsNotNull(baseJson.pedidos);
            Assert.AreEqual(3, baseJson.pedidos.Count);
            Assert.AreEqual("PAL-8491", baseJson.pedidos[0].id_pallet);
            Assert.AreEqual("AGU-RUB-330", baseJson.pedidos[0].codigo_sku);
            Assert.AreEqual(850f, baseJson.pedidos[0].peso_kg, 1e-3f);
            Assert.AreEqual("P1-C1-F1", baseJson.pedidos[0].coordenada_bodega);
            Assert.AreEqual("PAL-8493", baseJson.pedidos[2].id_pallet);
            Assert.AreEqual(920f, baseJson.pedidos[2].peso_kg, 1e-3f);

            var lotesEsperados = new[]
            {
                (archivo: "lote_30_pallets.json", idLote: "LOTE-ERP-030", totalPallets: 30, pesoTotal: 25500f),
                (archivo: "lote_260_pallets.json", idLote: "LOTE-ERP-260", totalPallets: 260, pesoTotal: 221000f),
                (archivo: "lote_450_pallets.json", idLote: "LOTE-ERP-450", totalPallets: 450, pesoTotal: 382500f)
            };

            for (int i = 0; i < lotesEsperados.Length; i++)
            {
                var esperado = lotesEsperados[i];
                string rutaLote = Path.Combine(Application.streamingAssetsPath, esperado.archivo);
                Assert.IsTrue(File.Exists(rutaLote), $"Falta {esperado.archivo} en StreamingAssets.");

                bool ok = RobotManagerEVE.TryParsearYValidarManifiesto(
                    File.ReadAllText(rutaLote),
                    null,
                    out Manifiesto lote,
                    out string err);

                Assert.IsTrue(ok, $"{esperado.archivo}: {err}");
                Assert.AreEqual(esperado.idLote, lote.id_lote);
                Assert.AreEqual(esperado.totalPallets, lote.total_pallets);
                Assert.AreEqual(esperado.pesoTotal, lote.peso_total_kg, 1e-2f);
                Assert.AreEqual(3, lote.pedidos.Count);
            }
        }

        [Test]
        public void TryParsearYValidarManifiesto_LoteHibridoSinPedidos_RequiereFallbackValido()
        {
            const string jsonSoloTotal = "{\"id_lote\":\"LOTE-HYB\",\"total_pallets\":15,\"pedidos\":[]}";

            bool sinFallback = RobotManagerEVE.TryParsearYValidarManifiesto(
                jsonSoloTotal,
                null,
                out _,
                out string errSinFallback);

            Assert.IsFalse(sinFallback);
            Assert.AreEqual("Esquema inválido", errSinFallback);

            var fallback = new List<Pedido>
            {
                new Pedido
                {
                    id_pallet = "PAL-FB-01",
                    nombre_producto = "Producto Fallback",
                    codigo_sku = "SKU-FB",
                    peso_kg = 850f,
                    coordenada_bodega = "P1-C1-F1"
                }
            };

            bool conFallback = RobotManagerEVE.TryParsearYValidarManifiesto(
                jsonSoloTotal,
                fallback,
                out Manifiesto loteConFallback,
                out string errConFallback);

            Assert.IsTrue(conFallback, errConFallback);
            Assert.IsNotNull(loteConFallback);
            Assert.AreEqual(1, loteConFallback.pedidos.Count);
            Assert.AreEqual("PAL-FB-01", loteConFallback.pedidos[0].id_pallet);
        }

        [Test]
        public void TryParsearYValidarManifiesto_EntradasInvalidasOVacias_RetornaFalse()
        {
            string[] invalidos =
            {
                null,
                "",
                "   ",
                "{}",
                "{json_corrupto",
                "{\"id_lote\":\"VACIO\",\"total_pallets\":0,\"pedidos\":[]}",
                "{\"id_lote\":\"SIN_ID\",\"total_pallets\":0,\"pedidos\":[{\"codigo_sku\":\"X\"}]}"
            };

            for (int i = 0; i < invalidos.Length; i++)
            {
                bool ok = RobotManagerEVE.TryParsearYValidarManifiesto(
                    invalidos[i],
                    null,
                    out Manifiesto lote,
                    out string err);

                Assert.IsFalse(ok, $"El caso inválido #{i} no debería haberse aceptado.");
                Assert.IsNull(lote);
                Assert.IsNotEmpty(err);
            }
        }

        [Test]
        public void TryParsearYValidarManifiesto_BomUtf8YArregloRaiz_DeserializaCorrectamente()
        {
            string conBom = "\uFEFF" + File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "lote_30_pallets.json"));
            bool okBom = RobotManagerEVE.TryParsearYValidarManifiesto(conBom, null, out Manifiesto loteBom, out string errBom);

            Assert.IsTrue(okBom, errBom);
            Assert.IsNotNull(loteBom);
            Assert.AreEqual("LOTE-ERP-030", loteBom.id_lote);
            Assert.AreEqual(30, loteBom.total_pallets);

            const string arregloRaiz = "\uFEFF[{\"id_pallet\":\"PAL-ARR-01\",\"nombre_producto\":\"Aguila\",\"codigo_sku\":\"AGU-RUB-330\",\"peso_kg\":850,\"coordenada_bodega\":\"P1-C1-F1\"}]";
            bool okArray = RobotManagerEVE.TryParsearYValidarManifiesto(arregloRaiz, null, out Manifiesto loteArray, out string errArray);

            Assert.IsTrue(okArray, errArray);
            Assert.IsNotNull(loteArray);
            Assert.AreEqual(1, loteArray.pedidos.Count);
            Assert.AreEqual(1, loteArray.total_pallets);
            Assert.AreEqual("PAL-ARR-01", loteArray.pedidos[0].id_pallet);
        }

        [Test]
        public void SelectorArchivosWindows_NormalizarYResolverBandejaERP_PriorizaBandejaYSeparadoresWindows()
        {
            Assert.AreEqual("C:\\Builds\\Sistema\\Bandeja_ERP", SelectorArchivosWindows.NormalizarRutaWindows("C:/Builds/Sistema/Bandeja_ERP/"));
            Assert.AreEqual("C:\\", SelectorArchivosWindows.NormalizarRutaWindows("C:/"));

            string tempRoot = Path.Combine(Path.GetTempPath(), "TestBodegaERP_" + System.Guid.NewGuid().ToString("N"));
            string dataDir = Path.Combine(tempRoot, "SistemaLogistico_AMR_Data");
            string streamingDir = Path.Combine(dataDir, "StreamingAssets");
            string bandejaStreaming = Path.Combine(streamingDir, "Bandeja_ERP");
            string bandejaExe = Path.Combine(tempRoot, "Bandeja_ERP");

            try
            {
                Directory.CreateDirectory(streamingDir);
                Assert.AreEqual(streamingDir, SelectorArchivosWindows.ResolverDirectorioInicialBandejaERP(streamingDir, dataDir));

                Directory.CreateDirectory(bandejaStreaming);
                Assert.AreEqual(bandejaStreaming, SelectorArchivosWindows.ResolverDirectorioInicialBandejaERP(streamingDir, dataDir));

                Directory.CreateDirectory(bandejaExe);
                Assert.AreEqual(bandejaExe, SelectorArchivosWindows.ResolverDirectorioInicialBandejaERP(streamingDir, dataDir));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }
    }
}
