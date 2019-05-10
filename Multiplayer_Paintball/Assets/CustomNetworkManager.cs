using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager : NetworkManager
{
    public SpawnPointScript[] spawnPoints;
    public GameObject player;
    // Server callbacks
    public override void OnServerConnect(NetworkConnection conn)
    {
        //Debug.Log("A client connected to the server: " + conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError) { Debug.LogError("ServerDisconnected due to error: " + conn.lastError); }
        }

        Debug.Log("A client disconnected from the server: " + conn);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].AlreadyInUse == true && spawnPoints[i].NetWorkID == conn.connectionId)
            {
                spawnPoints[i].AlreadyInUse = false;
                spawnPoints[i].NetWorkID = 0;
                NetworkServer.DestroyPlayersForConnection(conn);
                NetworkServer.RemoveExternalConnection(conn.connectionId);
                Debug.Log("CONNECTIONS: " + NetworkServer.connections.Count);
                break;
            }
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {

        NetworkServer.SetClientReady(conn);

        //Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);

    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].AlreadyInUse == false && NetworkServer.connections.Count <= spawnPoints.Length)
            {
                player = Instantiate(spawnPoints[i].WhatToSpawn, spawnPoints[i].SpawnLocation);
                player.GetComponentInChildren<Camera>().enabled = false;
                spawnPoints[i].AlreadyInUse = true;
                spawnPoints[i].NetWorkID = conn.connectionId;
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
                break;
            }
            else
            {
                Debug.Log("SPECTATE");
            }
        }
        Debug.Log("CONNECTIONS: " + NetworkServer.connections.Count);
        Debug.Log("SPAWNPOINTS: " + spawnPoints.Length);//what happend to this value?

        //Debug.Log("Client has requested to get his player added to the game");
    }

    //public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    //{
        
    //    if (player.gameObject != null)

    //        NetworkServer.Destroy(player.gameObject);

        //remove connection

    //}

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {

        //Debug.Log("Server network error occurred: " + (NetworkError)errorCode);

    }

    public override void OnStartHost()
    {

        //Debug.Log("Host has started");

    }

    public override void OnStartServer()
    {

        //Debug.Log("Server has started");

    }

    public override void OnStopServer()
    {

        //Debug.Log("Server has stopped");

    }

    public override void OnStopHost()
    {
        //Debug.Log("Host has stopped");
        //
    }

    // Client callbacks

    public override void OnClientConnect(NetworkConnection conn)
    {

        base.OnClientConnect(conn);
 
        //Debug.Log("Connected successfully to server, now to set up other stuff for the client...");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
        if (conn.lastError != NetworkError.Ok)
        {

            if (LogFilter.logError) { Debug.LogError("ClientDisconnected due to error: " + conn.lastError); }

        }

        //Debug.Log("Client disconnected from server: " + conn);

    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {

        //Debug.Log("Client network error occurred: " + (NetworkError)errorCode);

    }

    public override void OnClientNotReady(NetworkConnection conn)
    {

        //Debug.Log("Server has set client to be not-ready (stop getting state updates)");

    }

    public override void OnStartClient(NetworkClient client)
    {

        //Debug.Log("Client has started");

    }

    public override void OnStopClient()
    {

        //Debug.Log("Client has stopped");

    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {

        base.OnClientSceneChanged(conn);

        //Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");

    }

}
