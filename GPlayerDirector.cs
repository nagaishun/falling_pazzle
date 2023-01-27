using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogicalInput _logicalInput = new LogicalInput();

    NextQueue _nextQueue = new NextQueue();
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        Spawn(_nextQueue.Update());
    }

    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogicalInput.Key.MAX]
    {
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.X,
        KeyCode.Z,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
    };

    // ì¸óÕÇéÊÇËçûÇﬁ
    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        // ÉLÅ[ì¸óÕéÊìæ
        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
            }
        }

        _logicalInput.Update(inputDev);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ì¸óÕÇéÊÇËçûÇﬁ
        UpdateInput();

        if (!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
        }
    }

    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
