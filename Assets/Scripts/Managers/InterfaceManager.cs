using FusionExamples.Utility;
using System.Collections.Generic;
using UnityEngine;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Unity;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.WalletConnect;
using DapperLabs.Flow.Sdk.Cadence;
using System.Collections.Generic;
using DapperLabs.Flow.Sdk.DataObjects;
using Convert = DapperLabs.Flow.Sdk.Cadence.Convert;
using System;
using System.Threading;
using System.Threading.Tasks;
public class InterfaceManager : MonoBehaviour
{
	//HELPER
	public class PrevLoggedInAccounts
	{
		public List<AccountInfo> accounts;

		public PrevLoggedInAccounts()
		{
			accounts = new List<AccountInfo>();
		}

		public string GetUsernameByWallet(string wallet)
		{
			foreach(AccountInfo account in accounts)
			{
				if(account.UserWallet == wallet)
				{
					return account.Username;
				}
			}
			return string.Empty;
		}

		public float GetWalletBalanceByWallet(string wallet)
		{
			foreach(AccountInfo account in accounts)
			{
				if(account.UserWallet == wallet)
				{
					return account.WalletBalance;
				}
			}

			return 0f;
		}
	}

	public class AccountInfo
	{
		public string Username;
		public string UserWallet; 
		public float WalletBalance;
	}

	[SerializeField] private ProfileSetupUI profileSetup;

	public UIScreen mainMenu;
	public UIScreen pauseMenu;
	public UIScreen lobbyMenu;

	private MainUIScreen mainMenuUIScreen;
	public PrevLoggedInAccounts prevLoggedInAccounts;
	public static InterfaceManager Instance => Singleton<InterfaceManager>.Instance;
	public CadenceTransactionAsset CadenceSetUpAccount;
	public CadenceScriptAsset CadenceGetAllWagers;
	public CadenceTransactionAsset CadenceMintCoinsScript;
    [SerializeField] private UIPopUp    popUp;

	private void Start()
	{
		mainMenuUIScreen = mainMenu.GetComponent<MainUIScreen>();
		mainMenuUIScreen.onLoginButton += () => ConnectWallet();
		if(!PlayerPrefs.HasKey("prevAccounts"))
		{
			PlayerPrefs.SetString("prevAccounts", "");
			prevLoggedInAccounts = null;
		}
		else
		{
			prevLoggedInAccounts = JsonUtility.FromJson<PrevLoggedInAccounts>(PlayerPrefs.GetString("prevAccounts"));
		}
	}

	public void OpenPauseMenu()
	{
		// open pause menu only if the kart can drive and the menu isn't open already
		if (UIScreen.activeScreen != pauseMenu)
		{
			UIScreen.Focus(pauseMenu);
		}
	}

	public void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
		PlayerPrefs.DeleteAll();
	}

	[RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.quitting += InterfaceManager.Instance.ResetOwnedItems;
    }


	/// <summary>
	/// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
	/// </summary>
	public async Task<bool> DistributeWager(string matchId, string address)
	{

		Debug.Log("DistributeWager");
		// ClearTransactionResult();

		var script = @"
		import FungibleToken from 0x9a0766d93b6608b7
		import FlowRacerToken from 0x1c5fd54be8de5259
		import FlowRacer from 0x1c5fd54be8de5259

		transaction(
				matchId: UInt64,
				winner: Address,
			){ 
			let tokenReciever: &{FungibleToken.Receiver}
			let address: Address

			prepare(signer: AuthAccount){

				self.tokenReciever = getAccount(winner)
					.getCapability(FlowRacerToken.ReceiverPublicPath)
					.borrow<&{FungibleToken.Receiver}>()!

				self.address = signer.address

			}
			execute{
				FlowRacer.distributeWager(
					matchId: matchId, 
					winner: winner,
					responderRecievingCapability: self.tokenReciever
				)
			}
		}".Trim();

		Debug.Log($"Script: {script}");

		var matchIdString = matchId;

		List<CadenceBase> args = new List<CadenceBase>();
		args.Add(new CadenceNumber(CadenceNumberType.UInt64, matchIdString));
		args.Add(new CadenceAddress(address));

		Debug.Log($"Args: {args}");

		// Using the same account as proposer, payer and authorizer. 
		FlowTransactionResponse response = await Transactions.Submit(script, args);

		Debug.Log($"Response: {response}");

		Debug.Log($"Transaction Id: {response.Id}");
		Debug.Log($"Transaction Error: {response.Error}");
		//try catch 
		try
		{
			Debug.Log($"Transaction Error: {response.Error.Message}");
			Debug.Log($"Transaction Error: {response.Error.Exception}");
			Debug.Log($"Transaction Error: {response.Error.StackTrace}");
		}
		catch (System.Exception)
		{
			
			// throw;
		}

		return response.Error == null;

	}




	/// <summary>
	/// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
	/// </summary>
	public async void SubmitAcceptWager(string matchId, string amount)
	{

		Debug.Log("SubmitAcceptWager");
		// ClearTransactionResult();

		var script = @"
		import FungibleToken from 0x9a0766d93b6608b7
		import FlowRacerToken from 0x1c5fd54be8de5259
		import FlowRacer from 0x1c5fd54be8de5259

		transaction(matchId: UInt64, amount: UInt64){

			let sender: @FlowRacerToken.Vault
			let address: Address

			prepare(signer: AuthAccount){

				self.sender <- signer.borrow<&FlowRacerToken.Vault>(from: FlowRacerToken.VaultStoragePath)!.withdraw(amount: UFix64(amount)) as! @FlowRacerToken.Vault
				
				var account = getAccount(0x1c5fd54be8de5259)

				self.address = signer.address
			}

			execute{
				FlowRacer.acceptWager(
					matchId: matchId, 
					amount: amount, 
					account: self.address,
					requestorVault: <- self.sender
				)
			}
		}".Trim();

		Debug.Log($"Script: {script}");

		var matchIdString = matchId;
		var amountString = amount;

		List<CadenceBase> args = new List<CadenceBase>();
		args.Add(new CadenceNumber(CadenceNumberType.UInt64, matchIdString));
		args.Add(new CadenceNumber(CadenceNumberType.UInt64, amountString));

		Debug.Log($"Args: {args}");

		// Using the same account as proposer, payer and authorizer. 
		FlowTransactionResponse response = await Transactions.Submit(script, args);

		Debug.Log($"Response: {response}");

		Debug.Log($"Transaction Id: {response.Id}");
		Debug.Log($"Transaction Error: {response.Error}");
		//try catch 
		try
		{
			Debug.Log($"Transaction Error: {response.Error.Message}");
			Debug.Log($"Transaction Error: {response.Error.Exception}");
			Debug.Log($"Transaction Error: {response.Error.StackTrace}");
		}
		catch (System.Exception)
		{
			
			// throw;
		}

	}

	/// <summary>
	/// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
	/// </summary>
	public async Task<bool> SubmitTxMintToken(string amount)
	{

		Debug.Log("SubmitTxMintToken");
		// ClearTransactionResult();

		string script = CadenceMintCoinsScript.text;
		script = @"
		import FungibleToken from 0x9a0766d93b6608b7
		import MetadataViews from 0x631e88ae7f1d7c20

		import FlowRacerToken from 0x1c5fd54be8de5259

		transaction(amount: UFix64) {
			let tokenReceiver: &{FungibleToken.Receiver}
			let supplyBefore: UFix64

			prepare(signer: AuthAccount) {

				if signer.borrow<&FlowRacerToken.Vault>(from: FlowRacerToken.VaultStoragePath) != nil {
				} else {
					signer.save(
						<-FlowRacerToken.createEmptyVault(),
						to: FlowRacerToken.VaultStoragePath
					)

					signer.link<&FlowRacerToken.Vault{FungibleToken.Receiver}>(
						FlowRacerToken.ReceiverPublicPath,
						target: FlowRacerToken.VaultStoragePath
					)
					signer.link<&FlowRacerToken.Vault{FungibleToken.Balance, MetadataViews.Resolver}>(
						FlowRacerToken.VaultPublicPath,
						target: FlowRacerToken.VaultStoragePath
					)
					signer.link<&FlowRacerToken.Vault{FungibleToken.Provider}>(
						FlowRacerToken.VaultPublicPath,
						target: FlowRacerToken.VaultStoragePath
					)
				}  

				self.supplyBefore = FlowRacerToken.totalSupply

				self.tokenReceiver = signer
					.getCapability(FlowRacerToken.ReceiverPublicPath)
					.borrow<&{FungibleToken.Receiver}>()!
			}
			execute {
				
				let mintedVault <- FlowRacerToken.mintTokens(amount: amount)
				self.tokenReceiver.deposit(from: <-mintedVault)
			}
		}".Trim();

		Debug.Log($"Script: {script}");


		List<CadenceBase> args = new List<CadenceBase>();
		// args.Add(new CadenceAddress(recipient));
		args.Add(new CadenceNumber(CadenceNumberType.UFix64, amount));

		// Debug.Log($"Recipient: {recipient}");
		Debug.Log($"Amount: {amount}");
		Debug.Log($"Args: {args}");

		// Using the same account as proposer, payer and authorizer. 
		FlowTransactionResponse response = await Transactions.Submit(script, args);

		Debug.Log($"Response: {response}");

		Debug.Log($"Transaction Id: {response.Id}");
		Debug.Log($"Transaction Error: {response.Error}");
		//try catch 
		try
		{
			Debug.Log($"Transaction Error: {response.Error.Message}");
			Debug.Log($"Transaction Error: {response.Error.Exception}");
			Debug.Log($"Transaction Error: {response.Error.StackTrace}");
		}
		catch (System.Exception)
		{
			
			// throw;
		}

		return response.Error == null;
	}

	/// <summary>
	/// Demonstrates calling the Transactions.Submit() API with a script containing arguments. 
	/// </summary>
	public async Task<bool> SubmitTxMintNFT(string type, string url, string cost)
	{

		Debug.Log("SubmitTxMintNFT");
		// ClearTransactionResult();

		var script = @"
		import NonFungibleToken from 0x631e88ae7f1d7c20
		import CarNFT from 0x1c5fd54be8de5259
		import MetadataViews from 0x631e88ae7f1d7c20
		import FungibleToken from 0x9a0766d93b6608b7
		import FlowRacerToken from 0x1c5fd54be8de5259

		transaction(
			type: String,
			url: String,
			amount: UFix64
		) {

			let recipientCollectionRef: &{NonFungibleToken.CollectionPublic}
			let mintingIDBefore: UInt64
			let sender: @FlowRacerToken.Vault
			let tokenReceiver: &{FungibleToken.Receiver}

			prepare(signer: AuthAccount) {
				self.mintingIDBefore = CarNFT.totalSupply

				if signer.borrow<&AnyResource>(from: CarNFT.CollectionStoragePath) == nil {
					signer.save(<- CarNFT.createEmptyCollection(), to: CarNFT.CollectionStoragePath)
					signer.link<&AnyResource{NonFungibleToken.CollectionPublic, MetadataViews.ResolverCollection}>(CarNFT.CollectionPublicPath, target: CarNFT.CollectionStoragePath)
				}

				self.recipientCollectionRef = signer
					.getCapability(CarNFT.CollectionPublicPath)
					.borrow<&{NonFungibleToken.CollectionPublic}>()!
				
				self.sender <- signer.borrow<&FlowRacerToken.Vault>(from: FlowRacerToken.VaultStoragePath)!.withdraw(amount: UFix64(amount)) as! @FlowRacerToken.Vault

				self.tokenReceiver = signer
					.getCapability(FlowRacerToken.ReceiverPublicPath)
					.borrow<&{FungibleToken.Receiver}>()!
			}
			execute {
				CarNFT.mintNFT(
					recipient: self.recipientCollectionRef,
					type: type,
					url: url,
					requestorVault: <- self.sender,
					receiverCapability: self.tokenReceiver
				)
			}
		}".Trim();

		Debug.Log($"Script: {script}");

		List<CadenceBase> args = new List<CadenceBase>();
		args.Add(new CadenceString(type));
		args.Add(new CadenceString(url));
		args.Add(new CadenceNumber(CadenceNumberType.UFix64, cost));

		Debug.Log($"Args: {args}");

		// Using the same account as proposer, payer and authorizer. 
		FlowTransactionResponse response = await Transactions.Submit(script, args);

		Debug.Log($"Response: {response}");

		Debug.Log($"Transaction Id: {response.Id}");
		Debug.Log($"Transaction Error: {response.Error}");
		//try catch 
		try
		{
			Debug.Log($"Transaction Error: {response.Error.Message}");
			Debug.Log($"Transaction Error: {response.Error.Exception}");
			Debug.Log($"Transaction Error: {response.Error.StackTrace}");
		}
		catch (System.Exception)
		{
			
			// throw;
		}

		return response.Error == null;

	}

	/// <summary>
	/// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that is loaded from file (*.cdc). 
	/// The script takes a cadence Address argument and returns a cadence UFix64 value. 
	/// </summary>
	public async void GetAllWagers()
	{
		Debug.Log("ExecuteGetAllWagers");
		// string script = CadenceGetAllWagers.text;
		string script = @"
		import FlowRacer from 0x1c5fd54be8de5259

		pub fun main(): {UInt64: FlowRacer.MatchWager} {
			return FlowRacer.getAllWagers()
		}".Trim();

		FlowScriptRequest scriptReq = new FlowScriptRequest
		{
			Script = script
		};

		FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

		if (response.Error != null)
		{
			Debug.LogError(response.Error.Message);
			return;
		}

		string resultText = $"Response: {response.Value}";
		Debug.Log($"Script Response: {resultText}");
	}


	/// <summary>
	/// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that is loaded from file (*.cdc). 
	/// The script takes a cadence Address argument and returns a cadence UFix64 value. 
	/// </summary>
	public async Task<bool> ExecuteGetAllOwnedNFTs(string address)
	{
		Debug.Log("ExecuteGetAllOwnedNFTs");
		// string script = CadenceGetAllWagers.text;
		string script = @"
		import CarNFT from 0x1c5fd54be8de5259
		import MetadataViews from 0x631e88ae7f1d7c20

		pub fun main(address: Address): [CarNFT.CarNFTData] {
			
			let collection = getAccount(address).getCapability(CarNFT.CollectionPublicPath)
							.borrow<&{MetadataViews.ResolverCollection}>()
							?? panic(""Could not borrow a reference to the nft collection"")

			let ids = collection.getIDs()

			let answer: [CarNFT.CarNFTData] = []

			for id in ids {
			
			let nft = collection.borrowViewResolver(id: id)
			let view = nft.resolveView(Type<CarNFT.CarNFTData>())!

			let display = view as! CarNFT.CarNFTData
			answer.append(display)
			}
			
			return answer
		}".Trim();

		List<CadenceBase> args = new List<CadenceBase>();
		args.Add(new CadenceAddress(address));

		FlowScriptRequest scriptReq = new FlowScriptRequest
		{
			Script = script,
			Arguments = args
		};

		FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

		if (response.Error != null)
		{
			Debug.LogError(response.Error.Message);
			return false;
		}

		string resultText = $"Response: {response.Value}";
		Debug.Log($"Script Response: {resultText}");
		return true;
	}


	/// <summary>
	/// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that is loaded from file (*.cdc). 
	/// The script takes a cadence Address argument and returns a cadence UFix64 value. 
	/// </summary>
	public async void GetAllTotalWagers()
	{
		Debug.Log("ExecuteAllTotalWagers");
		// string script = CadenceGetAllWagers.text;
		string script = @"
		import FlowRacer from 0x1c5fd54be8de5259

		pub fun main(): {UInt64: UInt64} {
			return FlowRacer.getTotalWagers()
		}".Trim();

		FlowScriptRequest scriptReq = new FlowScriptRequest
		{
			Script = script
		};

		FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

		if (response.Error != null)
		{
			Debug.LogError(response.Error.Message);
			return;
		}

		string resultText = $"Response: {response.Value}";
		Debug.Log($"Script Response: {resultText}");
	}


	/// <summary>
	/// Demonstrates calling the Scripts.ExecuteAtLatestBlock() API with a script that is loaded from file (*.cdc). 
	/// The script takes a cadence Address argument and returns a cadence UFix64 value. 
	/// </summary>
	public async Task<string> GetTokenBalance(string address)
	{
		Debug.Log("GetTokenBalance");
		// string script = CadenceGetAllWagers.text;
		string script = @"
		import FungibleToken from 0x9a0766d93b6608b7
		import FlowRacerToken from 0x1c5fd54be8de5259

		pub fun main(address: Address): UFix64 {
			let account = getAccount(address)
			let vaultRef = account.getCapability(FlowRacerToken.VaultPublicPath)
				.borrow<&{FungibleToken.Balance}>()
				?? panic(""Could not borrow Balance reference to the Vault"")

			return vaultRef.balance
		}".Trim();

		List<CadenceBase> args = new List<CadenceBase>();
		args.Add(new CadenceAddress(address));

		FlowScriptRequest scriptReq = new FlowScriptRequest
		{
			Script = script,
			Arguments = args
		};

		FlowScriptResponse response = await Scripts.ExecuteAtLatestBlock(scriptReq);

		if (response.Error != null)
		{
			Debug.LogError(response.Error.Message);
			return "null";
		}

		string resultText = $"Balance: {Convert.FromCadence<string>(response.Value).ToString()}";
		Debug.Log($"Script Response: {resultText}");

		return Convert.FromCadence<string>(response.Value);
	}



	public void ConnectWallet() {
		// Set up SDK to access TestNet
		FlowConfig flowConfig = new DapperLabs.Flow.Sdk.FlowConfig()
		{
			NetworkUrl = "https://rest-testnet.onflow.org/v1",  // testnet
			Protocol = FlowConfig.NetworkProtocol.HTTP
		};
		FlowSDK.Init(flowConfig);

		IWallet walletProvider = ScriptableObject.CreateInstance<WalletConnectProvider>();
		walletProvider.Init(new WalletConnectConfig 
		{
			ProjectId = "4bfb21470f007a8e9ce8ce9b8026b422", // the Project ID from the previous step
			ProjectDescription = "Flow racer", // a description for your project
			ProjectIconUrl = "https://walletconnect.com/meta/favicon.ico", // URL for an icon for your project
			ProjectName = "Flow Racer", // the name of your project
			ProjectUrl = "https://flowracer.com" // URL for your project
		});
		FlowSDK.RegisterWalletProvider(walletProvider);
		
		// sh0x
		FlowSDK.GetWalletProvider().Authenticate("", (string flowAddress) => 
			{
				Debug.Log("Account: " + flowAddress);
				Debug.Log($"Authenticated - Flow account address is {flowAddress}");
				var account = walletProvider.GetAuthenticatedAccount();
				PlayerPrefs.SetString("flowAddress", flowAddress);
				if(CheckIfPrevLoggedIn(flowAddress))
				{
					ClientInfo.Username = prevLoggedInAccounts.GetUsernameByWallet(flowAddress);
					ClientInfo.WalletBalance = prevLoggedInAccounts.GetWalletBalanceByWallet(flowAddress);
					ClientInfo.UserWallet	= flowAddress;
				}
					
					// GetAllWagers();
					// SubmitTxMintNFT("1", "amazing url", "100");   						// NFT buy
					// ExecuteGetAllOwnedNFTs(flowAddress);
					// SubmitAcceptWager("0", "1");				// Wager
					// GetAllTotalWagers();
					// DistributeWager("0", flowAddress);			// End game
				if(UIScreen.activeScreen == mainMenu)
				{
					if(!CheckIfPrevLoggedIn(flowAddress))
					{
						ClientInfo.UserWallet = flowAddress;
						airdropRaceTokens();
					}
				}
			}, () => 
			{
				Debug.Log("Authentication failed.");
			});
	}

	private async void airdropRaceTokens()
	{
		popUp.SetDataAndOpen("200 $FR", "You have been awarded 200 Flow Race Token, depositing in your account", "");
		var awarded = await SubmitTxMintToken("200.0");
		if(awarded)
		{
			popUp.Close();
			ClientInfo.WalletBalance += 200f;
		}
		profileSetup.AssertProfileSetup();
		mainMenuUIScreen.onLoggedIn();
	}

	private bool CheckIfPrevLoggedIn(string address)
	{
		Debug.Log("Checking pre login");
		if(prevLoggedInAccounts == null)
			return false;
		
		Debug.Log("Checking pre login1");
		foreach(var account in prevLoggedInAccounts.accounts)
		{
			Debug.Log("Checking pre login2");
			if(account.UserWallet == address)
				return true;
		}

		return false;
	}


    private void ResetOwnedItems()
	{
		//save user data
		var userData = new AccountInfo {
			Username = ClientInfo.Username,
			UserWallet = ClientInfo.UserWallet,
			WalletBalance = ClientInfo.WalletBalance
		};

		prevLoggedInAccounts.accounts.Add(userData);

		PlayerPrefs.SetString("prevLoggedInAccounts", JsonUtility.ToJson(prevLoggedInAccounts));

		//clear user data
		ClientInfo.Username = string.Empty;
		ClientInfo.UserWallet = string.Empty;
		ClientInfo.KartId = 0;
		ClientInfo.LobbyName = string.Empty;
		ClientInfo.WalletBalance = 0;
		PlayerPrefs.DeleteKey("flowAddress");
		foreach (CarDefinition car in ResourceManager.Instance.carDefinitions)
		{
			car.IsOwned = false;
		}
	}

	// Audio Hooks
	public void SetVolumeMaster(float value) => AudioManager.SetVolumeMaster(value);
	public void SetVolumeSFX(float value) => AudioManager.SetVolumeSFX(value);
	public void SetVolumeUI(float value) => AudioManager.SetVolumeUI(value);
	public void SetVolumeMusic(float value) => AudioManager.SetVolumeMusic(value);
}