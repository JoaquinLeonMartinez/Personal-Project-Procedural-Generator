using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DirectionState { open, block, empty};
public enum RoomType { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, X};
public enum Door { Up, Down, Right, Left};

public class Room : MonoBehaviour
{
    //Backtraking:
    public RoomType type;
    public Dictionary<Door, bool> doorDictionary = new Dictionary<Door, bool>(); //true es que hay puerta y false es que no hay o ya se esta usando

    public Room(RoomType _type)
    {
        this.type = _type;
        SetupDoors(_type);
    }

    private void SetupDoors(RoomType type)
    {
        switch (type)
        {
            case RoomType.A:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Up, true);
                break;
            case RoomType.B:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Left, false);
                break;
            case RoomType.C:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Down,true);
                doorDictionary.Add(Door.Left,true);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.D:
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Right, false);
                break;
            case RoomType.E:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Down, false);
                break;
            case RoomType.F:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.G:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.H:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Down, false);
                break;
            case RoomType.I:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Down, false);
                break;
            case RoomType.J:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Down, false);
                break;
            case RoomType.K:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Down, false);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.L:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.M:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Down, false);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.N:
                doorDictionary.Add(Door.Right, true);
                doorDictionary.Add(Door.Left, true);
                doorDictionary.Add(Door.Down, false);
                doorDictionary.Add(Door.Up, false);
                break;
            case RoomType.O:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Down, true);
                doorDictionary.Add(Door.Up, true);
                doorDictionary.Add(Door.Left, false);
                break;
            case RoomType.P:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Up, false);
                doorDictionary.Add(Door.Down, false);
                break;
            case RoomType.X:
                doorDictionary.Add(Door.Right, false);
                doorDictionary.Add(Door.Left, false);
                doorDictionary.Add(Door.Up, false);
                doorDictionary.Add(Door.Down, false);
                break;
        }
    }

    //Backtracking:
    public bool isValid(DirectionState directionState, Door door)
    {
        if (directionState == DirectionState.block && this.doorDictionary[door])
        {
            //eliminar
            return false;
        }
        else if (directionState == DirectionState.open && !this.doorDictionary[door])
        {
            //eliminar
            return false;
        }

        return true;
    }

}
