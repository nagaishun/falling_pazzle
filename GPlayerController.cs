using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 移動速度遷移時間
    const int TRANS_TIME = 3;
    // 回転遷移時間
    const int ROT_TIME = 3;

    // 落下制御
    // ひとマス落下するカウント数
    const int FALL_COUNT_UNIT = 120;
    // 落下速度
    const int FALL_COUNT_SPD = 10;
    // 高速落下時の速度
    const int FALL_COUNT_FAST_SPD = 20;
    // 接地後移動可能時間
    const int GROUND_FRAMES = 50;


    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1,
    }

    [SerializeField] Puyo_Controller[] _puyoControllers = new Puyo_Controller[2] { default!, default! };
    [SerializeField] BoardController boardController = default!;

    Vector2Int _position;
    RotState _rotate = RotState.Up;

    AnimationController _animationController = new AnimationController();
    Vector2Int _last_position;
    RotState _last_rotate = RotState.Up;

    LogicalInput logicalInput = new LogicalInput();

    // 落下制御
    int _fallCount = 0;
    // 接地時間
    int _groundFrame = GROUND_FRAMES;

    // Start is called before the first frame update
    void Start()
    {
        //ひとまず決め打ちで色を設定
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);
        _rotate = RotState.Up;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
    {
        return pos + rotate_tbl[(int)rot];
    }
    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos))
        {
            return false;
        }
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot)))
        {
            return false;
        }
        return true;
    }

    void SetTransition(Vector2Int pos, RotState rot, int time)
    {
        // 補間のために保存しておく
        _last_position = _position;
        _last_rotate = _rotate;

        // 値の更新
        _position = pos;
        _rotate = rot;

        _animationController.Set(time);
    }

    private bool Translate(bool is_right)
    {
        //仮想的に移動できるか検証する
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate))
        {
            return false;
        }

        //実際に移動
        SetTransition(pos, _rotate, TRANS_TIME);

        return true;
    }

    bool Rotate(bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? +1 : +3)) & 3);

        //仮想的に移動できるか検証する(上下左右にずらした時も確認)
        Vector2Int pos = _position;

        switch (rot)
        {
            case RotState.Down:
                // 右(左)から下 : 自分の下か右(左)下にブロックがあれば引きあがる
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                   !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : -1, -1)))
                {
                    pos += Vector2Int.up;
                }
                break;
            case RotState.Right:
                // 右 : 右が埋まっていれば、左に移動
                if (!boardController.CanSettle(pos + Vector2Int.right))
                {
                    pos += Vector2Int.left;
                }
                break;
            case RotState.Left:
                // 左 : 左が埋まっていれば、右に移動
                if (!boardController.CanSettle(pos + Vector2Int.left))
                {
                    pos += Vector2Int.right;
                }
                break;
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }

        if (!CanMove(pos, rot))
        {
            return false;
        }

        //実際に移動
        SetTransition(pos, rot, ROT_TIME);

        return true;
    }

    void Settle()
    {
        // 直接接地
        bool is_set0 = boardController.Settle(_position, (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate), (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);

        gameObject.SetActive(false);
    }

    void QuickDrop()
    {
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;

        _position = pos;

        Settle();
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

        logicalInput.Update(inputDev);
    }

    bool Fall(bool is_fast)
    {
        _fallCount -= is_fast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        // ブロックを飛び越えたら、行けるのかチェック
        // ブロックが飛ぶ可能性がもないこともない気がするので複数落下に対応
        while (_fallCount < 0)
        {
            if (!CanMove(_position + Vector2Int.down, _rotate))
            {
                // 落ちれないなら
                // 動きを止める
                _fallCount = 0;

                // 時間があるなら、移動・回転可能
                if (0 < --_groundFrame)
                {
                    return true;
                }

                // 時間切れになったら本当に固定
                Settle();
                return false;
            }

            // 落ちれるなら下に進む
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }

        return true;
    }

    void Control()
    {
        // 落とす
        if (!Fall(logicalInput.IsRaw(LogicalInput.Key.Down)))
        {
            // 接地したら終了
            return;
        }

        // アニメーション中はキー入力を受け付けない
        if (_animationController.Update())
        {
            return;
        }

        //平行移動のキー入力取得
        if (logicalInput.IsRepeat(LogicalInput.Key.Right))
        {
            if (Translate(true))
            {
                return;
            }
        }
        if (logicalInput.IsRepeat(LogicalInput.Key.Left))
        {
            if (Translate(false))
            {
                return;
            }
        }

        //回転のキー入力取得
        if (logicalInput.IsTrigger(LogicalInput.Key.RotR))
        {
            if (Rotate(true))
            {
                return;
            }
        }
        if (logicalInput.IsTrigger(LogicalInput.Key.RotL))
        {
            if (Rotate(false))
            {
                return;
            }
        }
        //クイックドロップのキー入力取得
        if (logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }

    private void FixedUpdate()
    {
        // 入力を取り込む
        UpdateInput();

        // 操作を受けて動かす
        Control();

        // 表示
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;
        float anim_rate = _animationController.GetNormalized();
        _puyoControllers[0].SetPos(dy + Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(dy + Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));
    }


    // rateが 1 -> 0 で、pos_last -> pos, rot_last -> rot に遷移。rot が Rotstate.Invalid なら回転を考慮しない
    static Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int pos_last, RotState rot_last, float rate)
    {
        // 平行移動
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f), rate);

        if (rot == RotState.Invalid)
        {
            return p;
        }

        // 回転
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;

        // 近い方向に回る
        if (+Mathf.PI < theta)
        {
            theta = theta - 2.0f * Mathf.PI;
        }
        if (theta < -Mathf.PI)
        {
            theta = theta + 2.0f * Mathf.PI;
        }

        theta = theta0 + rate * theta;

        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }
}
