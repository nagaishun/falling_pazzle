using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PyoType
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
}
