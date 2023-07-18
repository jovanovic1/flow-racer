using System.Collections.Generic;
using System.Linq;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CreateGameUI : MonoBehaviour
{
	public InputField lobbyName;
	public Dropdown track;
	public Dropdown gameMode;
	public Slider playerCountSlider;
	public Image trackImage;
	public Text playerCountSliderText;
	public Image playerCountIcon;
	public Button confirmButton;
	public InputField feeText;
	[SerializeField] private UIPopUp   popUp;
	[SerializeField] private GameLauncher gameLauncher;
	[SerializeField] private UIScreen roomScreen;
    
	//resources
	public Sprite padlockSprite, publicLobbyIcon;

	private void Start()
	{

		playerCountSlider.SetValueWithoutNotify(8);
		SetPlayerCount();

		track.ClearOptions();
		track.AddOptions(ResourceManager.Instance.tracks.Select(x => x.trackName).ToList());
		track.onValueChanged.AddListener(SetTrack);
		SetTrack(0);

		gameMode.ClearOptions();
		gameMode.AddOptions(ResourceManager.Instance.gameTypes.Select(x => x.modeName).ToList());
		gameMode.onValueChanged.AddListener(SetGameType);
		SetGameType(0);

		playerCountSlider.wholeNumbers = true;
		playerCountSlider.minValue = 1;
		playerCountSlider.maxValue = 8;
		playerCountSlider.value = 2;
		playerCountSlider.onValueChanged.AddListener(x => ServerInfo.MaxUsers = (int)x);

		lobbyName.onValueChanged.AddListener(x =>
		{
			ServerInfo.LobbyName = x;
			confirmButton.interactable = !string.IsNullOrEmpty(x);
		});
		lobbyName.text = ServerInfo.LobbyName = "session" + Random.Range(0, 1000);

		feeText.onValueChanged.AddListener(x =>
		{
			ServerInfo.Fee = float.Parse(x);
			confirmButton.interactable = !string.IsNullOrEmpty(x);
		});
		ServerInfo.Fee = PlayerPrefs.GetFloat("gFee"); //initial value
		feeText.text = ServerInfo.Fee.ToString();

		ServerInfo.TrackId = track.value;
		ServerInfo.GameMode = gameMode.value;
		ServerInfo.MaxUsers = (int)playerCountSlider.value;

		confirmButton.onClick.AddListener(() =>
		{
			popUp.SetDataAndOpen($"Pay Fee {feeText.text} $FR", $"Pay entry fee and create match?", "Confirm");
			popUp.popUpConfimed += onPopUpConfirmed;
			popUp.popUpCanceled += onPopUpCanceled;
		});
	}

	private void onPopUpConfirmed()
	{
		// do a chain transaction to complete the payment
		//sh0x
		// await InterfaceManager.Instance. (feeText.text+".0");

		//if successful, change screen & create the lobby
		ClientInfo.WalletBalance = ClientInfo.WalletBalance - float.Parse(feeText.text);
		ValidateLobby();
		TryFocusScreen(roomScreen);
		TryCreateLobby(gameLauncher);
	}

	private void onPopUpCanceled()
	{
		popUp.popUpConfimed -= onPopUpConfirmed;
		popUp.popUpCanceled -= onPopUpCanceled;
		//do something
	}

	public void SetGameType(int gameType)
	{
		ServerInfo.GameMode = gameType;
	}

	public void SetTrack(int trackId)
	{
		ServerInfo.TrackId = trackId;
		trackImage.sprite = ResourceManager.Instance.tracks[trackId].trackIcon;
	}

	public void SetPlayerCount()
	{
        playerCountSlider.value = ServerInfo.MaxUsers;
		playerCountSliderText.text = $"{ServerInfo.MaxUsers}";
		playerCountIcon.sprite = ServerInfo.MaxUsers > 1 ? publicLobbyIcon : padlockSprite;
	}

	// UI Hooks

    private bool _lobbyIsValid;

	public void ValidateLobby()
	{
		_lobbyIsValid = string.IsNullOrEmpty(ServerInfo.LobbyName) == false;
	}

	public void TryFocusScreen(UIScreen screen)
	{
		if (_lobbyIsValid)
		{
			UIScreen.Focus(screen);
		}
	}

	public void TryCreateLobby(GameLauncher launcher)
	{
		if (_lobbyIsValid)
		{
			launcher.JoinOrCreateLobby();
			_lobbyIsValid = false;
		}
	}
}