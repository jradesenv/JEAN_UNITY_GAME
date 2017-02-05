using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

public class NetworkController : MonoBehaviour, INetEventListener
{
    private static NetClient _netClient;
    private static NetPeer _serverPeer;
    private static bool _alreadyExists = false;

    private static readonly string _host = "192.168.1.110";
    private static readonly int _port = 9050;
    private static readonly char _messageTypeSeparator = '#';
    private static readonly char _messageValuesSeparator = '!';
    private static readonly string _connectionKey = "testejean";

    private static readonly int _messageMaxLength = 200;

    public static bool isConnected = false;
    public static NetworkController sharedInstance;

    #region Connect to Server

    public void ConnectToServer()
    {
        _netClient = new NetClient(this, _connectionKey);
        _netClient.Start();
        _netClient.Connect(_host, _port);
        _netClient.UpdateTime = 15;
    }

    public delegate void OnConnectionToServerFailEvent(string message);
    public static event OnConnectionToServerFailEvent _onConnectionToServerFailHandler = delegate { };
    public static void ListenToConnectionToServerFail(OnConnectionToServerFailEvent onConnectionToServerFailHandler)
    {
        NetworkController._onConnectionToServerFailHandler += onConnectionToServerFailHandler;
    }

    public static void UnlistenToConnectionToServerFail(OnConnectionToServerFailEvent onConnectionToServerFailHandler)
    {
        NetworkController._onConnectionToServerFailHandler -= onConnectionToServerFailHandler;
    }

    public delegate void OnConnectToServerEvent();
    public static event OnConnectToServerEvent _onConnectToServerHandler = delegate { };
    public static void ListenToConnectToServer(OnConnectToServerEvent onConnectedToServerHandler)
    {
        NetworkController._onConnectToServerHandler += onConnectedToServerHandler;
    }

    public static void UnlistenToConnectToServer(OnConnectToServerEvent onConnectedToServerHandler)
    {
        NetworkController._onConnectToServerHandler -= onConnectedToServerHandler;
    }

    #endregion

    #region Create Account

    public delegate void OnCreateAccountFailEvent(string message);
    private static OnCreateAccountFailEvent _onCreateAccountFailHandler = null;

    public static void ListenCreateAccountEvent(OnCreateAccountFailEvent onCreateAccountFailHandler)
    {
        NetworkController._onCreateAccountFailHandler = onCreateAccountFailHandler;
    }

    public static void UnlistenCreateAccountEvent()
    {
        NetworkController._onCreateAccountFailHandler = null;
    }

    private static void OnCreateAccountFail(string[] arrValues)
    {
        string message = arrValues[0];

        Debug.Log("Recebeu mensagem de falha de criação de conta: " + message);

        if (NetworkController._onCreateAccountFailHandler != null)
        {
            NetworkController._onCreateAccountFailHandler(message);
        }
        else
        {
            Debug.Log("Handler de falha de criação de conta não registrado!");
        }
    }

    public static void SendCreateAccountMessage(string username, string password, string playerName, Enums.CharacterClass characterClass)
    {
        string message = FormatMessageContent(Enums.ToServerMessageType.CREATE_ACCOUNT, username, password, playerName, Converters.CharacterClassToString(characterClass));
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de criação de conta: " + username + " | " + password + " | " + playerName);
    }

    #endregion

    #region Login

    public delegate void OnLoginFailEvent(string message);
    public delegate void OnLoginSuccessEvent(string id, string name, float posX, float posY, Enums.CharacterClass characterClass, DateTime lastLogin);

    private class OnLoginHandler
    {
        public OnLoginFailEvent fail;
        public OnLoginSuccessEvent success;

        public OnLoginHandler(OnLoginFailEvent failHandler, OnLoginSuccessEvent successHandler)
        {
            this.fail = failHandler;
            this.success = successHandler;
        }
    }

    private static OnLoginHandler _onLoginHandlers = null;
    public static void ListenLoginEvent(OnLoginFailEvent failHandler, OnLoginSuccessEvent successHandler)
    {
        NetworkController._onLoginHandlers = new OnLoginHandler(failHandler, successHandler);
    }

    public static void UnlistenLoginEvent()
    {
        NetworkController._onLoginHandlers = null;
    }

    private static void OnLoginFail(string[] arrValues)
    {
        string message = arrValues[0];

        Debug.Log("Recebeu mensagem de falha de login: " + message);

        if (NetworkController._onLoginHandlers != null)
        {
            NetworkController._onLoginHandlers.fail(message);
        }
        else
        {
            Debug.Log("Handler de login não registrado!");
        }
    }

    private static void OnLoginSuccess(string[] arrValues)
    {
        string id = arrValues[0];
        string name = arrValues[1];
        float posX = Converters.StringToFloat(arrValues[2]);
        float posY = Converters.StringToFloat(arrValues[3]);
        Enums.CharacterClass characterClass = Converters.StringToCharacterClass(arrValues[4]);
        DateTime lastLogin = Converters.StringToDateTime(arrValues[5]);

        Debug.Log("Recebeu mensagem de sucesso de login de: " + id + " - " + name);

        if (NetworkController._onLoginHandlers != null)
        {
            NetworkController._onLoginHandlers.success(id, name, posX, posY, characterClass, lastLogin);
        }
        else
        {
            Debug.Log("Handler de login não registrado!");
        }
    }

    public static void SendLoginMessage(string username, string password)
    {
        string message = FormatMessageContent(Enums.ToServerMessageType.LOGIN, username, password);
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de login: " + username + " | " + password);
    }

    #endregion

    #region User Connected

    public delegate void OnUserConnectedEvent(string id, string name, float posX, float posY, Enums.CharacterClass characterClass);
    private static OnUserConnectedEvent _onUserConnectedHandler = null;
    public static void ListenUserConnectedEvent(OnUserConnectedEvent onUserConnectedHandler)
    {
        NetworkController._onUserConnectedHandler = onUserConnectedHandler;
    }

    public static void UnlistenUserConnectedEvent()
    {
        NetworkController._onUserConnectedHandler = null;
    }

    private static void OnUserConnected(string[] arrValues)
    {
        string id = arrValues[0];
        string name = arrValues[1];
        float posX = Converters.StringToFloat(arrValues[2]);
        float posY = Converters.StringToFloat(arrValues[3]);
        Enums.CharacterClass characterClass = Converters.StringToCharacterClass(arrValues[4]);

        Debug.Log("Recebeu mensagem de usuario conectado de: " + id + " - " + name);

        if (NetworkController._onUserConnectedHandler != null)
        {
            NetworkController._onUserConnectedHandler(id, name, posX, posY, characterClass);
        }
        else
        {
            Debug.Log("Handler de usuario conectado não registrado!");
        }
    }

    #endregion

    #region Movement

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
        OnMovementHandler handler = new OnMovementHandler(initHandler, endHandler);
        if (_dicOnMoveHandlers.ContainsKey(id))
        {
            _dicOnMoveHandlers[id] = handler;
        }
        else
        {
            _dicOnMoveHandlers.Add(id, handler);
        }
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
        else
        {
            Debug.Log("Handler de inicio de movimento não existente para id: " + id);
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
        else
        {
            Debug.Log("Handler de fim de movimento não existente para id: " + id);
        }
    }

    public static void SendMovementMessage(string id, int moveX, int moveY)
    {

        string sMoveX = Converters.IntToString(moveX);
        string sMoveY = Converters.IntToString(moveY);

        string message = FormatMessageContent(Enums.ToServerMessageType.MOVE, id, sMoveX, sMoveY);
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de movimento: " + id + " | " + sMoveX + "|" + sMoveY);

        string[] values = new[] { id, sMoveX, sMoveY };
        OnMovementReceive(values); //send locally
    }

    public static void SendEndMovementMessage(string id, float actualX, float actualY)
    {
        string sActualX = Converters.FloatToString(actualX);
        string sActualY = Converters.FloatToString(actualY);

        string message = FormatMessageContent(Enums.ToServerMessageType.END_MOVE, id, sActualX, sActualY);
        NetworkController.SendMessage(message);

        Debug.Log("Enviou mensagem de fim de movimento: " + id + " | " + sActualX + "|" + sActualY);

        string[] values = new[] { id, sActualX, sActualY };
        OnEndMovementReceive(values); //send locally
    }

    #endregion

    #region Get Players

    public static void SendGetOtherPlayersMessage(string id)
    {
        string message = FormatMessageContent(Enums.ToServerMessageType.GET_OTHER_PLAYERS, id);
        SendMessage(message);
    }

    #endregion

    void Awake()
    {
        if (_alreadyExists)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _alreadyExists = true;
            NetworkController.sharedInstance = this;
            DontDestroyOnLoad(this.gameObject);
            //StartCoroutine("ConnectToServer");
        }
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
        if (this == NetworkController.sharedInstance && NetworkController._netClient != null)
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

        if (messageType == Enums.FromServerMessageType.MOVE.ToString("D"))
        {
            NetworkController.OnMovementReceive(arrValues);
        }
        else if (messageType == Enums.FromServerMessageType.END_MOVE.ToString("D"))
        {
            NetworkController.OnEndMovementReceive(arrValues);
        }
        else if (messageType == Enums.FromServerMessageType.USER_CONNECTED.ToString("D"))
        {
            NetworkController.OnUserConnected(arrValues);
        }
        else if (messageType == Enums.FromServerMessageType.LOGIN_SUCCESS.ToString("D"))
        {
            NetworkController.OnLoginSuccess(arrValues);
        }
        else if (messageType == Enums.FromServerMessageType.LOGIN_FAIL.ToString("D"))
        {
            NetworkController.OnLoginFail(arrValues);
        }
        else if (messageType == Enums.FromServerMessageType.CREATE_ACCOUNT_FAIL.ToString("D"))
        {
            NetworkController.OnCreateAccountFail(arrValues);
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
        if (_onConnectToServerHandler != null)
        {
            _onConnectToServerHandler();
        }

        Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectReason reason, int socketErrorCode)
    {
        NetworkController.isConnected = false;
        if (_onConnectionToServerFailHandler != null)
        {
            _onConnectionToServerFailHandler(reason.ToString());
        }

        Debug.Log("[CLIENT] We disconnected because " + reason.ToString());
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

    public static string FormatMessageContent(Enums.ToServerMessageType type, params string[] args)
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
}