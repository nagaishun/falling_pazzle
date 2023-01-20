using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuyoType
{
    Blank = 0,

    Green = 1,
    Red = 2,
    Yellow = 3,
    Blue = 4,



    Invalid = 5,
};

[RequireComponent(typeof(Renderer))]
public class Puyo_Controller : MonoBehaviour
{
    static readonly Color[] color_table = new Color[]
    {
        Color.black,

        Color.green,
        Color.red,
        Color.yellow,
        Color.blue,


        Color.gray,
    };

    [SerializeField] Renderer my_renderer = default!;
    PuyoType _type = PuyoType.Invalid;



    public void SetPuyoType(PuyoType type)
    {
        _type = type;

        my_renderer.material.color = color_table[(int)_type];
    }
    public PuyoType GetPuyoType()
    {
        return _type;
    }

    public void SetPos(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }
}
