namespace RevolutionaryStuff.ApiCore.Services.ServerInfoFinders;

public interface IServerInfoFinder
{
    ServerInfo GetServerInfo(ServerInfoOptions? options = null);
}
