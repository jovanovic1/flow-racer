using UnityEngine;
using UnityEngine.UI;

public class JoinGameUI : MonoBehaviour {
    
    public InputField lobbyName;
	public Button confirmButton;
	[SerializeField] private UIPopUp popUp;
	[SerializeField] private GameLauncher gameLauncher;
	[SerializeField] private UIScreen roomScreen;

	private void OnEnable()
	{
		SetLobbyName(lobbyName.text);
	}

	private void Start() {
		lobbyName.onValueChanged.AddListener(SetLobbyName);
        lobbyName.text = ClientInfo.LobbyName;

		// confirmButton.onClick.AddListener( () => {
		// 	popUp.SetDataAndOpen("Finding match", $"fetching match details", "Cancel");
		// 	popUp.popUpConfimed += onPopUpConfirmed;
		// 	popUp.popUpCanceled += onPopUpCanceled;
		// });
		confirmButton.onClick.AddListener(onConfirmButtonClicked);
	}

	private async void onConfirmButtonClicked()
	{
		//find match 
		// var session = await gameLauncher.GetSessionInfoByID();
		var session = 0;
		if(session == null)
		{
			popUp.SetDataAndOpen("Error", $"No match found", "Ok");
			popUp.popUpConfimed += onPopUpConfirmed;
			popUp.popUpCanceled += onPopUpCanceled;
			return;
		}
		else
		{
			popUp.SetDataAndOpen($"Pay {PlayerPrefs.GetFloat("gFee")}", $"Match found", "Confirm");
			popUp.popUpConfimed += onPopUpConfirmed1;
			popUp.popUpCanceled += onPopUpCanceled1;
		}
	}

	private void onPopUpConfirmed1()
	{
		Debug.Log("Hey1");
		//take joining fee - sh0x 
		//PlayerPrefs.GetFloat("gFee")}
		//join lobby
		gameLauncher.JoinOrCreateLobby();
		//change screen to join match
		UIScreen.Focus(roomScreen);
	}

	private void onPopUpCanceled1()
	{
		popUp.popUpConfimed -= onPopUpConfirmed1;
		popUp.popUpCanceled -= onPopUpCanceled1;	
	}

	private void onPopUpConfirmed()
	{
		//cancel the match finding coroutine
	}

	private void onPopUpCanceled()
	{
		//cancel the match finding coroutine
	}

    private void SetLobbyName(string lobby)
	{
		ClientInfo.LobbyName = lobby;
		//confirmButton.interactable = !string.IsNullOrEmpty(lobby);
	}
}
