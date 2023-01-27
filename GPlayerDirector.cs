using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogicalInput _logicalInput = new LogicalInput();

    // 次nextのゲームオブジェクトを制御
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };

    NextQueue _nextQueue = new NextQueue();
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        Spawn(_nextQueue.Update());
        UpdateNextsView();
    }

    void UpdateNextsView()
    {
        _nextQueue.Each((int idx, Vector2Int n) =>
        {
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
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

    // 入力を取り込む
    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        // キー入力取得
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
        // 入力を取り込む
        UpdateInput();

        if (!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
            UpdateNextsView();
        }
    }

    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
