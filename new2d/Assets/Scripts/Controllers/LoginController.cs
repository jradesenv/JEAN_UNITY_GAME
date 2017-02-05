using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public InputField txtUsername;
    public InputField txtPassword;
    public Text lblMessage;
    public Button btnLogin;
    public Button btnTryAgain;

    public void OnConnectedToServerHandler()
    {
        ChangeControlsActive(true);
        SetMessage("Conectado ao servidor com sucesso!");
        btnTryAgain.gameObject.SetActive(false);
    }

    public void OnConnectionToServerFailHandler(string message)
    {
        ChangeControlsActive(false);
        SetMessage("Não foi possível conectar ao servidor: " + message);
        btnTryAgain.gameObject.SetActive(true);
    }

    void Start()
    {
        btnLogin.onClick.AddListener(OnLoginClick);
        btnTryAgain.onClick.AddListener(Connect);
        btnTryAgain.gameObject.SetActive(false);

        NetworkController.ListenLoginEvent(OnLoginFailHandler, OnLoginSuccessHandler);
        NetworkController.ListenToConnectToServer(OnConnectedToServerHandler);
        NetworkController.ListenToConnectionToServerFail(OnConnectionToServerFailHandler);

        StartCoroutine("Connect");
    }

    private void Connect()
    {
        btnTryAgain.gameObject.SetActive(false);
        ChangeControlsActive(false);
        SetMessage("Conectando ao servidor. Por favor, aguarde.");
        NetworkController.sharedInstance.ConnectToServer();
    }

    System.Collections.IEnumerator WaitForSecondsWrapper(float secs)
    {
        yield return new UnityEngine.WaitForSeconds(secs);
    }

    private void OnDestroy()
    {
        NetworkController.UnlistenLoginEvent();
        NetworkController.UnlistenToConnectToServer(OnConnectedToServerHandler);
        NetworkController.UnlistenToConnectionToServerFail(OnConnectionToServerFailHandler);
        btnLogin.onClick.RemoveAllListeners();
    }

    private void OnLoginClick()
    {
        string username = txtUsername.text;
        string password = txtPassword.text;

        if (String.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            this.SetMessage("Por favor, preencha usuário e senha.");
        }
        else
        {
            this.SetMessage("Entrando...");
            this.ChangeControlsActive(false);
            NetworkController.SendLoginMessage(username, password);
        }
    }

    public void OnLoginSuccessHandler(string id, string name, float posX, float posY, DateTime lastLogin)
    {
        GameController.playerStats.id = id;
        GameController.playerStats.playerName = name;
        GameController.playerStats.x = posX;
        GameController.playerStats.y = posY;
        GameController.playerStats.lastLogin = lastLogin;

        SceneManager.LoadScene(SceneNames.main);

        Destroy(this.gameObject);
    }

    public void OnLoginFailHandler(string msg)
    {
        this.SetMessage(msg);
        this.ChangeControlsActive(true);
    }

    private void ChangeControlsActive(bool isActive)
    {
        txtUsername.gameObject.SetActive(isActive);
        txtPassword.gameObject.SetActive(isActive);
        btnLogin.gameObject.SetActive(isActive);
    }

    #region Message

    private void SetMessage(string message)
    {
        lblMessage.text = message;
        lblMessage.gameObject.SetActive(true);
    }

    private void ClearMessage()
    {
        lblMessage.text = string.Empty;
        lblMessage.gameObject.SetActive(false);
    }

    #endregion
}
