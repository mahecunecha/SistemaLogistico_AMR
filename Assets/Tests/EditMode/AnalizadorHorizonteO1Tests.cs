using NUnit.Framework;

namespace ProyectoBodega.Tests.EditMode
{
    public class AnalizadorHorizonteO1Tests
    {
        [Test]
        public void CalcularProyeccionHorizonte_EscenarioAInicioTurno_1_5h()
        {
            var r = RobotManagerEVE.CalcularProyeccionHorizonte(50, 80.0f, 80.0f, 1.5f);

            Assert.AreEqual(RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno, r.Escenario);
            Assert.AreEqual(42500f, r.PesoLoteKg, 1e-2f);
            Assert.AreEqual(192.5f, r.EnergiaRequeridaSoC, 1e-2f);
            Assert.AreEqual(120.0f, r.EnergiaDisponibleSoC, 1e-2f);
            Assert.AreEqual(72.5f, r.DeficitSoC, 1e-2f);
            Assert.AreEqual(0f, r.PorcentajeCarryOver, 1e-3f);
            Assert.AreEqual(3, r.AMRRequeridos);
        }

        [Test]
        public void CalcularProyeccionHorizonte_EscenarioBMadrugada_5_0h()
        {
            var r = RobotManagerEVE.CalcularProyeccionHorizonte(50, 80.0f, 80.0f, 5.0f);

            Assert.AreEqual(RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada, r.Escenario);
            Assert.AreEqual(3.0f, r.HorasRestantes, 1e-3f);
            Assert.AreEqual(30, r.PedidosAbsorbibles);
            Assert.AreEqual(20, r.PedidosRezagados);
            Assert.AreEqual(40f, r.PorcentajeCarryOver, 1e-3f);
            Assert.AreEqual(72.5f, r.DeficitSoC, 1e-2f);
            Assert.AreEqual(4, r.AMRRequeridos);
        }

        [Test]
        public void CalcularProyeccionHorizonte_EscenarioCFinalTurno_7_5h()
        {
            var r = RobotManagerEVE.CalcularProyeccionHorizonte(50, 80.0f, 80.0f, 7.5f);

            Assert.AreEqual(RobotManagerEVE.EscenarioHorizonte.EscenarioC_FinalTurno, r.Escenario);
            Assert.AreEqual(0.5f, r.HorasRestantes, 1e-3f);
            Assert.AreEqual(5, r.PedidosAbsorbibles);
            Assert.AreEqual(45, r.PedidosRezagados);
            Assert.AreEqual(90f, r.PorcentajeCarryOver, 1e-3f);
            Assert.AreEqual(72.5f, r.DeficitSoC, 1e-2f);
            Assert.AreEqual(4, r.AMRRequeridos);
        }
    }
}
