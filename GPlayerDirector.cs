using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;

    NextQueue _nextQueue = new NextQueue();
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();

        _nextQueue.Initialize();
        Spawn(_nextQueue.Update());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
        }
    }

    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
