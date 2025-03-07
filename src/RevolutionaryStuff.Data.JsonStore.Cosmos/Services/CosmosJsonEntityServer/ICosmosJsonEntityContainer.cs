﻿using Microsoft.Azure.Cosmos;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;

public interface ICosmosJsonEntityContainer : IJsonEntityContainer
{
    Container Container { get; }
}
