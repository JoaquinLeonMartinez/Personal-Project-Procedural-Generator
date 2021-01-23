using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DirectionState { open, block, empty};
public enum RoomType { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, X};
public enum Door { Up, Down, Right, Left};

public class Room : MonoBehaviour
{
    #region OLD_VERSIONS
    /*
    public enum RoomType { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P};
    public RoomType type;
    int lastRoomChecked = 0;
        public List<Door> doors = new List<Door>();
    public Door entryDoor;

    //Rooms hijo
    public List<GameObject> connections = new List<GameObject>();
    public GameObject parent;
    */
    #endregion

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

    #region OLD_VERSION
    /*
    
        public static Door GetComplementaryDoor(Door door)
    {
        switch (door)
        {
            case Door.Right:
                return Door.Left;
            case Door.Down:
                return Door.Up;
            case Door.Left:
                return Door.Right;
            case Door.Up:
                return Door.Down;
        }

        return Door.Down; //nunca deberia llegar a este punto
    }

    public void ClearDoors()
    {
        doors.Clear();
    }
    public int GetOpenDoors(RoomTypeBacktracking type)
    {
        //TODO: no va a funcionar, no tiene porque ser de la izquierda las que cierran, podrian ser de la derecha
        int openDoors = 0;
        switch (type) //la derecha y abajo suman, las izquierdas y arriba restan
        {
            case RoomTypeBacktracking.A:
                openDoors = 0; //Sumamos 2 pero tambien restamos dos
                break;
            case RoomTypeBacktracking.B:
                openDoors = 1; //restamos una, sumamos dos
                break;
            case RoomTypeBacktracking.C:
                openDoors = 1; //restamos una, sumamos dos
                break;
            case RoomTypeBacktracking.D:
                openDoors = -1; //restamos dos, sumamos una
                break;
            case RoomTypeBacktracking.E:
                openDoors = -1; //restamos dos, sumamos una
                break;
            case RoomTypeBacktracking.F:
                openDoors = 2; //suma dos
                break;
            case RoomTypeBacktracking.G:
                openDoors = 0; //+1-1
                break;
            case RoomTypeBacktracking.H:
                openDoors = -2; //-2
                break;
            case RoomTypeBacktracking.I:
                openDoors = 0; //+1-1
                break;
            case RoomTypeBacktracking.J:
                openDoors = -1; // - 1
                break;
            case RoomTypeBacktracking.K:
                openDoors = -1; // - 1
                break;
            case RoomTypeBacktracking.L:
                openDoors = -1; // - 1
                break;
            case RoomTypeBacktracking.M:
                openDoors = -1; // - 1
                break;
            case RoomTypeBacktracking.N:
                openDoors = 0; //+1 - 1
                break;
            case RoomTypeBacktracking.O:
                openDoors = 0; //+1 - 1
                break;
            case RoomTypeBacktracking.P:
                break;
            case RoomTypeBacktracking.X:
                break;
        }

        return openDoors;
    }

    //Indica si la room se podría conectar a la puerta indicada, es decir, devuelve true cuando tiene la puerta contraria
    public bool IsCompatible(Door door)
    {
        Door deshiredDoor = GetComplementaryDoor(door);

        foreach (var item in doors)
        {
            if (item == deshiredDoor)
            {
                //Debug.Log($"La puerta {door} es compatible con la puerta {item}");
                return true;
            }
            else
            {
                //Debug.Log($"La puerta {door} NO es compatible con la puerta {item}");
            }
        }
        return false;
    }
        //Indica si una room tiene una puerta en un lugar determinado
    public bool DoorExists(Door door)
    {
        foreach (var item in doors)
        {
            if (item == door)
            {
                return true;
            }
        }

        return false;
    }

    public void SetDoors()
    {
        switch (type)
        {
            case RoomType.A:
                doors.Add(Door.Right);
                doors.Add(Door.Down);
                doors.Add(Door.Left);
                doors.Add(Door.Up);
                break;
            case RoomType.B:
                doors.Add(Door.Right);
                doors.Add(Door.Down);
                doors.Add(Door.Up);
                break;
            case RoomType.C:
                doors.Add(Door.Right);
                doors.Add(Door.Down);
                doors.Add(Door.Left);
                break;
            case RoomType.D:
                doors.Add(Door.Down);
                doors.Add(Door.Left);
                doors.Add(Door.Up);
                break;
            case RoomType.E:
                doors.Add(Door.Right);
                doors.Add(Door.Left);
                doors.Add(Door.Up);
                break;
            case RoomType.F:
                doors.Add(Door.Right);
                doors.Add(Door.Down);
                break;
            case RoomType.G:
                doors.Add(Door.Down);
                doors.Add(Door.Left);
                break;
            case RoomType.H:
                doors.Add(Door.Left);
                doors.Add(Door.Up);
                break;
            case RoomType.I:
                doors.Add(Door.Right);
                doors.Add(Door.Up);
                break;
            case RoomType.J:
                doors.Add(Door.Up);
                break;
            case RoomType.K:
                doors.Add(Door.Right);
                break;
            case RoomType.L:
                doors.Add(Door.Down);
                break;
            case RoomType.M:
                doors.Add(Door.Left);
                break;
            case RoomType.N:
                doors.Add(Door.Right);
                doors.Add(Door.Left);
                break;
            case RoomType.O:
                doors.Add(Door.Down);
                doors.Add(Door.Up);
                break;
            case RoomType.P:
                break;
        }
    }
        public void ClearConnections()
    {
        foreach (var item in connections)
        {
            Destroy(item);
        }
    }


    public void SetParent(GameObject _parent)
    {
        parent = _parent;
    }

    public void DisableDoor(Door door)
    {
        bool enc = false;
        int i = 0;
        for (i = 0; i < doors.Count && !enc; i++)
        {
            if (doors[i] == door)
            {
                enc = true;
                //entryDoor = doors[i];
                break;
            }
        }

        if (enc)
        {
            doors.RemoveAt(i);
        }
        
    }


    
    public void SetUp(GameObject _parent, Door door)
    {
        if (type != RoomType.P)
        {
            SetParent(_parent);
            entryDoor = door; //nos fiamos de que sea correcta
            DisableDoor(door);
        }
    }

        public bool CheckIfAvailableSlots()
    {
        return lastRoomChecked < connections.Count;
    }

    public bool DeleteRoomsP()
    {
        bool enc = false;

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].GetComponent<Room>().type == RoomType.P)
            {
                enc = true;
                //borramos
                Destroy(connections[i].GetComponent<Room>());
                connections.RemoveAt(i);
                i--;
            }
        }

        return enc;
    }

    //Con esto comprobaremos si tiene alguna room P como hijo, si lo tiene la devolvemos
    public GameObject GetRoomP()
    {
        bool enc = false;
        int i;
        for (i = lastRoomChecked; i < connections.Count && !enc; i++)
        {
            if (connections[i].GetComponent<Room>().type == RoomType.P)
            {
                lastRoomChecked = i;
                enc = true;
            }
        }

        lastRoomChecked = i;

        return connections[lastRoomChecked];
    }
     */

    #endregion
}
