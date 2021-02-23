﻿using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Serialization;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit.Simulation
{
    [TestClass]
    public class SimulationTickSnapshotSerializerTests
    {
        [TestMethod]
        public void SimulationTickSnapshotSerializer_Serialize_WritesObjectComponentCommands()
        {
            var simulation = new NetworkSimulation(new TestLogger());
            simulation.RegisterComponentType<TestClientRpcComponent>();
            simulation.RegisterComponentType<TestServerRpcComponent>();

            var serializer = new NetworkSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

            // Write
            var tickSnapshot = new SimulationTickSnapshot()
            {
                TickNumber = 1,
                TickTime = DateTime.UtcNow,
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
            simulation.TickSerializer.Serialize(tickSnapshot, writer);

            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);
            SimulationTickSnapshot tickSnapshot2 = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot2, reader);

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
            var simulation = new NetworkSimulation(new TestLogger());
            simulation.RegisterComponentType<TestClientRpcComponent>();
            simulation.RegisterComponentType<TestServerRpcComponent>();

            var serializer = new NetworkSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

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
            var tickSnapshot = new SimulationTickSnapshot()
            {
                TickNumber = 1,
                TickTime = DateTime.UtcNow,
                Commands = new List<ISimulationCommand>()
                {
                    new ClientRpcSimulationCommand(1, clientRpcComponentTypeHash, clientRpcMethodHash, new object[]{ false, 5f, "6", 7 }),

                    new ServerRpcSimulationCommand(1, serverRpcComponentTypeHash, serverRpcMethodHash, new object[]{ false, 5f, "6", 7 })
                }
            };
            simulation.TickSerializer.Serialize(tickSnapshot, writer);

            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);
            var tickSnapshot2 = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot2, reader);

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
            var simulation = new NetworkSimulation(new TestLogger());
            var serializer = new NetworkSerializer();
            ByteStreamWriter writer = new ByteStreamWriter(serializer);

            TimeSpan tickTime = TimeSpan.FromMilliseconds(1000d / 60);

            // Write
            SimulationTickSnapshot tickSnapshot;

            DateTime now = DateTime.UtcNow;

            // 1
            tickSnapshot = new SimulationTickSnapshot(1, now + tickTime);
            simulation.TickSerializer.Serialize(tickSnapshot, writer);
            
            // 2
            tickSnapshot = new SimulationTickSnapshot(2, now + tickTime * 2);
            simulation.TickSerializer.Serialize(tickSnapshot, writer);

            // 3
            tickSnapshot = new SimulationTickSnapshot(3, now + tickTime * 3)
            {
                Commands = new List<ISimulationCommand>()
                {
                    new SpawnObjectSimulationCommand(1),
                }
            };
            simulation.TickSerializer.Serialize(tickSnapshot, writer);

            // 4
            tickSnapshot = new SimulationTickSnapshot(4, now + tickTime * 4);
            simulation.TickSerializer.Serialize(tickSnapshot, writer);


            // Read
            ByteStreamReader reader = new ByteStreamReader(writer, serializer);

            // 1
            tickSnapshot = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot, reader);
            Assert.AreEqual((uint)1, tickSnapshot.TickNumber);
            Assert.AreEqual(now + tickTime, tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);

            // 2
            tickSnapshot = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot, reader);
            Assert.AreEqual((uint)2, tickSnapshot.TickNumber);
            Assert.AreEqual(now + tickTime * 2, tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);

            // 3
            tickSnapshot = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot, reader);
            Assert.AreEqual((uint)3, tickSnapshot.TickNumber);
            Assert.AreEqual(now + tickTime * 3, tickSnapshot.TickTime);
            Assert.AreEqual(1, tickSnapshot.Commands.Count);
            Assert.IsTrue(tickSnapshot.Commands[0].Type == CommandType.SpawnObject && ((SpawnObjectSimulationCommand)tickSnapshot.Commands[0]).ObjectId == 1);

            // 4
            tickSnapshot = new SimulationTickSnapshot();
            simulation.TickSerializer.Deserialize(tickSnapshot, reader);
            Assert.AreEqual((uint)4, tickSnapshot.TickNumber);
            Assert.AreEqual(now + tickTime * 4, tickSnapshot.TickTime);
            Assert.AreEqual(0, tickSnapshot.Commands.Count);
        }
    }
}