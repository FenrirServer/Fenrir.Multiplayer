using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Data;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationTickSnapshotTests
    {
        [TestMethod]
        public void SimulationTickSnapshot_Serialize_WritesObjectComponentCommands()
        {
            var simulation = new Simulation(new TestLogger());
            simulation.RegisterComponentType<TestClientRpcComponent>();
            simulation.RegisterComponentType<TestServerRpcComponent>();

            var serializer = new FenrirSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

            DateTime now = DateTime.UtcNow;

            // Write
            var tickSnapshot = new SimulationTickSnapshot(simulation)
            {
                TickTime = now,
                Commands = new List<ISimulationCommand>()
                {
                    new SpawnObjectSimulationCommand(1),
                    new SpawnObjectSimulationCommand(2),
                    new AddComponentSimulationCommand(1, 100),
                    new SpawnObjectSimulationCommand(3),
                    new AddComponentSimulationCommand(3, 100),
                    new DestroyObjectSimulationCommand(1),
                    new SpawnObjectSimulationCommand(4),
                    new DestroyObjectSimulationCommand(2),
                    new RemoveComponentSimulationCommand(3, 100),
                    new DestroyObjectSimulationCommand(4),
                    new DestroyObjectSimulationCommand(3),
                }
            };
            tickSnapshot.Serialize(writer);

            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);
            var tickSnapshot2 = new SimulationTickSnapshot(simulation);
            tickSnapshot2.Deserialize(reader);

            Assert.AreEqual(11, tickSnapshot2.Commands.Count);

            Assert.IsTrue(tickSnapshot2.Commands[0].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot2.Commands[0]).ObjectId == 1);
            Assert.IsTrue(tickSnapshot2.Commands[1].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot2.Commands[1]).ObjectId == 2);
            Assert.IsTrue(tickSnapshot2.Commands[2].Type == CommandType.AddComponent && ((AddComponentSimulationCommand)tickSnapshot2.Commands[2]).ObjectId == 1 && ((AddComponentSimulationCommand)tickSnapshot2.Commands[2]).ComponentTypeHash == 100);
            Assert.IsTrue(tickSnapshot2.Commands[3].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot2.Commands[3]).ObjectId == 3);
            Assert.IsTrue(tickSnapshot2.Commands[4].Type == CommandType.AddComponent && ((AddComponentSimulationCommand)tickSnapshot2.Commands[4]).ObjectId == 3 && ((AddComponentSimulationCommand)tickSnapshot2.Commands[4]).ComponentTypeHash == 100);
            Assert.IsTrue(tickSnapshot2.Commands[5].Type == CommandType.DestroyObject && ((DestroyObjectSimulationCommand)tickSnapshot2.Commands[5]).ObjectId == 1);
            Assert.IsTrue(tickSnapshot2.Commands[6].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot2.Commands[6]).ObjectId == 4);
            Assert.IsTrue(tickSnapshot2.Commands[7].Type == CommandType.DestroyObject && ((DestroyObjectSimulationCommand)tickSnapshot2.Commands[7]).ObjectId == 2);
            Assert.IsTrue(tickSnapshot2.Commands[8].Type == CommandType.RemoveComponent && ((RemoveComponentSimulationCommand)tickSnapshot2.Commands[8]).ObjectId == 3 && ((RemoveComponentSimulationCommand)tickSnapshot2.Commands[8]).ComponentTypeHash == 100);
            Assert.IsTrue(tickSnapshot2.Commands[9].Type == CommandType.DestroyObject && ((DestroyObjectSimulationCommand)tickSnapshot2.Commands[9]).ObjectId == 4);
            Assert.IsTrue(tickSnapshot2.Commands[10].Type == CommandType.DestroyObject && ((DestroyObjectSimulationCommand)tickSnapshot2.Commands[10]).ObjectId == 3);
        }


        [TestMethod]
        public void SimulationTickSnapshot_Serialize_WritesRpcCommands()
        {
            var simulation = new Simulation(new TestLogger());
            simulation.RegisterComponentType<TestClientRpcComponent>();
            simulation.RegisterComponentType<TestServerRpcComponent>();

            var serializer = new FenrirSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

            DateTime now = DateTime.UtcNow;

            // Get rpc method info
            ulong clientRpcComponentTypeHash = simulation.GetComponentTypeHash<TestClientRpcComponent>();
            ulong serverRpcComponentTypeHash = simulation.GetComponentTypeHash<TestServerRpcComponent>();

            // Get rpc component wrappers
            var clientRpcComponentWrapper = simulation.GetComponentWrapper(typeof(TestClientRpcComponent));
            var serverRpcComponentWrapper = simulation.GetComponentWrapper(typeof(TestServerRpcComponent));

            // Get method hashes
            var clientRpcMethodHash = clientRpcComponentWrapper.CalculateMethodHash(typeof(TestClientRpcComponent).GetMethod("DoTest"));
            var serverRpcMethodHash = serverRpcComponentWrapper.CalculateMethodHash(typeof(TestServerRpcComponent).GetMethod("DoTest"));

            // Write
            var tickSnapshot = new SimulationTickSnapshot(simulation)
            {
                TickTime = now,
                Commands = new List<ISimulationCommand>()
                {
                    new ClientRpcSimulationCommand(1, clientRpcComponentTypeHash, clientRpcMethodHash, new object[]{ false, 5f, "6", 7 }),

                    new ServerRpcSimulationCommand(1, serverRpcComponentTypeHash, serverRpcMethodHash, new object[]{ false, 5f, "6", 7 })
                }
            };
            tickSnapshot.Serialize(writer);

            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);
            var tickSnapshot2 = new SimulationTickSnapshot(simulation);
            tickSnapshot2.Deserialize(reader);

            Assert.AreEqual(2, tickSnapshot2.Commands.Count);

            Assert.IsTrue(tickSnapshot2.Commands[0].Type == CommandType.ClientRpc);
            var clientRpcCmd = ((ClientRpcSimulationCommand)tickSnapshot2.Commands[0]);
            Assert.AreEqual(1, clientRpcCmd.ObjectId);
            Assert.AreEqual(clientRpcComponentTypeHash, clientRpcCmd.ComponentTypeHash);
            Assert.AreEqual(clientRpcMethodHash, clientRpcCmd.MethodHash);
            Assert.AreEqual(false, clientRpcCmd.Parameters[0]);
            Assert.AreEqual(5f, clientRpcCmd.Parameters[1]);
            Assert.AreEqual("6", clientRpcCmd.Parameters[2]);
            Assert.AreEqual(7, clientRpcCmd.Parameters[3]);

            Assert.IsTrue(tickSnapshot2.Commands[1].Type == CommandType.ServerRpc);
            var serverRpcCmd = ((ServerRpcSimulationCommand)tickSnapshot2.Commands[1]);
            Assert.AreEqual(1, serverRpcCmd.ObjectId);
            Assert.AreEqual(serverRpcComponentTypeHash, serverRpcCmd.ComponentTypeHash);
            Assert.AreEqual(serverRpcMethodHash, serverRpcCmd.MethodHash);
            Assert.AreEqual(false, serverRpcCmd.Parameters[0]);
            Assert.AreEqual(5f, serverRpcCmd.Parameters[1]);
            Assert.AreEqual("6", serverRpcCmd.Parameters[2]);
            Assert.AreEqual(7, serverRpcCmd.Parameters[3]);
        }


        [TestMethod]
        public void SimulationTickSnapshot_Serialize_MultipleSnapshots_IntoSingleStream()
        {
            var simulation = new Simulation(new TestLogger());
            var serializer = new FenrirSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

            DateTime now = DateTime.UtcNow;

            // Write
            SimulationTickSnapshot tickSnapshot;

            // 1
            tickSnapshot = new SimulationTickSnapshot(simulation) { TickTime = now };
            tickSnapshot.Serialize(writer);
            
            // 2
            tickSnapshot = new SimulationTickSnapshot(simulation) { TickTime = now + TimeSpan.FromSeconds(1) };
            tickSnapshot.Serialize(writer);

            // 3
            tickSnapshot = new SimulationTickSnapshot(simulation)
            {
                TickTime = now + TimeSpan.FromSeconds(2),
                Commands = new List<ISimulationCommand>()
                {
                    new SpawnObjectSimulationCommand(1),
                }
            };
            tickSnapshot.Serialize(writer);

            // 4
            tickSnapshot = new SimulationTickSnapshot(simulation) { TickTime = now + TimeSpan.FromSeconds(3) };
            tickSnapshot.Serialize(writer);


            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);

            // 1
            tickSnapshot = new SimulationTickSnapshot(simulation);
            tickSnapshot.Deserialize(reader);
            Assert.AreEqual(now, tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);

            // 2
            tickSnapshot = new SimulationTickSnapshot(simulation);
            tickSnapshot.Deserialize(reader);
            Assert.AreEqual(now + TimeSpan.FromSeconds(1), tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);

            // 3
            tickSnapshot = new SimulationTickSnapshot(simulation);
            tickSnapshot.Deserialize(reader);
            Assert.AreEqual(now + TimeSpan.FromSeconds(2), tickSnapshot.TickTime);
            Assert.AreEqual(1, tickSnapshot.Commands.Count);
            Assert.IsTrue(tickSnapshot.Commands[0].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot.Commands[0]).ObjectId == 1);

            // 4
            tickSnapshot = new SimulationTickSnapshot(simulation);
            tickSnapshot.Deserialize(reader);
            Assert.AreEqual(now + TimeSpan.FromSeconds(3), tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);
        }
    }
}
