using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "New Powerup", menuName = "Scriptable Object/Powerup")]
public class Powerup : ScriptableObject
{
	public string itemName;
	public Sprite itemIcon;
	public SpawnedPowerup prefab;
    
    public void Use(NetworkRunner runner, KartEntity user) {
        if ( prefab != null ) {
            //
            // The prediction key is the unique identification of this prefab that is to be spawned. Both the local client
            // and server create (hopefully identical) keys, to create a link between the server object and local object.
            // See more: https://doc.photonengine.com/en-US/fusion/current/manual/spawning#spawn_prediction
            //
            var predictionKey = new NetworkObjectPredictionKey() {
                Byte0 = (byte)(runner.Simulation.Tick % 256),
                Byte1 = (byte)user.Object.InputAuthority
            };

            var position = user.itemDropNode.position;
            var rotation = user.itemDropNode.rotation;

            var obj = runner.Spawn(prefab, position, rotation, null, null, predictionKey);
            obj.Init(user);
        }
    }
}