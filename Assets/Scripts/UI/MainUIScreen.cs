using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class MainUIScreen : MonoBehaviour
{
	public UIScreen mainMenu;
    [SerializeField] private GameObject loginButton;
    [SerializeField] private Text       walletBalance;
    [SerializeField] private GameObject hostButton;
    [SerializeField] private GameObject joinButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject garageButton;
    [SerializeField] private Text       loggedInText;
    [SerializeField] private UIScreen   createRoomScreen;
    [SerializeField] private UIScreen   joinRoomScreen;
    [SerializeField] private UIScreen   garageScreen;
    [SerializeField] private GameLauncher gameLauncher;
    public Action onLoginButton;

    void Awake()
    {

        setUiButtonsToLogin();
        UIScreen.Focus(GetComponent<UIScreen>());
        loginButton.GetComponent<Button>().onClick.AddListener(() => onLoginButtonClicked());
        hostButton.GetComponent<Button>().onClick.AddListener(onHostButtonClicked);
        joinButton.GetComponent<Button>().onClick.AddListener(onJoinButtonClicked);
    }

    void Update()
    {
        if(UIScreen.activeScreen == mainMenu)
        {
            if(walletBalance.text != ClientInfo.WalletBalance.ToString() + " $FR")
            walletBalance.text = ClientInfo.WalletBalance.ToString() + " $FR";
        }
    }

    public void onCarPedestalClicked()
    {
        if(PlayerPrefs.HasKey("flowAddress"))
        {
            UIScreen.Focus(garageScreen);
        }
    }

    private void onLoginButtonClicked()
    {
        onLoginButton.Invoke();
    }

    private void onHostButtonClicked()
    {
        if(CheckCarOwned())
        {
            UIScreen.Focus(createRoomScreen);
            gameLauncher.SetCreateLobby();
        }
    }

    private void onJoinButtonClicked()
    {
        if(CheckCarOwned())
        {
            UIScreen.Focus(joinRoomScreen);
            gameLauncher.SetJoinLobby();
        }
    }

    private bool CheckCarOwned()
    {
        if(ClientInfo.KartId == -1)
        {
            loggedInText.text = "Please select a car";
            return false;
        }
        else
        {
            loggedInText.text = $"Logged in: {PlayerPrefs.GetString("flowAddress")}"; 
            return true;
        }
    }

    private async Task GetBalanceAndUpdate()
    {
        string balance = await InterfaceManager.Instance.GetTokenBalance(ClientInfo.UserWallet);
        // Now you have the balance, you can proceed with further actions
        ClientInfo.WalletBalance = float.Parse(balance);
        Debug.Log("Wallet balance: "+balance);
        walletBalance.text = ClientInfo.WalletBalance.ToString() + "$ FR";
    }

    public async void onLoggedIn()
    {
		setLoggedInText(PlayerPrefs.GetString("flowAddress"));
        if(ClientInfo.UserWallet == null)
            ClientInfo.UserWallet = PlayerPrefs.GetString("flowAddress");
        walletBalance.text = ClientInfo.WalletBalance.ToString() + " $FR"; 
        Debug.Log("logged in: "+ ClientInfo.UserWallet);
        setUiButtonsToMenu();
        //get user's token balance
        GetBalanceAndUpdate();
    }

    public void setUiButtonsToLogin()
    {
        loginButton.gameObject.SetActive(true);
        hostButton.SetActive(false);
        joinButton.SetActive(false);
        settingsButton.SetActive(false);
        exitButton.SetActive(false);
        garageButton.SetActive(false);
        walletBalance.transform.parent.gameObject.SetActive(false);
        loggedInText.text = "";
    }

    public void setUiButtonsToMenu()
    {
        SetCar();
        loginButton.gameObject.SetActive(false);
        hostButton.SetActive(true);
        joinButton.SetActive(true);
        settingsButton.SetActive(true);
        walletBalance.transform.parent.gameObject.SetActive(true);
        // exitButton.SetActive(true);
        garageButton.SetActive(true);
    }

    private void SetCar()
    {
        var foundCar = false;
        //check if car is owned
        foreach (CarDefinition car in ResourceManager.Instance.carDefinitions)
        {
            if (car.IsOwned)
            {
                ClientInfo.KartId = Array.IndexOf(ResourceManager.Instance.carDefinitions, car);
                if (SpotlightGroup.Search("CarHolder", out SpotlightGroup spotlight))
                {
                    spotlight.FocusIndex(ClientInfo.KartId);
                } 
                foundCar = true;
                break;
            }
        }

        if(!foundCar)
        {
            ClientInfo.KartId = -1; //lock
            if (SpotlightGroup.Search("CarHolder", out SpotlightGroup spotlight))
            {
                spotlight.FocusIndex(ClientInfo.KartId);
            }
        }
        //get data from chain
        //store in prefs
        //set first owned car or last selected car
        Debug.Log("found car"+ClientInfo.KartId);
        
            
    }

    public void setLoggedInText(string text)
    {
        loggedInText.text = $"Logged In: {text}";
    }
}
