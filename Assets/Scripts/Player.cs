using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();

    public override void OnNetworkSpawn()
    {
        // Add "onStateChanged" to be called every time the value of Position is changed
        Position.OnValueChanged += OnStateChanged; 

        if (IsOwner)
        {
            Move();
        }
    }

    public override void OnNetworkDespawn()
    {
        // when we remove ourself as a player we dont want updates on the Position
        Position.OnValueChanged -= OnStateChanged;
    }

    public void OnStateChanged(Vector2 previous, Vector2 current)
    {
        // current is the same as Position.Value, this event is triggered
        // to tell you it has been updated
        if (current != previous)
        {
            transform.position = current;
        }
    }

    public void Move()
    {
        SubmitPositionRequestServerRpc();
    }

    [Rpc(SendTo.Server)] // attribute, meta data so other functions know what kind of function this is. 
    void SubmitPositionRequestServerRpc(RpcParams rpcParams = default)
    {
        var randomPosition = GetRandomPosition();
        transform.position = randomPosition;
        Position.Value = randomPosition;
    }

    static Vector2 GetRandomPosition()
    {
        return new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
    }
}
