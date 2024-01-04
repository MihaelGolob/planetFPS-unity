using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;

public enum ConnectionState
{
    DISCONNECTED,
    CONNECTING,
    CONNECTED
}
public class NetworkSingleton
{
    private static NetworkSingleton instance = null;
    public ConnectionState connectionState = ConnectionState.DISCONNECTED;
    public String ip = "127.0.0.1";
    public Int32 port = 8088;

    private TcpClient connection = null;
    private NetworkStream net_stream = null;
    private BinaryWriter net_writer = null;
    private BinaryReader net_reader = null;

    public static NetworkSingleton getInstance()
    {
        if(instance == null)
            instance = new NetworkSingleton();
        return instance;
    }

    //TODO tole daj na locen thread, da ne zastekne celega programa.
    public void disconnect()
    {
        if (connectionState == ConnectionState.DISCONNECTED)
            return;

        connection.Close();
        net_stream.Close();
        net_reader.Close();
        net_writer.Close();

        connectionState = ConnectionState.DISCONNECTED;
    }
    
    public void connect()
    {
        if (connectionState != ConnectionState.DISCONNECTED)
            return;

        connectionState = ConnectionState.CONNECTING;
        connection = new TcpClient(ip, port);
        net_stream = connection.GetStream();
        net_reader = new BinaryReader(net_stream);
        net_writer = new BinaryWriter(net_stream);

        connectionState = ConnectionState.CONNECTED;
    }

    public void send(Vector3 position)
    {
        if (connectionState != ConnectionState.CONNECTED)
            return;

        //net_writer.Write(position.x);
        //net_writer.Write(position.y);
        //net_writer.Write(position.z);
        net_writer.Write(position.ToString());
    }

}

public class NetworkManager : MonoBehaviour
{

    private void OnGUI()
    {
        if(GUILayout.Button("Connect"))
        {
            Debug.Log("Connecting");
            NetworkSingleton ns = NetworkSingleton.getInstance();
            ns.connect();
            
        }
        if(GUILayout.Button("Send"))
        {
            NetworkSingleton ns = NetworkSingleton.getInstance();
            ns.send(new Vector3(1, 2, 3));
        }
    }
    private void OnDisable()
    {
        NetworkSingleton ns = NetworkSingleton.getInstance();
        ns.disconnect();
        Debug.Log("Disconnecting.");
    }

}
