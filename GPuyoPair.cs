using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoPair : MonoBehaviour
{
    [SerializeField] Puyo_Controller[] puyos = { default!, default! };

    public void SetPuyoType(PuyoType axis, PuyoType child)
    {
        puyos[0].SetPuyoType(axis);
        puyos[1].SetPuyoType(child);
    }
}
