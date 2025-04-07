using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();
    public NetworkVariable<int> SpriteIndex = new NetworkVariable<int>();
    public Sprite[] sprites = new Sprite[3];
    SpriteRenderer spRend;
    float speed = 5f;

    public void Start()
    {
        // For dynamically spawned objects such as player prefabs this is called after
        // OnNetworkSpawn, so we can read the value of the SpriteIndex NetworkVariable
        // and not have to use a OnValueChanged callback as it is only ever set,
        // never modified after. 
        spRend = GetComponent<SpriteRenderer>();
        spRend.sprite = sprites[SpriteIndex.Value];
    }
    public override void OnNetworkSpawn()
    {
        // Add "onStateChanged" to be called every time the value of Position is changed
        Position.OnValueChanged += OnStateChanged;

        if (IsServer)
        {
            SpriteIndex.Value = UnityEngine.Random.Range(0, 3);
            Position.Value = new Vector2(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5));
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
        if (Position.Value != previous)
        {
            transform.position = Position.Value;
        }
    }

    enum Direction
    {
        LEFT, RIGHT, UP, DOWN
    }
    public void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                SubmitPositionRequestServerRpc(Direction.LEFT);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                SubmitPositionRequestServerRpc(Direction.RIGHT);
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
               SubmitPositionRequestServerRpc(Direction.UP);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                SubmitPositionRequestServerRpc(Direction.DOWN);
            }
        }
    }

    [Rpc(SendTo.Server)] // attribute, meta data so other functions know what kind of function this is. 
    void SubmitPositionRequestServerRpc(Direction direction)
    {
       //So why are we doing it here instead of Update, 1. Using RPC,
       //2. Anti cheat, incase we have obsticals so the server can say no
        Vector2 position = transform.position;

        if (direction == Direction.LEFT)
        {
            position.x -= speed * Time.deltaTime;
        }
        else if (direction == Direction.RIGHT)
        {
            position.x += speed * Time.deltaTime;
        }

        else if (direction == Direction.UP)
        {
            position.y += speed * Time.deltaTime;
        }
        else if (direction == Direction.DOWN)
        {
            position.y -= speed * Time.deltaTime;
        }

        Position.Value = position;
    }

    //static Vector2 GetRandomPosition()
    //{
    //    return new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
    //}
}
