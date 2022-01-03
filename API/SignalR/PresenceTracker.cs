
namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;        
        lock (OnlineUsers)
        {

            if (!OnlineUsers.ContainsKey(username))
                OnlineUsers.Add(username, new List<string>());

            if (!OnlineUsers[username].Any(id => id == connectionId))
            {
                OnlineUsers[username].Add(connectionId);
                isOnline = true;
            }                
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        bool isOffline = false;
        lock(OnlineUsers)
        {
            if(!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

            OnlineUsers[username].Remove(connectionId);

            if(!OnlineUsers[username].Any())
            {
                OnlineUsers.Remove(username);
                isOffline = true;
            }                
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        
        lock(OnlineUsers)
        {
            onlineUsers = OnlineUsers.Keys.OrderBy(k => k).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public Task<List<string>> GetConnectionsForUser(string username)
    {
       List<string> connectionsIds;
       lock(OnlineUsers)
       {
           connectionsIds = OnlineUsers.GetValueOrDefault(username);
       } 

       return Task.FromResult(connectionsIds);
    }
}