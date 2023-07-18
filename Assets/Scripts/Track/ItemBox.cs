using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class ItemBox : NetworkBehaviour, ICollidable {
    
    public GameObject model;
    public ParticleSystem breakParticle;
    public float cooldown = 5f;
    public Transform visuals;

    [Networked(OnChanged = nameof(OnKartChanged))] public KartEntity Kart { get; set; }
    [Networked] public TickTimer DisabledTimer { get; set; }

    public bool Collide(KartEntity kart) {
        if ( kart != null && DisabledTimer.ExpiredOrNotRunning(Runner) ) {
            Kart = kart;
            DisabledTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
            var powerUp = GetRandomPowerup();
            Kart.SetHeldItem(powerUp);
        }

        return true;
    }

    private static void OnKartChanged(Changed<ItemBox> changed) { changed.Behaviour.OnKartChanged(); }
    private void OnKartChanged() {
        
        visuals.gameObject.SetActive(Kart == null);

        if ( Kart == null )
            return;

        AudioManager.PlayAndFollow(
            Kart.HeldItem != null ? "itemCollectSFX" : "itemWasteSFX",
            transform,
            AudioManager.MixerTarget.SFX
        );

        breakParticle.Play();
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();
        
        if (DisabledTimer.ExpiredOrNotRunning(Runner) && Kart != null) {
            Kart = null;
        }
    }

    private int GetRandomPowerup() {
        var powerUps = ResourceManager.Instance.powerups;
        var seed = Runner.Simulation.Tick;
        
        Random.InitState(seed);
        
        return Random.Range(0, powerUps.Length);
    }
}