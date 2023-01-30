using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct FallData
{
    public readonly int X { get; }
    public readonly int Y { get; }
    // 落ちる先
    public readonly int Dest { get; }

    public FallData(int x, int y, int dest)
    {
        X = x;
        Y = y;
        Dest = dest;
    }
}
public class BoardController : MonoBehaviour
{
    // 単位セル当たりの落下フレーム数
    public const int FALL_FRAME_PER_CELL = 5;

    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;

    [SerializeField] GameObject prefabPuyo = default!;

    int[,] _board = new int[BOARD_HEIGHT, BOARD_WIDTH];
    GameObject[,] _Puyos = new GameObject[BOARD_HEIGHT, BOARD_WIDTH];

    // 落ちる際の一次的変数
    List<FallData> _falls = new List<FallData>();
    int _fallFrames = 0;

    private void ClearAll()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                _board[y, x] = 0;

                if (_Puyos[y, x] != null)
                {
                    Destroy(_Puyos[y, x]);
                }
                _Puyos[y, x] = null;
            }
        }
    }

    void Start()
    {
        ClearAll();

        //for (int y = 0; y < BOARD_HEIGHT; y++)
        //{
        //    for (int x = 0; x < BOARD_WIDTH; x++)
        //    {
        //        Settle(new Vector2Int(x, y), Random.Range(1, 5));
        //    }
        //}
    }

    public static bool IsValidated(Vector2Int pos)
    {
        return 0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT;
    }

    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValidated(pos))
        {
            return false;
        }
        return 0 == _board[pos.y, pos.x];
    }

    public bool Settle(Vector2Int pos, int val)
    {
        if (!CanSettle(pos))
        {
            return false;
        }

        _board[pos.y, pos.x] = val;

        Debug.Assert(_Puyos[pos.y, pos.x] == null);
        Vector3 world_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, world_position, Quaternion.identity, transform);
        _Puyos[pos.y, pos.x].GetComponent<Puyo_Controller>().SetPuyoType((PuyoType)val);

        return true;
    }

    public bool CheckFall()
    {
        return true;
    }

    public bool Fall()
    {
        return true;
    }
}
