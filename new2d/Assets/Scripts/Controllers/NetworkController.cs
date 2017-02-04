using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Linq;
using System;
using System.Collections.Generic;

public class NetworkController : MonoBehaviour, INetEventListener
{
    private static NetClient _netClient;
    private static NetPeer _serverPeer;
    private static bool _alreadyExists = false;

    private static readonly string _host = "localhost";
    private static readonly int _port = 9050;
    private static readonly char _messageTypeSeparator = '#';
    private static readonly char _messageValuesSeparator = '!';
    private static readonly string _connectionKey = "testejean";
    private static readonly int _messageMaxLength = 200;

    public static bool isConnected = false;

    //Movement
    public delegate void OnMovementReceiveEvent(int moveX, int moveY);
    public delegate void OnEndMovementReceiveEvent(float posX, float posY);
    private class OnMovementHandler
    {
        public OnMovementReceiveEvent init;
        public OnEndMovementReceiveEvent end;

        public OnMovementHandler(OnMovementReceiveEvent initHandler, OnEndMovementReceiveEvent endHandler)
        {
            this.init = initHandler;
            this.end = endHandler;
        }
    }

    private static Dictionary<String, OnMovementHandler> _dicOnMoveHandlers = new Dictionary<string, OnMovementHandler>();
    public static void ListenObjectMovement(string id, OnMovementReceiveEvent initHandler, OnEndMovementReceiveEvent endHandler)
    {
        _dicOnMoveHandlers.Add(id, new OnMovementHandler(initHandler, endHandler));
    }

    public static void UnlistenObjectMovement(string id)
    {
        _dicOnMoveHandlers.Remove(id);
    }

    private static void OnMovementReceive(string[] arrValues)
    {
        string id = arrValues[0];
        int moveX = Converters.StringToInt(arrValues[1]);
        int moveY = Converters.StringToInt(arrValues[2]);

        Debug.Log("Recebeu mensagem de movimento de " + id);

        OnMovementHandler handler;
        if (_dicOnMoveHandlers.TryGetValue(id, out handler))
        {
            handler.init(moveX, moveY);
        }
    }

    private static void OnEndMovementReceive(string[] arrValues)
    {
        string id = arrValues[0];
        float posX = Converters.StringToFloat(arrValues[1]);
        float posY = Converters.StringToFloat(arrValues[2]);

        Debug.Log("Recebeu mensagem de fim de movimento de " + id);

        OnMovementHandler handler;
        if (_dicOnMoveHandlers.TryGetValue(id, out handler))
        {
            handler.end(posX, posY);
        }
    }

    public static void SendMovementMessage(string id, int moveX, int moveY)
    {

        string sMoveX = Converters.IntToString(moveX);
        string sMoveY = Converters.IntToString(moveY);

        string message = FormatMessageContent(ToServerMessageType.MOVE, id, sMoveX, sMoveY);
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de movimento: " + id + " | " + sMoveX + "|" + sMoveY);

        string[] values = new[] { id, sMoveX, sMoveY };
        OnMovementReceive(values); //send locally
    }

    public static void SendEndMovementMessage(string id, float actualX, float actualY)
    {
        string sActualX = Converters.FloatToString(actualX);
        string sActualY = Converters.FloatToString(actualY);

        string message = FormatMessageContent(ToServerMessageType.END_MOVE, id, sActualX, sActualY);
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de fim de movimento: " + id + " | " + sActualX + "|" + sActualY);

        string[] values = new[] { id, sActualX, sActualY };
        OnEndMovementReceive(values); //send locally

    }
    //end Movement

    void Start()
    {
        if (_alreadyExists)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _alreadyExists = true;
            DontDestroyOnLoad(this.gameObject);
            StartCoroutine("ConnectToServer");
        }
    }

    void ConnectToServer()
    {
        _netClient = new NetClient(this, _connectionKey);
        _netClient.Start();
        _netClient.Connect(_host, _port);
        _netClient.UpdateTime = 15;
    }

    void Update()
    {
        if (NetworkController._netClient != null)
        {
            NetworkController._netClient.PollEvents();
        }
    }

    void OnDestroy()
    {
        if (NetworkController._netClient != null)
        {
            NetworkController._netClient.Stop();
        }
    }

    public void OnNetworkReceive(NetPeer fromPeer, NetDataReader dataReader)
    {
        string completeMessage = dataReader.GetString(_messageMaxLength);
        string[] arrMessageParts = completeMessage.Split(_messageTypeSeparator);
        string messageType = arrMessageParts[0];
        string[] arrValues = arrMessageParts[1].Split(_messageValuesSeparator);

        if (messageType == FromServerMessageType.MOVE.ToString("D"))
        {
            NetworkController.OnMovementReceive(arrValues);
        }
        else if (messageType == FromServerMessageType.END_MOVE.ToString("D"))
        {
            NetworkController.OnEndMovementReceive(arrValues);
        }
        else
        {
            Debug.Log("[WARNING] Received a message with unknown type: " + completeMessage);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        _serverPeer = peer;
        NetworkController.isConnected = true;

        Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectReason reason, int socketErrorCode)
    {
        Debug.Log("[CLIENT] We disconnected because " + reason);
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.DiscoveryResponse && _netClient.Peer == null)
        {
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            _netClient.Connect(remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    private static new void SendMessage(string message)
    {
        string completeMessage = message;
        NetDataWriter writer = new NetDataWriter();
        writer.Put(completeMessage);
        NetworkController._serverPeer.Send(writer, SendOptions.ReliableOrdered);
    }

    public static string FormatMessageContent(ToServerMessageType type, params string[] args)
    {
        string message = type.ToString("D") + NetworkController._messageTypeSeparator;
        int argsLength = args.Length;

        if (argsLength == 1)
        {
            message = message + args[0];
        }
        else if (argsLength > 1)
        {
            message = message + String.Join(NetworkController._messageValuesSeparator.ToString(), args);
        }

        return message;
    }

    public enum FromServerMessageType
    {
        MOVE = 0,
        END_MOVE = 1,
    }

    public enum ToServerMessageType
    {
        MOVE = 0,
        END_MOVE = 1
    }
}