using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateAccountController : MonoBehaviour
{
    public InputField txtUsername;
    public InputField txtPassword;
    public InputField txtPlayerName;
    public Dropdown cboCharacterClass;

    public Text lblMessage;
    public Button btnCreateAccount;
    public Button btnBack;

    void Start()
    {
        btnCreateAccount.onClick.AddListener(OnCreateAccountClick);
        btnBack.onClick.AddListener(OnBackClick);

        cboCharacterClass.AddOptions(new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData(Converters.CharacterClassToSpriteSheetName(Enums.CharacterClass.WARRIOR)),
            new Dropdown.OptionData(Converters.CharacterClassToSpriteSheetName(Enums.CharacterClass.RANGER)),
            new Dropdown.OptionData(Converters.CharacterClassToSpriteSheetName(Enums.CharacterClass.MAGE))
        });

        NetworkController.ListenCreateAccountEvent(OnCreateAccountFailHandler);
        NetworkController.ListenLoginEvent(null, OnLoginSuccessHandler);
    }

    private void OnDestroy()
    {
        NetworkController.UnlistenCreateAccountEvent();
        NetworkController.UnlistenLoginEvent();
        btnCreateAccount.onClick.RemoveAllListeners();
        btnBack.onClick.RemoveAllListeners();
    }

    private void OnBackClick()
    {
        SceneManager.LoadScene(SceneNames.login);

        Destroy(this.gameObject);
    }

    private void OnCreateAccountClick()
    {
        string username = txtUsername.text;
        string password = txtPassword.text;
        string playerName = txtPlayerName.text;
        Enums.CharacterClass characterClass = Converters.StringToCharacterClass(cboCharacterClass.value.ToString());

        if (String.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(playerName))
        {
            this.SetMessage("Por favor, preencha todos os campos.");
        }
        else
        {
            this.SetMessage("Criando...");
            this.ChangeControlsActive(false);
            NetworkController.SendCreateAccountMessage(username, password, playerName, characterClass);
        }
    }

    public void OnLoginSuccessHandler(string id, string name, float posX, float posY, Enums.CharacterClass characterClass, DateTime lastLogin)
    {
        GameController.playerStats.id = id;
        GameController.playerStats.playerName = name;
        GameController.playerStats.x = posX;
        GameController.playerStats.y = posY;
        GameController.playerStats.lastLogin = lastLogin;
        GameController.playerStats.characterClass = characterClass;

        SceneManager.LoadScene(SceneNames.main);

        Destroy(this.gameObject);
    }

    public void OnCreateAccountFailHandler(string msg)
    {
        this.SetMessage(msg);
        this.ChangeControlsActive(true);
    }

    private void ChangeControlsActive(bool isActive)
    {
        txtUsername.gameObject.SetActive(isActive);
        txtPassword.gameObject.SetActive(isActive);
        btnCreateAccount.gameObject.SetActive(isActive);
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
