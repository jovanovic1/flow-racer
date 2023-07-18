using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class KartEntity : KartComponent
{
	public static event Action<KartEntity> OnKartSpawned;
	public static event Action<KartEntity> OnKartDespawned;

    public event Action<int> OnHeldItemChanged;
    public event Action<int> OnCoinCountChanged;
    
	public KartAnimator Animator { get; private set; }
	public KartCamera Camera { get; private set; }
	public KartController Controller { get; private set; }
	public KartInput Input { get; private set; }
	public KartLapController LapController { get; private set; }
	public KartAudio Audio { get; private set; }
	public GameUI Hud { get; private set; }
	public KartItemController Items { get; private set; }
	public NetworkRigidbody Rigidbody { get; private set; }

	public Powerup HeldItem =>
		HeldItemIndex == -1
			? null
			: ResourceManager.Instance.powerups[HeldItemIndex];

    [Networked(OnChanged = nameof(OnHeldItemIndexChangedCallback))]
	public int HeldItemIndex { get; set; } = -1;

	[Networked(OnChanged = nameof(OnCoinCountChangedCallback))]
	public int CoinCount { get; set; }

	public Transform itemDropNode;

    private bool _despawned;

	private static void OnHeldItemIndexChangedCallback(Changed<KartEntity> changed)
	{
		changed.Behaviour.OnHeldItemChanged?.Invoke(changed.Behaviour.HeldItemIndex);

		if (changed.Behaviour.HeldItemIndex != -1)
		{
			foreach (var behaviour in changed.Behaviour.GetComponentsInChildren<KartComponent>())
				behaviour.OnEquipItem(changed.Behaviour.HeldItem, 3f);
		}
	}

	private static void OnCoinCountChangedCallback(Changed<KartEntity> changed)
	{
		changed.Behaviour.OnCoinCountChanged?.Invoke(changed.Behaviour.CoinCount);
	}

	private void Awake()
	{
		// Set references before initializing all components
		Animator = GetComponentInChildren<KartAnimator>();
		Camera = GetComponent<KartCamera>();
		Controller = GetComponent<KartController>();
		Input = GetComponent<KartInput>();
		LapController = GetComponent<KartLapController>();
		Audio = GetComponentInChildren<KartAudio>();
		Items = GetComponent<KartItemController>();
		Rigidbody = GetComponent<NetworkRigidbody>();

		// Initializes all KartComponents on or under the Kart prefab
		var components = GetComponentsInChildren<KartComponent>();
		foreach (var component in components) component.Init(this);
	}

	public static readonly List<KartEntity> Karts = new List<KartEntity>();

	public override void Spawned()
	{
		base.Spawned();
		
		if (Object.HasInputAuthority)
		{
			// Create HUD
			Hud = Instantiate(ResourceManager.Instance.hudPrefab);
			Hud.Init(this);

			Instantiate(ResourceManager.Instance.nicknameCanvasPrefab);
		}

		Karts.Add(this);
		OnKartSpawned?.Invoke(this);
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		base.Despawned(runner, hasState);
		Karts.Remove(this);
		_despawned = true;
		OnKartDespawned?.Invoke(this);
	}

	private void OnDestroy()
	{
		Karts.Remove(this);
		if (!_despawned)
		{
			OnKartDespawned?.Invoke(this);
		}
	}

    private void OnTriggerStay(Collider other) {
        if (other.TryGetComponent(out ICollidable collidable))
        {
            collidable.Collide(this);
        }
    }

    public bool SetHeldItem(int index)
	{
		if (HeldItem != null) return false;
        
		HeldItemIndex = index;
		return true;
	}

	public void SpinOut()
	{
		Controller.IsSpinout = true;
	}

	private IEnumerable OnSpinOut()
	{
		yield return new WaitForSeconds(2f);

		Controller.IsSpinout = false;
	}
}