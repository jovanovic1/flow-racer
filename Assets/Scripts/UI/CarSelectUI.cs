using System.Collections;
using System.Collections.Generic;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectUI : MonoBehaviour
{
	public Image speedStatBar;
	public Image accelStatBar;
	public Image turnStatBar;
	public Text	 priceText;
	public Button backButton;
	public Button buyButton;
	[SerializeField] private UIPopUp popUp;

	private int selectedCarIndex = -1;
	private CarDefinition[] carDefs; 

	private void OnEnable() {
		if(ClientInfo.KartId != -1)
		{
			SelectKart(ClientInfo.KartId);
		}
		backButton.onClick.AddListener(OnBackButtonClicked);
		buyButton.onClick.AddListener(OnBuyButtonClicked);
		carDefs = ResourceManager.Instance.carDefinitions;
	}

	private void OnBackButtonClicked()
	{
		Debug.Log("Back button clicked"+ selectedCarIndex);
		//do something
		if(selectedCarIndex != -1)
			if(carDefs[selectedCarIndex].IsOwned)
				ClientInfo.KartId = selectedCarIndex;

		SelectKart(ClientInfo.KartId); //focus the selected kart
	}

	

	private async void OnBuyButtonClicked()
	{
		Debug.Log("Buy button clicked"+ selectedCarIndex);
		var selectedCar = carDefs[selectedCarIndex];
		if(selectedCar.Price >-1)
		{
			//check with chain
			//sh0x buy transaction initiated here for a selected NFT
			popUp.SetDataAndOpen("Transaction Submitted", "Please wait for the transaction to be mined", "");
			bool fetched1 = await InterfaceManager.Instance.SubmitTxMintNFT((selectedCarIndex+1).ToString(), "www.google.com", carDefs[selectedCarIndex].Price.ToString() + ".0");
			bool fetched2 = await InterfaceManager.Instance.ExecuteGetAllOwnedNFTs(ClientInfo.UserWallet);
			if(fetched1)
			{
				ClientInfo.WalletBalance = ClientInfo.WalletBalance - selectedCar.Price;
				popUp.Close();
				selectedCar.IsOwned = true;
				ClientInfo.KartId = selectedCarIndex;
			}
			else
			{
				//show txn failed
			}
			if(fetched2)
			{
				//update the car selection data
			}
		}
		else
		{
			selectedCar.IsOwned = true;
			ClientInfo.KartId = selectedCarIndex;
		}

		UIScreen.BackToInitial();
	}
	
	public void SelectKart(int kartIndex)
	{
		selectedCarIndex = kartIndex;
		// ClientInfo.KartId = kartIndex;
        if (SpotlightGroup.Search("CarHolder", out SpotlightGroup spotlight)) spotlight.FocusIndex(kartIndex);
		if(kartIndex != -1)
			ApplyStats(selectedCarIndex);

        if ( RoomPlayer.Local != null ) {
            RoomPlayer.Local.RPC_SetKartId(kartIndex);
        }
	}

	private void ApplyStats(int index)
	{
		CarDefinition def = ResourceManager.Instance.carDefinitions[index];
		speedStatBar.fillAmount = def.SpeedStat;
		accelStatBar.fillAmount = def.AccelStat;
		turnStatBar.fillAmount = def.TurnStat;
		if(def.IsOwned)
		{
			priceText.text = "Owned";
			backButton.GetComponentInChildren<Text>().text = "Confirm";
			backButton.gameObject.SetActive(true);
			buyButton.gameObject.SetActive(false);
		}
		else
		{
			priceText.text = def.Price.ToString() + "$FR";
			backButton.GetComponentInChildren<Text>().text = "Back";
			buyButton.gameObject.SetActive(true);
		}
	}
}
