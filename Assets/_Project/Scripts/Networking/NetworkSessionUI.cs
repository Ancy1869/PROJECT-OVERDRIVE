using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Temporary development UI for starting and stopping an NGO session.
/// Attach this to a scene object while testing multiplayer.
/// Replace this OnGUI panel with the real menu UI later.
/// </summary>
public sealed class NetworkSessionUI : MonoBehaviour
{
    private string statusMessage = "Not connected";

    private void OnEnable()
    {
        NetworkManager.OnServerStarted += HandleServerStarted;
        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManager.OnServerStarted -= HandleServerStarted;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(16, 16, 280, 220), GUI.skin.box);
        GUILayout.Label("PROJECT OVERDRIVE - NETWORK");

        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null)
        {
            GUILayout.Label("NetworkManager not found in this scene.");
            GUILayout.Label("Add a NetworkManager with UnityTransport.");
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"Status: {statusMessage}");

        if (!manager.IsListening)
        {
            if (GUILayout.Button("Start Host"))
                StartHost();

            if (GUILayout.Button("Start Client"))
                StartClient();

            if (GUILayout.Button("Start Server"))
                StartServer();
        }
        else
        {
            GUILayout.Label($"Local Client ID: {manager.LocalClientId}");

            if (GUILayout.Button("Disconnect / Stop"))
                StopSession();
        }

        GUILayout.EndArea();
    }

    public void StartHost()
    {
        if (!CanStartSession())
            return;

        statusMessage = NetworkManager.Singleton.StartHost()
            ? "Starting as Host..."
            : "Could not start Host. Check the Console.";
    }

    public void StartClient()
    {
        if (!CanStartSession())
            return;

        statusMessage = NetworkManager.Singleton.StartClient()
            ? "Connecting as Client..."
            : "Could not start Client. Check the Console.";
    }

    public void StartServer()
    {
        if (!CanStartSession())
            return;

        statusMessage = NetworkManager.Singleton.StartServer()
            ? "Starting dedicated Server..."
            : "Could not start Server. Check the Console.";
    }

    public void StopSession()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.Shutdown();
        statusMessage = "Disconnected";
    }

    private bool CanStartSession()
    {
        if (NetworkManager.Singleton == null)
        {
            statusMessage = "NetworkManager not found.";
            return false;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            statusMessage = "A session is already running.";
            return false;
        }

        return true;
    }

    private void HandleServerStarted()
    {
        statusMessage = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost
            ? "Host running"
            : "Server running";
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
            statusMessage = NetworkManager.Singleton.IsHost ? "Host running" : "Connected as Client";
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
            statusMessage = "Disconnected";
    }
}
