using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextQueue
{
    private enum Constants
    {
        PUYO_TYPE_MAX = 4,      // 出現する色の種類の数
        PUYO_NEXT_HISTORIES = 2,// NEXTの個数
    };

    Queue<Vector2Int> _nexts = new Queue<Vector2Int>();

    Vector2Int CreateNext()
    {
        return new Vector2Int(
            Random.Range(0, (int)Constants.PUYO_TYPE_MAX) + 1,
            Random.Range(0, (int)Constants.PUYO_TYPE_MAX) + 1);

    }

    public void Initialize()
    {
        // キューをPUYO_NEXT_HISTORIESセットの乱数で満たす
        for (int t = 0; t < (int)Constants.PUYO_NEXT_HISTORIES; t++)
        {
            _nexts.Enqueue(CreateNext());
        }
    }

    public Vector2Int Update()
    {
        // 先頭を出して、後ろに新しい乱数セットを追加
        Vector2Int next = _nexts.Dequeue();
        _nexts.Enqueue(CreateNext());

        return next;
    }

    public void Each(System.Action<int, Vector2Int> cb)
    {
        int idx = 0;
        foreach (Vector2Int n in _nexts)
        {
            cb(idx++, n);
        }
    }
}
