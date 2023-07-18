using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPowerup : SpawnedPowerup {
    public override void Init(KartEntity spawner) {
        base.Init(spawner);
        
        spawner.Controller.GiveBoost(false, 2);
        
        // Runner.Despawn(Object, true);
        // Destroy(gameObject);
    }

    public override void Spawned() {
        base.Spawned();

        Runner.Despawn(Object, true);
    }

    public override void PredictedSpawnSpawned() { }
    public override void PredictedSpawnUpdate() { }
    public override void PredictedSpawnRender() { }
    public override void PredictedSpawnFailed() { }
    public override void PredictedSpawnSuccess() { }
}
