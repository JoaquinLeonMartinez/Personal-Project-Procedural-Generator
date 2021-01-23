using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonManager : MonoBehaviour
{
    //Singleton
    private static DungeonManager instance = null;
    public static DungeonManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    //Distancia entre centros de salas
    public static float distance = 4.5f;
    public static float distanceP = 3.25f;

    // Array de salas predefinidas
    public List<GameObject> predefinedRooms = new List<GameObject>();

    // Nº de salas
    [System.NonSerialized]
    public int roomsToGenerate = 2;
    //public TMP_InputField roomsToGenerateInput;
    public GameObject dungeonContainer;
    //La utilizaremos para ir guardando las roomsque generemos que no sean de tipo P
    List<GameObject> generatedRooms = new List<GameObject>();

    //Backtraking
    public List<Room> grid;
    int sizeX = 4;
    int sizeY = 4;

    public void GenerateDungeon()
    {
        GenerateDungeonBacktrackingManager();
    }

    public void GenerateDungeonBacktrackingManager()
    {
        AdaptGrid();
        InitGrid(); // ponemos toda la grid a X
        PrintGrid();
        Debug.Log($"Generamos mazmorras con rooms to generate = {roomsToGenerate}");
        //Backtracking
        if (!GenerateDungeonBacktracking(grid, 0, roomsToGenerate))
        {
            Debug.LogError("No hay solucion");
        }
        else
        {
            CloseOpenPaths(grid);
            GridToDungeonExport(grid);
        }
    }

    public void CloseOpenPaths(List<Room> grid)
    {
        for (int i = 0; i < grid.Count; i++)
        {
            if (grid[i].type == RoomType.X) //si es tipo X y tiene alguna puerta abierta se pone
            {
                Dictionary<Door, DirectionState> dictionaryDirections = new Dictionary<Door, DirectionState>();
                //Cosas a tener en cuenta a la hora de chequear:

                //Chequear arriba
                dictionaryDirections.Add(Door.Up, CheckDirection(grid, i, Door.Up));
                //Chequear derecha
                dictionaryDirections.Add(Door.Right, CheckDirection(grid, i, Door.Right));
                //Chequear abajo
                dictionaryDirections.Add(Door.Down, CheckDirection(grid, i, Door.Down));
                //Chequear izquierda
                dictionaryDirections.Add(Door.Left, CheckDirection(grid, i, Door.Left));

                if (dictionaryDirections[Door.Up] == DirectionState.open && dictionaryDirections[Door.Left] == DirectionState.open)
                {
                    grid[i] = new Room(RoomType.H);
                }
                else if(dictionaryDirections[Door.Up] == DirectionState.open && dictionaryDirections[Door.Right] == DirectionState.open)
                {
                    grid[i] = new Room(RoomType.I);
                }
                else if (dictionaryDirections[Door.Down] == DirectionState.open && dictionaryDirections[Door.Right] == DirectionState.open)
                {
                    grid[i] = new Room(RoomType.F);
                }
                else if (dictionaryDirections[Door.Down] == DirectionState.open && dictionaryDirections[Door.Left] == DirectionState.open)
                {
                    grid[i] = new Room(RoomType.G);
                }
                else if (dictionaryDirections[Door.Up] == DirectionState.open)
                {
                    //ponemos la J
                    grid[i] = new Room(RoomType.J);
                }
                else if (dictionaryDirections[Door.Right] == DirectionState.open)
                {
                    //ponemos la K
                    grid[i] = new Room(RoomType.K);
                }
                else if (dictionaryDirections[Door.Down] == DirectionState.open)
                {
                    //ponemos la L
                    grid[i] = new Room(RoomType.L);
                }
                else if (dictionaryDirections[Door.Left] == DirectionState.open)
                {
                    //ponemos la M
                    grid[i] = new Room(RoomType.M);
                }
            }
        }
        PrintGrid();
    }
    public void AdaptGrid()//adaptamos la grid a las rooms que hay por generar
    {
        if(roomsToGenerate > (sizeX * sizeY) / 2)
        {
            sizeY = (int) Mathf.Sqrt(roomsToGenerate*2) + 1;
            sizeX = sizeY;
            Debug.Log($"Adaptamos la grid, ahora es de tamaño {sizeY*sizeX} posiciones");
        }
    }

    public void InitGrid()
    {
        grid.Clear();
        for (int i = 0; i < (sizeX*sizeY); i++)
        {
            grid.Add(new Room(RoomType.X));
        }
    }

    public void PrintGrid()
    {
        string printedGrid = "Grid: ";

        for (int i = 0; i < grid.Count; i++)
        {
            if (i % sizeY == 0) //esto significa que cambia de fila
            {
                printedGrid += "\n " + grid[i].type;
            }
            else
            {
                printedGrid += " " + grid[i].type;
            }
        }
        Debug.Log($" {printedGrid}");
    }

    public void GridToDungeonExport(List<Room> grid)
    {
        for (int i = 0; i < grid.Count; i++)
        {
            if (grid[i].type != RoomType.X)
            {
                var roomToInstantiate = GetRoomPrefab(grid[i].type);
                var currentRoom = Instantiate(roomToInstantiate, GetPositionWorld(i), roomToInstantiate.transform.rotation);
                currentRoom.transform.parent = dungeonContainer.transform;
            }

        }
    }
    
    public Vector3 GetPositionWorld(int positionGrid)
    {
        return new Vector3((positionGrid / sizeY) * distance, 0, (positionGrid % sizeY) * distance);
    }

    public GameObject GetRoomPrefab(RoomType type)
    {
        GameObject target = null;
        bool enc = false;

        for (int i = 0; i < predefinedRooms.Count && !enc; i++)
        {
            if (predefinedRooms[i].GetComponent<Room>().type == type)
            {
                enc = true;
                target = predefinedRooms[i];
            }
        }

        return target;
    }

    public bool GenerateDungeonBacktracking(List<Room> grid, int position, int roomsToGenerate)
    {
        if (roomsToGenerate <= 0)
        {
            return true; //esto indica que hemos llegado al final y lo hemos petado (en plan bien)
        }

        if (position >= grid.Count)
        {
            return false; //hemos llegado al final y esta solucion no era buena
        }

        List<Room> arrayPosiblesOpciones = GetCompatibles(grid, position);

        bool isOk = false;
        bool firstTime = true;
        for (int i = 0; i < arrayPosiblesOpciones.Count && !isOk; i++)
        {
            grid[position] = arrayPosiblesOpciones[i];
            if (isNormalRoomType(arrayPosiblesOpciones[i].type) && firstTime)
            {
                roomsToGenerate--;
                firstTime = false;
            }
            Debug.Log($"Intentamos posicion {position} con {arrayPosiblesOpciones[i].type}, quedan {roomsToGenerate} rooms por generar");
            PrintGrid();
            isOk = GenerateDungeonBacktracking(grid, position + 1, roomsToGenerate);
        }

        if (isOk)
        {
            Debug.Log($"La casilla {position} se ha generado con una {grid[position].type}");
            //Extra: generar aqui todas las P (de esta room) //Ahora mismo lo hace despues del backtracking
            return true;
        }
        else
        {
            grid[position].type = RoomType.X;
            return false;
        }
    }

    public bool isNormalRoomType(RoomType type)
    {
        bool isValid = true;
        //Si en un futuro tengo las salas J,K, L, M se quitarian del IF porque pasariamos a contarlos
        if (type == RoomType.X || type == RoomType.J || type == RoomType.K || type == RoomType.L || type == RoomType.M)
        {
            isValid = false;
        }

        return isValid;
    }

    public List<Room> GetCompatibles(List<Room> grid, int position)
    {
        List<Room> compatibles = new List<Room>();
        Dictionary<Door, DirectionState> dictionaryDirections = new Dictionary<Door, DirectionState>();

        //Chequear arriba
        dictionaryDirections.Add(Door.Up, CheckDirection(grid, position, Door.Up));
        //Chequear derecha
        dictionaryDirections.Add(Door.Right, CheckDirection(grid, position, Door.Right));
        //Chequear abajo
        dictionaryDirections.Add(Door.Down, CheckDirection(grid, position, Door.Down));
        //Chequear izquierda
        dictionaryDirections.Add(Door.Left, CheckDirection(grid, position, Door.Left));

        //Comprobar si la conexion con room anterior existe (y no es el caso 0), en caso de no tener conexion ni siquiera intenta buscar compatibles, pone X
        if (ValidRoom(position, dictionaryDirections))
        {
            // con lo que devuelven necesitamos obtener una lista de rooms compatibles
            compatibles = GenerateOptions(dictionaryDirections);
        }
        else
        {
            //ponemos una X para que continue
            compatibles.Add(new Room(RoomType.X));
        }

        dictionaryDirections.Clear(); //lo limpiamos por si acaso (no es necesario al ser una variable del propio metodo)

        return compatibles;
    }

    public bool ValidRoom(int currentPos, Dictionary<Door, DirectionState> dictionaryDirections)
    {
        if (currentPos == 0)
        {
            return true;
        }

        if (dictionaryDirections[Door.Up] == DirectionState.open || dictionaryDirections[Door.Left] == DirectionState.open)
        {
            return true;
        }

        return false; ;
    }

    public DirectionState CheckDirection(List<Room> grid, int currentPos, Door direction)
    {
        //Las opciones son 3: que arriba no haya nada (X), que arriba haya algo que bloquee (final del array o room sin puerta), que haya una room con puerta)
        DirectionState state = DirectionState.block;

        int posToCheck = 0;
        int currentRow = 0;
        int posToCheckRow = 0;

        switch (direction)
        {
            case Door.Up:
                //Comprobamos la casilla de arriba, viniendo de abajo
                posToCheck = currentPos - sizeY;
                state = CheckPosition(grid, posToCheck, Door.Down);
                break;

            case Door.Right:
                posToCheck = currentPos + 1;
                currentRow = currentPos / sizeY;
                posToCheckRow = posToCheck / sizeY;
                //Debug.Log($"posToCheck = {posToCheck} == currentPos {currentPos}, pertenecen a las filas {posToCheckRow} == {currentRow}, sizeY == {sizeY}");
                if (currentRow == posToCheckRow)//esto siginifica que estan en la misma fila, por lo tanto comprobamos el resto de condiciones, sino bloqued
                {
                    state = CheckPosition(grid, posToCheck, Door.Left);
                }
                else
                {
                    state = DirectionState.block;
                }
                break;

            case Door.Down:
                posToCheck = currentPos + sizeY;
                state = CheckPosition(grid, posToCheck, Door.Up);
                break;

            case Door.Left:
                posToCheck = currentPos - 1;
                currentRow = currentPos / sizeY;
                posToCheckRow = posToCheck / sizeY;
                //Debug.Log($"posToCheck = {posToCheck} == currentPos {currentPos}, pertenecen a las filas {posToCheckRow} == {currentRow}, sizeY == {sizeY}");
                if (currentRow == posToCheckRow) //esto siginifica que estan en la misma fila, por lo tanto comprobamos el resto de condiciones, sino bloqued
                {
                    state = CheckPosition(grid, posToCheck, Door.Right);
                }
                else
                {
                    state = DirectionState.block;
                }
                break;
        }
        Debug.Log($"En pos {currentPos} la direccion {direction} se encuentra en estado {state}");
        return state;
    }

    public DirectionState CheckPosition(List<Room> grid, int posToCheck, Door door)
    {
        DirectionState state = DirectionState.block;

        if (posToCheck < 0 || posToCheck >= grid.Count)//con esto comprobamos arriba y abajo
        {
            state = DirectionState.block; //no existe la posicion
        }
        else if (grid[posToCheck].type == RoomType.X)
        {
            state = DirectionState.empty; //La room de arriba esta vacia
        }
        else if (grid[posToCheck].doorDictionary[door]) //TODO, ESTO NO DDEBERIA SER SIEMPRE DOWN
        {
            state = DirectionState.open; //hay una puerta con una puerta abierta, por lo tanto la room que vaya a qui debe tener una puerta abierta
        }
        else if (!grid[posToCheck].doorDictionary[door])
        {
            state = DirectionState.block; //hay una sala sin puerta, por lo tanto bloqueada
        }

        return state;
    }

    public List<Room> GenerateOptions(Dictionary<Door, DirectionState> dictionaryDirections)
    {
        List<Room> disponibleRooms = new List<Room>();

        //disponibleRooms.Add(new Room(RoomTypeBacktracking.A)); // no tengo esta pieza
        disponibleRooms.Add(new Room(RoomType.B));
        disponibleRooms.Add(new Room(RoomType.C));
        disponibleRooms.Add(new Room(RoomType.D));
        disponibleRooms.Add(new Room(RoomType.E));
        disponibleRooms.Add(new Room(RoomType.F));
        disponibleRooms.Add(new Room(RoomType.G));
        disponibleRooms.Add(new Room(RoomType.H));
        disponibleRooms.Add(new Room(RoomType.I));

        /*
        //Estas las generaremos al final
        disponibleRooms.Add(new Room(RoomTypeBacktracking.J));
        disponibleRooms.Add(new Room(RoomTypeBacktracking.K));
        disponibleRooms.Add(new Room(RoomTypeBacktracking.L));
        disponibleRooms.Add(new Room(RoomTypeBacktracking.M));
         */

        disponibleRooms.Add(new Room(RoomType.N));
        disponibleRooms.Add(new Room(RoomType.O));

        List<Room> compatibles = new List<Room>();

        //bucle añadiendo a compatibles todas las que tengan Up false
        compatibles = Filter(dictionaryDirections, disponibleRooms);

        //Randomize list
        ListOperations.Shuffle<Room>(compatibles);

        return compatibles;
    }

    public List<Room> Filter(Dictionary<Door, DirectionState> dictionaryDirections, List<Room> candidates)
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (!candidates[i].isValid(dictionaryDirections[Door.Up], Door.Up))
            {
                candidates.RemoveAt(i);
            }
            else if (!candidates[i].isValid(dictionaryDirections[Door.Right], Door.Right))
            {
                candidates.RemoveAt(i);
            } 
            else if (!candidates[i].isValid(dictionaryDirections[Door.Down], Door.Down))
            {
                candidates.RemoveAt(i);
            }
            else if (!candidates[i].isValid(dictionaryDirections[Door.Left], Door.Left))
            {
                candidates.RemoveAt(i);
            }
        }

        Debug.Log($"Acabamos de filtrar todas las direcciones, hay {candidates.Count} candidatos");
        for (int i = 0; i < candidates.Count; i++)
        {
            Debug.Log($"Candidato {i}: {candidates[i].type}");
        }

        return candidates;
    }

    public void ResetDungeon()
    {
        foreach (Transform child in dungeonContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        generatedRooms.Clear();
    }
}
