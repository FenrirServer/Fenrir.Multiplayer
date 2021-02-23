﻿using System;

namespace Fenrir.Multiplayer.Simulation.Command
{
    class DestroyObjectSimulationCommand : IObjectSimulationCommand
    {
        public CommandType Type => CommandType.DestroyObject;

        public ushort ObjectId { get; private set; }

        public DestroyObjectSimulationCommand(ushort objectId)
        {
            ObjectId = objectId;
        }
    }
}