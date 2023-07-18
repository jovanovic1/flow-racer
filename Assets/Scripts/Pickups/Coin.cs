using Fusion;
using UnityEngine;

public class Coin : NetworkBehaviour, ICollidable {

    [Networked(OnChanged = nameof(OnIsEnabledChangedCallback))]
    public NetworkBool IsActive { get; set; } = true;

    public Transform visuals;

    public bool Collide(KartEntity kart) {
        if ( IsActive ) {
            kart.CoinCount++;

            IsActive = false;
            
            if ( kart.Object.HasStateAuthority ) {
                Runner.Despawn(Object);
            }
        }

        return true;
    }

    private static void OnIsEnabledChangedCallback(Changed<Coin> changed) {
        var behaviour = changed.Behaviour;
        behaviour.visuals.gameObject.SetActive(behaviour.IsActive);

        if ( !behaviour.IsActive )
            AudioManager.PlayAndFollow("coinSFX", behaviour.transform, AudioManager.MixerTarget.SFX);
    }
}
