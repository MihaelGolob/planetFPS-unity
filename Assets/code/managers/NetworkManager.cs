using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public enum MsgFormat
{
    DestroyPlayer = 1,
    SpawnPlayer,
    MovePlayer,
    Hello,
    SpawnBullet
}

class NetworkMessage
{
    public UInt32 target;
    public byte[] data;
    public NetworkMessage(uint target, byte[] data)
    {
        this.target = target;
        this.data = data;
    }
}

class NetworkPlayerData
{
    public GameObject g = null;
    public Coroutine move_coroutine = null;
    public UInt32 id = 0;

    public NetworkPlayerData(UInt32 id,  GameObject g)
    {
        this.id = id;
        this.g = g;
    }
}

public class NetworkManager : MonoBehaviour
{
    public GameObject EnemyPrefab;

    public static readonly float send_frequency = 20;
    public static GameObject game_object;
    Queue<NetworkMessage> messages = new Queue<NetworkMessage>(100);
    NetworkMessage move_message = new NetworkMessage(1000, null);
    Dictionary<UInt32, NetworkPlayerData> network_players = new Dictionary<uint, NetworkPlayerData>(20);

    void tx_spawn_player_single(UInt32 target, Vector3 position) {
        byte[] packet = new byte[sizeof(UInt32) + 3*sizeof(float)];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((UInt32)MsgFormat.SpawnPlayer);
        binWriter.Write(position.x);
        binWriter.Write(position.y);
        binWriter.Write(position.z);
        binWriter.Flush();

        messages.Enqueue(new NetworkMessage(target, packet));
    }

    public void tx_spawn_player(Vector3 position)
    {
        //broadcast
        tx_spawn_player_single(0, position);
    }

    public void tx_destroy_player() {
        byte[] packet = new byte[sizeof(UInt32)];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((UInt32)MsgFormat.DestroyPlayer);
        binWriter.Flush();

        //broadcast
        //NetworkClient.send(0, packet);
        messages.Equals(new NetworkMessage(0, packet));
    }
    public void tx_move_player(Vector3 position, Quaternion rotation)
    {
        byte[] packet = new byte[sizeof(UInt32) + 7 * sizeof(float)];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((UInt32)MsgFormat.MovePlayer);
        binWriter.Write(position.x);
        binWriter.Write(position.y);
        binWriter.Write(position.z);

        binWriter.Write(rotation.x);
        binWriter.Write(rotation.y);
        binWriter.Write(rotation.z);
        binWriter.Write(rotation.w);
        
        binWriter.Flush();

        //broadcast
        //NetworkClient.send(0, packet);
        move_message = new NetworkMessage(0, packet);
    }

    void tx_hello(UInt32 target) //like tx spawn player, but not broadcast
    {
        byte[] packet = new byte[sizeof(UInt32) + 3 * sizeof(float)];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((UInt32)MsgFormat.Hello);
        binWriter.Flush();

        //broadcast
        //NetworkClient.send(0, packet);
        messages.Enqueue(new NetworkMessage(target, packet));
    }

    public void tx_spawn_bullet(Vector3 pos, Vector3 dir)
    {
        byte[] packet = new byte[sizeof(UInt32) + 6 * sizeof(float)];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((UInt32)MsgFormat.SpawnBullet);
        binWriter.Write(pos.x);
        binWriter.Write(pos.y);
        binWriter.Write(pos.z);

        binWriter.Write(dir.x);
        binWriter.Write(dir.y);
        binWriter.Write(dir.z);
        binWriter.Flush();

        messages.Enqueue(new NetworkMessage(0, packet));
    }

    Vector3 parse_vec3(byte[] packet, int offset)
    {
        return new Vector3(
            BitConverter.ToSingle(packet, offset + 0 * sizeof(float)),
            BitConverter.ToSingle(packet, offset + 1 * sizeof(float)),
            BitConverter.ToSingle(packet, offset + 2 * sizeof(float))
        );
    }
    Quaternion parse_quat(byte[] packet, int offset)
    {
        return new Quaternion(
            BitConverter.ToSingle(packet, offset + 0 * sizeof(float)),
            BitConverter.ToSingle(packet, offset + 1 * sizeof(float)),
            BitConverter.ToSingle(packet, offset + 2 * sizeof(float)),
            BitConverter.ToSingle(packet, offset + 3 * sizeof(float))
        );
    }

    void on_raw_msg(byte[] packet)
    {
        //parse messages
        UInt32 len    = BitConverter.ToUInt32(packet, 0);
        UInt32 sender = BitConverter.ToUInt32(packet, 4);
        UInt32 target = BitConverter.ToUInt32(packet, 8);
        UInt32 format = BitConverter.ToUInt32(packet, 12);

        switch ((MsgFormat)format)
        {
            case MsgFormat.SpawnPlayer:
                {
                    Vector3 pos = parse_vec3(packet, 16);
                    rx_spawn_player(sender, pos);
                    return;
                }
            case MsgFormat.DestroyPlayer:
                rx_destroy_player(sender);
                return;
            case MsgFormat.MovePlayer:
                {
                    Vector3 pos = parse_vec3(packet, 16);
                    Quaternion rot = parse_quat(packet, 28);
                    rx_move_player(sender, pos, rot);
                    return;
                }
            case MsgFormat.Hello:
                {
                    rx_hello(sender);
                    return;
                }
            case MsgFormat.SpawnBullet:
                {
                    Vector3 pos = parse_vec3(packet, 16);
                    Vector3 dir = parse_vec3(packet, 28);
                    rx_spawn_bullet(pos, dir);
                    return;
                }
        }
    }

    void rx_hello(UInt32 player)
    {
        //TODO: Posljemo, samo ce smo zivi aka ingame
        tx_spawn_player_single(player, Vector3.zero);
    }

    void safeStopCoroutine(Coroutine routine)
    {
        if (routine == null)
            return;
        StopCoroutine(routine);
    }

    void rx_spawn_bullet(Vector3 pos, Vector3 dir)
    {
        //TODO, Tole je zacasno ...
        GameObject.Find("gunYellow").GetComponent<WeaponNormal>().CreateBullet(pos, dir);
    }

    void rx_spawn_player(UInt32 player, Vector3 position)
    {
        GameObject net_player = GameObject.Instantiate(EnemyPrefab);
        net_player.transform.position = position;

        if (network_players.ContainsKey(player)) //To se naceloma ne sme zgodit.
        {
            safeStopCoroutine(network_players[player].move_coroutine);
            Destroy(network_players[player].g);
            network_players.Remove(player);
        }

        network_players.Add(player, new NetworkPlayerData(player, net_player));

        Debug.Log("Rx: spawn player: " + player + ", " + position);
    }
    void rx_destroy_player(UInt32 player)
    {
        if (network_players.ContainsKey(player)) //To se naceloma ne sme zgodit.
        {
            safeStopCoroutine(network_players[player].move_coroutine);
            Destroy(network_players[player].g);
            network_players.Remove(player);
        }
        
        Debug.Log("Rx: destroy player: " + player);
    }

    IEnumerator move_player_smooth(GameObject g, Vector3 start_pos, Quaternion start_rot, Vector3 end_pos, Quaternion end_rot)
    {
        float t = 0;
        float full_t = (1.0f / send_frequency);
        while (t < full_t)
        {
            t += Time.deltaTime;
            float p = t / full_t;

            g.transform.position = Vector3.LerpUnclamped(start_pos, end_pos, p);
            g.transform.rotation = Quaternion.Lerp(start_rot, end_rot, p);
            yield return null;
        }
    }

    void rx_move_player(UInt32 player, Vector3 position, Quaternion rotation)
    {
        if (!network_players.ContainsKey(player))
            return;

        safeStopCoroutine(network_players[player].move_coroutine);
        network_players[player].move_coroutine = StartCoroutine(move_player_smooth(network_players[player].g, network_players[player].g.transform.position, network_players[player].g.transform.rotation, position, rotation));
    }

    void on_connected(UInt32 id)
    {
        tx_hello(0);
        Debug.Log("Connected. id: " + id);
    }
    void on_disconnected()
    {
        foreach(UInt32 key in network_players.Keys)
        {
            StopCoroutine(network_players[key].move_coroutine);
            Destroy(network_players[key].g);
        }
        network_players.Clear();

        Debug.Log("Disconnected.");
    }


    System.String ip = "192.168.2.133";
    bool connecting = false;
    void threaded_connect()
    {
        connecting = true;

        NetworkClient.connect(ip, on_connected, on_disconnected, on_raw_msg);

        connecting = false;
    }
    private void OnGUI()
    {
        if(connecting)
        {
            GUILayout.Label("Connecting ...");
        }else
        {
            if (!NetworkClient.is_connected())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("IP");
                ip = GUILayout.TextField(ip);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Connect"))
                {
                    connecting = true;
                    Thread thread = new Thread(new ThreadStart(threaded_connect));
                    thread.Start();
                }
            }else
            {
                GUILayout.Label("Connected");
            }
        }
    }

    float last_update = 0;
    private void Update()
    {
        NetworkClient.update();
        
        //send heartbeats
        if(NetworkClient.is_connected())
        {
            //Tule posljemo vse paketke in omejimo na idk 20 ali pa 40 Hz
            float time = Time.time;

            if(time - last_update > (1.0f / 20))
            {
                last_update = time;
                while (messages.Count > 0)
                {
                    NetworkMessage msg = messages.Dequeue();
                    NetworkClient.send(msg.target, msg.data);
                }
                //posebej za move
                NetworkClient.send(move_message.target, move_message.data);
            }

            if (time - last_update > 0.2f)
            {
                last_update = time;
                NetworkClient.send(1000, null);
            }
        }
    }


    private void OnEnable()
    {
        game_object = this.gameObject;
    }

    private void OnDisable()
    {
        NetworkClient.disconnect();
    }

}


static class NetworkClient
{
    static readonly int rx_buffer_size = 1024;
    static byte[] rx_buffer = new byte[rx_buffer_size];
    static int rx_buffer_index = 0;
    static UInt32 id = 0;
    static NetworkStream stream = null;
    static OnMsgCallback msg_callback = null;
    static OnConnectedCallback connected_callback = null;
    static OnDisconnectedCallback disconnected_callback = null;

    public delegate void OnMsgCallback(byte[] packet);
    public delegate void OnConnectedCallback(UInt32 id);
    public delegate void OnDisconnectedCallback();

    public static bool is_connected()
    {
        return (stream != null) && (id != 0);
    }

    public static void connect(System.String ip, OnConnectedCallback a, OnDisconnectedCallback b, OnMsgCallback c)
    {
        if (is_connected())
            return;

        //cleanup
        disconnected_callback = null;
        disconnect();

        Int32 port = 8088;

        try
        {
            //using TcpClient client = new TcpClient(server, port); //Tole bi blo treba dati na svoj thread...
            TcpClient client = new TcpClient(ip, port);
            stream = client.GetStream();
        }
        catch
        {
            stream = null;
        }

        //tu zdej cakamo na prvi (server) msg
        msg_callback = client_get_id_msg;

        int retries = 5;
        while (!is_connected())
        {
            update_nocheck();
            Thread.Sleep(100);
            if (retries-- == 0)
            {
                disconnect();
                return;
            }
        }

        connected_callback = a;
        disconnected_callback = b;
        msg_callback = c;

        if(connected_callback != null)
            connected_callback(id);
    }

    public static void disconnect()
    {
        id = 0;
        if (stream != null)
        {
            stream.Close();
            stream = null;

            if (disconnected_callback != null)
            {
                disconnected_callback();
            }
        }
    }

    public static void send(UInt32 target, byte[] data)
    {
        if (data == null)
            data = new byte[0];

        UInt32 size = 3 * sizeof(UInt32) + (uint)data.Length;

        //Ustavtimo msg
        byte[] packet = new byte[size];
        MemoryStream memStream = new MemoryStream(packet);
        BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write(size);
        binWriter.Write(id);
        binWriter.Write(target);
        binWriter.Write(data);
        binWriter.Flush();

        bool success = true;
        try
        {
            stream.Write(packet, 0, (int)size);
        }
        catch
        {
            success = false;
        }
        if (!success)
        {
            disconnect();
        }
    }

    public static void update()
    {
        if (!is_connected())
            return;
        update_nocheck();
    }

    public static UInt32 get_id()
    {
        return id;
    }

    private static int read_from_connection(NetworkStream conn, ref byte[] data)
    {
        if (!conn.DataAvailable)
            return 0;

        while (true)
        {
            int read_ammount = 0;
            if (rx_buffer_index < sizeof(UInt32))
            {
                read_ammount = sizeof(UInt32) - rx_buffer_index;
            }
            else
            {
                UInt32 target_index = BitConverter.ToUInt32(rx_buffer, 0);
                if (target_index >= rx_buffer_size || rx_buffer_index >= rx_buffer_size)
                    return -1;
                read_ammount = (int)target_index - rx_buffer_index;
            }

            if (read_ammount == 0)
            {
                rx_buffer_index = 0;
                data = rx_buffer;
                return 1;
            }

            if (!conn.DataAvailable)
                break;

            int bytes_read = conn.Read(rx_buffer, rx_buffer_index, read_ammount);
            rx_buffer_index += bytes_read;
        }
        return 0;
    }

    private static void update_nocheck()
    {
        if (stream == null)
            return;

        bool disconnected = false;
        while (true)
        {
            byte[] packet = null;
            int read_status = read_from_connection(stream, ref packet);
            if (read_status < 0)
            {
                disconnected = true;
                break;
            }
            if (read_status == 0)
                break;

            UInt32 len = BitConverter.ToUInt32(packet, 0);
            if (len < 3 * sizeof(UInt32))
            {
                disconnected = true;
                break;
            }
            if (msg_callback != null)
            {
                msg_callback(packet);
            }
        }
        if (disconnected)
        {
            disconnect();
        }
    }

    private static void client_get_id_msg(byte[] packet)
    {
        UInt32 sender = BitConverter.ToUInt32(packet, 4);
        UInt32 target = BitConverter.ToUInt32(packet, 8);
        if (sender == 0)
        {
            id = target;
        }
    }
}
