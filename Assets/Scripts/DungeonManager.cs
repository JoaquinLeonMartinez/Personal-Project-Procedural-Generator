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

    #region OLD_VERSION_ATTRIBUTES
    int checkedRooms = 0;
    int ramificaciones = 1;
    #endregion

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

    #region OLD_VERSION

    /*public void GenerateDungeonBase()
    {
        //La primera sala no puede ser del tipo P, que es el ultimo en el array, de modo que generamos un random hasta el .count - 1
        //La generamos en el (0,0,0)
        var currentRoom = predefinedRooms[Random.Range(0, predefinedRooms.Count - 1)];
        currentRoom = Instantiate(currentRoom, new Vector3(0, 0, 0), currentRoom.transform.rotation);
        //currentRoom.GetComponent<Room>().SetDoors();
        roomsToGenerate--;
        Debug.Log($"Acabamos de generar la primera room (tipo {currentRoom.GetComponent<Room>().type.ToString()})");

        //Ahora generamos el resto de rooms
        while (roomsToGenerate > 0)
        {
            Debug.Log($"Quedan {roomsToGenerate} por generar");
            // Antes de nada necesitamos saber que salas son compatibles con cada puerta
            Debug.Log($"La room actual tiene {currentRoom.GetComponent<Room>().doors.Count} puertas");

            for (int j = 0; j < currentRoom.GetComponent<Room>().doors.Count; j++)
            {
                if (roomsToGenerate == 0)
                {
                    GameObject roomToInstantiate = predefinedRooms[predefinedRooms.Count - 1]; //Si no quedan rooms completamos las aperturas con rooms de tipo P
                    Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distanceP), roomToInstantiate.transform.rotation);
                }
                else
                {
                    Debug.Log($"Vamos a buscar rooms compatibles con la puerta {currentRoom.GetComponent<Room>().doors[j].ToString()}");
                    List<GameObject> compatibleRooms = GetCompatibleRooms(currentRoom.GetComponent<Room>().doors[j]);
                    //Debug.Log($"Hemos obtenido {compatibleRooms.Count} rooms compatibles.");
                    if (compatibleRooms.Count != 0) //esto se va a dar siempre realmente
                    {
                        //Seleccionamos una de ellas de forma aleatoria
                        GameObject roomToInstantiate = compatibleRooms[Random.Range(0, compatibleRooms.Count)];
                        Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distance), roomToInstantiate.transform.rotation);
                        Debug.Log($"Hemos instanciado una room de tipo {roomToInstantiate.GetComponent<Room>().type.ToString()}");//, con una rotacion de {roomToInstantiate.transform.rotation.x}, {roomToInstantiate.transform.rotation.y}, {roomToInstantiate.transform.rotation.z}");
                    }
                }
                roomsToGenerate--;
            }
        }
    }
    
    
    public void SetPredefinedRooms()
    {
        foreach (var item in predefinedRooms)
        {
            item.GetComponent<Room>().ClearDoors();
            item.GetComponent<Room>().ClearConnections();
            item.GetComponent<Room>().SetDoors();
        }
    }
    
         public void GenerateDungeonLinealImproved()
    {
        //generar todo menos las P
        //para determinar si hay un bloqueo debe encontrar obstaculos en todas sus puertas
        //si encuentra un bloqueo vuelve a empezar desde otra

        var currentRoom = predefinedRooms[Random.Range(0, predefinedRooms.Count - 1)];
        currentRoom = Instantiate(currentRoom, new Vector3(0, 0, 0), currentRoom.transform.rotation);
        currentRoom.transform.parent = dungeonContainer.transform;
        roomsToGenerate--;
        bool isClose = false;
        //Ahora generamos el resto de rooms
        while (!isClose)
        {

            for (int j = 0; j < currentRoom.GetComponent<Room>().doors.Count; j++)
            {
                if (!isBlockedSimplify(currentRoom, currentRoom.GetComponent<Room>().doors[j]))//si la puerta no esta bloqueada
                {
                    List<GameObject> compatibleRooms = GetCompatibleRooms(currentRoom.GetComponent<Room>().doors[j]);
                    GenerateRoomNormal(currentRoom, compatibleRooms[Random.Range(0, compatibleRooms.Count)], GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distance), j);
                    //j--; 
                }
                else
                {

                }
            }
        }

        if (roomsToGenerate <= 0)
        {
            isClose = true;
        }
        else if(currentRoom == generatedRooms[generatedRooms.Count - 1])
        {
            //bloqueo
            isClose = true;
        }
        else
        {
            currentRoom = generatedRooms[generatedRooms.Count - 1];
        }

    }

        public void GenerateDungeonLinealImprovedFail() //not working
    {
        //La primera sala no puede ser del tipo P, que es el ultimo en el array, de modo que generamos un random hasta el .count - 1
        //La generamos en el (0,0,0)
        GameObject lastRoom = null;
        var currentRoom = predefinedRooms[Random.Range(0, predefinedRooms.Count - 1)];
        currentRoom = Instantiate(currentRoom, new Vector3(0, 0, 0), currentRoom.transform.rotation);
        currentRoom.transform.parent = dungeonContainer.transform;
        //currentRoom.GetComponent<Room>().SetDoors();
        roomsToGenerate--;
        Debug.Log($"Acabamos de generar la primera room (tipo {currentRoom.GetComponent<Room>().type.ToString()})");
        bool isClose = false;
        //Ahora generamos el resto de rooms
        while (!isClose)
        {
            //Debug.Log($"Quedan {roomsToGenerate} por generar");
            // Antes de nada necesitamos saber que salas son compatibles con cada puerta
            //Debug.Log($"La room actual tiene {currentRoom.GetComponent<Room>().doors.Count} puertas");
            bool firstRoomGenerated = false; //primera room conectada a otra

            if (roomsToGenerate == 0)//esto indica que este sera el ultimo loop y que solo generaremos p
            {
                //Debug.Log($"Ya no se generara nada que no sea de tipo P");
                isClose = true; //para cuando acabe el loop estara cerrado el circuito
                firstRoomGenerated = true;
            }

            bool finished = false;

            for (int j = 0; j < currentRoom.GetComponent<Room>().doors.Count && !finished; j++)
            {
                //Debug.Log($"Puertas posibles = {currentRoom.GetComponent<Room>().doors.Count}");
                bool isWayBlocked = isBlocked(currentRoom, currentRoom.GetComponent<Room>().doors[j]);
                if (firstRoomGenerated || isWayBlocked)
                {
                    //Las tipo P no cuentan como room, realmente no puedes entrar
                    if (isWayBlocked)
                    {
                        //En caso de estar bloqueado aqui acaba todo, destruimos la current y la sustituimos por una room P 
                        GenerateEndRoom(currentRoom);
                        finished = true;
                        isClose = true;
                    }
                    else //si el camino no esta bloqueado instanciamos una room P y listo
                    {
                        GenerateRoomP(currentRoom, predefinedRooms[predefinedRooms.Count - 1], GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distanceP), j);
                    }
                }
                else
                {
                    //Debug.Log($"Vamos a buscar rooms compatibles con la puerta {currentRoom.GetComponent<Room>().doors[j].ToString()}");
                    List<GameObject> compatibleRooms = GetCompatibleRooms(currentRoom.GetComponent<Room>().doors[j]);
                    //Debug.Log($"Hemos obtenido {compatibleRooms.Count} rooms compatibles.");
                    if (compatibleRooms.Count != 0) //esto se va a dar siempre realmente
                    {
                        //Generamos la room
                        lastRoom = GenerateRoomNormal(currentRoom, compatibleRooms[Random.Range(0, compatibleRooms.Count)], GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distance), j);
                        //Debug.Log($"Desactivamos la puerta {Room.GetComplementaryDoor(currentRoom.GetComponent<Room>().doors[j])} de la nueva room");
                        firstRoomGenerated = true;
                        roomsToGenerate--;
                        j--; //como borramos una puerta hay que decrementar este valor
                    }
                    else
                    {
                        Debug.Log($"La puerta {currentRoom.GetComponent<Room>().doors[j]} no es compatible con nada");
                    }
                }

            }


            if (currentRoom != lastRoom && !isClose) //comprobamos que la ultima generada no sea la actual, eso significaria que no se ha hecho ninguna conexion (caso del else)
            {
                //Cuando acabamos con las puertas de una room, vamos con las de la siguiente
                currentRoom = lastRoom;
                //Debug.Log($"Next room = {currentRoom.GetComponent<Room>().type}");
            }
            else if (roomsToGenerate != 0)
            {
                //Si llegamos aqui significa que nos hemos bloqueado, no se puede seguir generando la mazmorra
                //Debug.Log($"Hemos llegado a un bloqueo, quedan {roomsToGenerate} rooms por generar");

                //En este caso buscaremos otra room compatible
                
                currentRoom = FindOtherPath();

                if (checkedRooms >= generatedRooms.Count || currentRoom == null)// si no quedan compatibles pues nos aguantamos
                {
                    Debug.Log($"Hemos llegado a un bloqueo critico, checked rooms = {checkedRooms} and generated rooms = {generatedRooms.Count}");
                    isClose = true;
                }
                else
                {
                    isClose = false;
                }
            }
        }

        Debug.Log($"Acaba con roomsToGenerate = {roomsToGenerate}");
    }

    public GameObject FindOtherPath()
    {
        GameObject roomToGenerate = null;
        bool enc = false;

        for (int i = checkedRooms; i < generatedRooms.Count && !enc; i++)
        {
            //si tiene alguna room P hijo la borra y devuelve true
            if (generatedRooms[i].GetComponent<Room>().DeleteRoomsP())
            {
                roomToGenerate = generatedRooms[i];
                Debug.LogError($"Hemos encontrado una nueva room por la que empezar de tipo {generatedRooms[i].GetComponent<Room>().type}");
                enc = true;
            }
            checkedRooms = i; //para no volver a buscar en la misma
        }

        return roomToGenerate;
    }

    
    public GameObject GenerateRoomNormal(GameObject currentRoom, GameObject roomToInstantiate, Vector3 position, int doorPosition)
    {
        var generatedRoom = Instantiate(roomToInstantiate, position, roomToInstantiate.transform.rotation);
        generatedRoom.transform.parent = dungeonContainer.transform;
        generatedRoom.GetComponent<Room>().SetUp(currentRoom, Room.GetComplementaryDoor(currentRoom.GetComponent<Room>().doors[doorPosition])); // no pasa anda porque lo llamemos con P, no hara anda  y ya esta
        currentRoom.GetComponent<Room>().connections.Add(generatedRoom);
        generatedRooms.Add(generatedRoom);
        Debug.Log($"Hemos instanciado una room de tipo {roomToInstantiate.GetComponent<Room>().type.ToString()} con parent tipo {currentRoom.GetComponent<Room>().type} en la puerta {currentRoom.GetComponent<Room>().doors[doorPosition]} (la cual vamos a desactivar), y la añadimos al array de generated");

        currentRoom.GetComponent<Room>().DisableDoor(currentRoom.GetComponent<Room>().doors[doorPosition]);//desactivamos la puerta en la que hemos generado al hijo
        roomsToGenerate--;

        return generatedRoom;
    }

    public void GenerateRoomP(GameObject currentRoom, GameObject roomToInstantiate, Vector3 position, int doorPosition)
    {
        currentRoom.GetComponent<Room>().connections.Add(Instantiate(roomToInstantiate,position , roomToInstantiate.transform.rotation));
        currentRoom.GetComponent<Room>().connections[currentRoom.GetComponent<Room>().connections.Count - 1].transform.parent = dungeonContainer.transform;
        currentRoom.GetComponent<Room>().connections[currentRoom.GetComponent<Room>().connections.Count - 1].GetComponent<Room>().SetParent(currentRoom);
        Debug.Log($"Hemos instanciado una room de tipo {roomToInstantiate.GetComponent<Room>().type.ToString()} con parent tipo {currentRoom.GetComponent<Room>().type} en la puerta {currentRoom.GetComponent<Room>().doors[doorPosition]}");
    }

    public void GenerateEndRoom(GameObject currentRoom)
    {
        //Antes de borrar la current room, habra que borrar todos sus hijos :')
        //Limpiamos las conexiones de la actual
        currentRoom.GetComponent<Room>().ClearConnections();
        //En la posicion de la que vamos a destruir, generamos una torre
        GameObject roomToInstantiate = predefinedRooms[predefinedRooms.Count - 1];
        var endRoom = Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().entryDoor, distance - distanceP), roomToInstantiate.transform.rotation);
        endRoom.transform.parent = dungeonContainer.transform;
        endRoom.GetComponent<Room>().SetParent(currentRoom.GetComponent<Room>().parent);//le decimos a la room quien es su padre
        currentRoom.GetComponent<Room>().parent.GetComponent<Room>().connections.Add(endRoom); //le decimos al padre que tiene un nuevo hijo
        Debug.Log($"Hemos borrado la room {currentRoom.GetComponent<Room>().type} y finalizamos");
        //Destruimos la actual
        Destroy(currentRoom);
        generatedRooms.RemoveAt(generatedRooms.Count -1); // lo quitamos de la lista
        roomsToGenerate++; //hemos destruido una que ya habiamos restado
        Debug.Log($"Rooms generadas (sin P) antes del bloqueo: {generatedRooms.Count}, quedan {roomsToGenerate} por generar");

    }


    public void GenerateDungeonLineal()
    {
        //La primera sala no puede ser del tipo P, que es el ultimo en el array, de modo que generamos un random hasta el .count - 1
        //La generamos en el (0,0,0)
        GameObject lastRoom = null;
        var currentRoom = predefinedRooms[Random.Range(0, predefinedRooms.Count - 1)];
        currentRoom = Instantiate(currentRoom, new Vector3(0, 0, 0), currentRoom.transform.rotation);
        currentRoom.transform.parent = dungeonContainer.transform;
        //currentRoom.GetComponent<Room>().SetDoors();
        roomsToGenerate--;
        Debug.Log($"Acabamos de generar la primera room (tipo {currentRoom.GetComponent<Room>().type.ToString()})");
        bool isClose = false;
        //Ahora generamos el resto de rooms
        while (!isClose)
        {
            //Debug.Log($"Quedan {roomsToGenerate} por generar");
            // Antes de nada necesitamos saber que salas son compatibles con cada puerta
            //Debug.Log($"La room actual tiene {currentRoom.GetComponent<Room>().doors.Count} puertas");
            bool firstRoomGenerated = false; //primera room conectada a otra

            if (roomsToGenerate == 0)//esto indica que este sera el ultimo loop y que solo generaremos p
            {
                //Debug.Log($"Ya no se generara nada que no sea de tipo P");
                isClose = true; //para cuando acabe el loop estara cerrado el circuito
                firstRoomGenerated = true;
            }

            bool finished = false;

            for (int j = 0; j < currentRoom.GetComponent<Room>().doors.Count && !finished; j++)
            {
                //Debug.Log($"Puertas posibles = {currentRoom.GetComponent<Room>().doors.Count}");
                bool isWayBlocked = isBlocked(currentRoom, currentRoom.GetComponent<Room>().doors[j]);
                if (firstRoomGenerated || isWayBlocked)
                {
                    //Las tipo P no cuentan como room, realmente no puedes entrar
                    if (isWayBlocked)
                    {
                        //En caso de estar bloqueado aqui acaba todo, destruimos la current y la sustituimos por una room P 
                        //Antes de borrar la current room, habra que borrar todos sus hijos :')
                        currentRoom.GetComponent<Room>().ClearConnections();
                        //En la posicion de la que vamos a destruir, generamos una torre
                        GameObject roomToInstantiate = predefinedRooms[predefinedRooms.Count - 1];
                        var endRoom = Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().entryDoor, distance - distanceP), roomToInstantiate.transform.rotation);
                        endRoom.transform.parent = dungeonContainer.transform;
                        endRoom.GetComponent<Room>().SetParent(currentRoom.GetComponent<Room>().parent);//le decimos a la room quien es su padre
                        currentRoom.GetComponent<Room>().parent.GetComponent<Room>().connections.Add(endRoom); //le decimos al padre que tiene un nuevo hijo
                        Debug.Log($"Hemos borrado la room {currentRoom.GetComponent<Room>().type} y finalizamos");
                        //Destruimos la actual
                        Destroy(currentRoom);
                        //salimos de este bucle, pero no del otro
                        finished = true;
                        isClose = true;


                    }
                    else //si el camino no esta bloqueado instanciamos una y listo
                    {
                        GameObject roomToInstantiate = predefinedRooms[predefinedRooms.Count - 1]; //Si no quedan rooms completamos las aperturas con rooms de tipo P
                        currentRoom.GetComponent<Room>().connections.Add(Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distanceP), roomToInstantiate.transform.rotation));
                        currentRoom.GetComponent<Room>().connections[currentRoom.GetComponent<Room>().connections.Count - 1].transform.parent = dungeonContainer.transform;
                        currentRoom.GetComponent<Room>().connections[currentRoom.GetComponent<Room>().connections.Count - 1].GetComponent<Room>().SetParent(currentRoom);
                        Debug.Log($"Hemos instanciado una room de tipo {roomToInstantiate.GetComponent<Room>().type.ToString()} con parent tipo {currentRoom.GetComponent<Room>().type} en la puerta {currentRoom.GetComponent<Room>().doors[j]}");
                    }
                }
                else
                {
                    //Debug.Log($"Vamos a buscar rooms compatibles con la puerta {currentRoom.GetComponent<Room>().doors[j].ToString()}");
                    List<GameObject> compatibleRooms = GetCompatibleRooms(currentRoom.GetComponent<Room>().doors[j]);
                    //Debug.Log($"Hemos obtenido {compatibleRooms.Count} rooms compatibles.");
                    if (compatibleRooms.Count != 0) //esto se va a dar siempre realmente
                    {
                        //Seleccionamos una de ellas de forma aleatoria
                        GameObject roomToInstantiate = compatibleRooms[Random.Range(0, compatibleRooms.Count)];
                        lastRoom = Instantiate(roomToInstantiate, GeneratePosition(currentRoom.transform, currentRoom.GetComponent<Room>().doors[j], distance), roomToInstantiate.transform.rotation);
                        lastRoom.transform.parent = dungeonContainer.transform;
                        lastRoom.GetComponent<Room>().SetUp(currentRoom, Room.GetComplementaryDoor(currentRoom.GetComponent<Room>().doors[j]));
                        currentRoom.GetComponent<Room>().connections.Add(lastRoom);
                        Debug.Log($"Hemos instanciado una room de tipo {roomToInstantiate.GetComponent<Room>().type.ToString()} con parent tipo {currentRoom.GetComponent<Room>().type} en la puerta {currentRoom.GetComponent<Room>().doors[j]}");//, con una rotacion de {roomToInstantiate.transform.rotation.x}, {roomToInstantiate.transform.rotation.y}, {roomToInstantiate.transform.rotation.z}");
                        //Debug.Log($"Desactivamos la puerta {Room.GetComplementaryDoor(currentRoom.GetComponent<Room>().doors[j])} de la nueva room");
                        firstRoomGenerated = true;
                        roomsToGenerate--;
                    }
                    else
                    {
                        Debug.Log($"La puerta {currentRoom.GetComponent<Room>().doors[j]} no es compatible con nada");
                    }
                }
                
            }
            
            if (currentRoom != lastRoom)
            {
                //Cuando acabamos con las puertas de una room, vamos con las de la siguiente
                currentRoom = lastRoom;
                //Debug.Log($"Next room = {currentRoom.GetComponent<Room>().type}");
            }
            else if(roomsToGenerate != 0)
            {
                //Si llegamos aqui significa que nos hemos bloqueado, no se puede seguir generando la mazmorra
                Debug.Log($"Hemos llegado a un bloqueo, quedan {roomsToGenerate} rooms por generar");
                isClose = true;
            } 
        }

        //Debug.Log($"Sale del bucle con isClose = {isClose} y roomsToGenerate = {roomsToGenerate}");
    }


    //Devolverá una lista de rooms compatibles con una puerta de la room ya creada
    public List<GameObject> GetCompatibleRooms(Door door)
    {
        List<GameObject> compatibleRooms = new List<GameObject>();

        for (int i = 0; i < predefinedRooms.Count; i++)
        {
            
            if (predefinedRooms[i].GetComponent<Room>().IsCompatible(door))
            {
                //Debug.Log($"La room {predefinedRooms[i].GetComponent<Room>().type} es compatible");
                compatibleRooms.Add(predefinedRooms[i]);
            }
        }

        return compatibleRooms;
    }

    
    public Vector3 GeneratePosition(Transform parentTransform, Door door, float distance)
    {
        Vector3 position = Vector3.zero;
        switch (door)
        {
            case Door.Right:
                return new Vector3(parentTransform.position.x, parentTransform.position.y, parentTransform.position.z + distance);
            case Door.Down:
                return new Vector3(parentTransform.position.x + distance, parentTransform.position.y, parentTransform.position.z);
            case Door.Left:
                return new Vector3(parentTransform.position.x, parentTransform.position.y, parentTransform.position.z - distance);
            case Door.Up:
                return new Vector3(parentTransform.position.x - distance, parentTransform.position.y, parentTransform.position.z);
        }

        return position;
    }

    public bool isBlockedSimplify(GameObject currentRoom, Door door) //Devuelve true si choca con algo
    {
        Vector3 origin = Vector3.zero;
        Vector3 target1 = Vector3.zero;
        float maxDistance = 10f; ;
        float offsetY = 3f;
        float offset = 0f; //a cuanta distancia del centro se lanza el raycast
        float offsetTarget = distance / 2;

        switch (door)
        {
            case Door.Right:
                origin = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z + offset);
                target1 = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, currentRoom.transform.position.z + distance * 2);
                break;
            case Door.Down:
                origin = new Vector3(currentRoom.transform.position.x + offset, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z);
                target1 = new Vector3(currentRoom.transform.position.x + distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z);
                break;
            case Door.Left:
                origin = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z - offset);
                target1 = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, currentRoom.transform.position.z - distance * 2);
                break;
            case Door.Up:
                origin = new Vector3(currentRoom.transform.position.x - offset, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z);
                target1 = new Vector3(currentRoom.transform.position.x - distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z);
                break;
        }

        //Debug.Log($"Vamos a comprobar si esta bloqueada la puerta {door} de la room {currentRoom.GetComponent<Room>().type}");
        //En un futuro podria ser util si queremos ver con que elementos choca
        RaycastHit hit;
        if (Physics.Raycast(origin, (target1 - origin).normalized, out hit, maxDistance))
        {
            //Ha colisionado con algo
            Debug.Log($"(Raycast 1) La puerta {door} de la room {currentRoom.GetComponent<Room>().type} esta bloqueada con la room {hit.collider.GetComponent<Room>().type}");
            return true;
        }

        return false;
    }

    public bool isBlocked(GameObject currentRoom, Door door) //Devuelve true si choca con algo
    {
        Vector3 origin = Vector3.zero;
        Vector3 target1 = Vector3.zero; //hacemos tres raycasts
        Vector3 target2 = Vector3.zero;
        Vector3 target3 = Vector3.zero;
        float maxDistance = 10f; ;
        float offsetY = 3f;
        float offset = 0f; //a cuanta distancia del centro se lanza el raycast
        float offsetTarget = distance / 2;

        switch (door)
        {
            case Door.Right:
                origin = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z + offset);
                target1 = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, currentRoom.transform.position.z + distance * 2);
                target2 = new Vector3(currentRoom.transform.position.x + offsetTarget, currentRoom.transform.position.y, currentRoom.transform.position.z + distance * 2);
                target3 = new Vector3(currentRoom.transform.position.x - offsetTarget, currentRoom.transform.position.y, currentRoom.transform.position.z + distance * 2);
                break;
            case Door.Down:
                origin = new Vector3(currentRoom.transform.position.x + offset, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z);
                target1 = new Vector3(currentRoom.transform.position.x + distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z);
                target2 = new Vector3(currentRoom.transform.position.x + distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z + offsetTarget);
                target3 = new Vector3(currentRoom.transform.position.x + distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z - offsetTarget);
                break;
            case Door.Left:
                origin = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z - offset);
                target1 = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, currentRoom.transform.position.z - distance * 2);
                target2 = new Vector3(currentRoom.transform.position.x + offsetTarget, currentRoom.transform.position.y, currentRoom.transform.position.z - distance * 2);
                target3 = new Vector3(currentRoom.transform.position.x - offsetTarget, currentRoom.transform.position.y, currentRoom.transform.position.z - distance * 2);
                break;
            case Door.Up:
                origin = new Vector3(currentRoom.transform.position.x - offset, currentRoom.transform.position.y + offsetY, currentRoom.transform.position.z);
                target1 = new Vector3(currentRoom.transform.position.x - distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z);
                target2 = new Vector3(currentRoom.transform.position.x - distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z + offsetTarget);
                target3 = new Vector3(currentRoom.transform.position.x - distance * 2, currentRoom.transform.position.y, currentRoom.transform.position.z - offsetTarget);
                break;
        }

        //Debug.Log($"Vamos a comprobar si esta bloqueada la puerta {door} de la room {currentRoom.GetComponent<Room>().type}");
        //En un futuro podria ser util si queremos ver con que elementos choca
        RaycastHit hit;
        if (Physics.Raycast(origin, (target1 - origin).normalized, out hit, maxDistance))
        {
            //Ha colisionado con algo
            Debug.Log($"(Raycast 1) La puerta {door} de la room {currentRoom.GetComponent<Room>().type} esta bloqueada con la room {hit.collider.GetComponent<Room>().type}");
        }
        else if (Physics.Raycast(origin, (target2 - origin).normalized, out hit, maxDistance))
        {
            Debug.Log($"(Raycast 2) La puerta {door} de la room {currentRoom.GetComponent<Room>().type} esta bloqueada con la room {hit.collider.GetComponent<Room>().type}");
        }
        else if (Physics.Raycast(origin, (target3 - origin).normalized, out hit, maxDistance))
        {
            Debug.Log($"(Raycast 3) La puerta {door} de la room {currentRoom.GetComponent<Room>().type} esta bloqueada con la room {hit.collider.GetComponent<Room>().type}");
        }

        //TODO: Devolver la distancia del que choca con algo, si es menor de una torre pequeña no pasa nada
        return Physics.Raycast(origin, (target3 - origin).normalized, out hit, maxDistance) || Physics.Raycast(origin, (target2 - origin).normalized, out hit, maxDistance) || Physics.Raycast(origin, (target1 - origin).normalized, out hit, maxDistance);
    }

     */

    #endregion
}
