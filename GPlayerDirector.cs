using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

interface IState
{
    public enum E_State
    {
        Control = 0,
        GameOver = 1,
        Falling = 2,
        Erasing = 3,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayDirector parent);
    E_State Update(PlayDirector parent);
}

[RequireComponent(typeof(BoardController))]
public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogicalInput _logicalInput = new LogicalInput();
    BoardController _boardController = default!;

    // ��next�̃Q�[���I�u�W�F�N�g�𐧌�
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };

    NextQueue _nextQueue = new NextQueue();

    // ��ԊǗ�
    IState.E_State _current_state = IState.E_State.Falling;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new GameOverState(),
        new FallingState(),
        new ErasingState(),
    };

    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _boardController = GetComponent<BoardController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        // ��Ԃ̏�����
        InitializeState();
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

    // ���͂���荞��
    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;

        // �L�[���͎擾
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
        // ���͂���荞��
        UpdateInput();

        UpdateState();
    }
    class ControlState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            if (!parent.Spawn(parent._nextQueue.Update()))
            {
                return IState.E_State.GameOver;
            }

            parent.UpdateNextsView();
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent.player.activeSelf ? IState.E_State.Unchanged : IState.E_State.Falling;
        }
    }

    class GameOverState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            // ���g���C
            SceneManager.LoadScene(0);
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return IState.E_State.Unchanged;
        }
    }

    class FallingState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            return parent._boardController.CheckFall() ? IState.E_State.Unchanged : IState.E_State.Erasing;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent._boardController.Fall() ? IState.E_State.Unchanged : IState.E_State.Erasing;
        }
    }

    class ErasingState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            return parent._boardController.CheckErase() ? IState.E_State.Unchanged : IState.E_State.Control;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent._boardController.Erase() ? IState.E_State.Unchanged : IState.E_State.Falling;
        }
    }

    void InitializeState()
    {
        // Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Initialize(this);

        if (next_state != IState.E_State.Unchanged)
        {
            _current_state = next_state;
            // �������ŏ�Ԃ��ς��悤�Ȃ�A�ċA�I�ɏ������Ăяo��
            InitializeState();
        }
    }

    void UpdateState()
    {
        var next_state = states[(int)_current_state].Update(this);
        if (next_state != IState.E_State.Unchanged)
        {
            // ���̏�ԂɑJ��
            _current_state = next_state;
            InitializeState();
        }
    }
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}

