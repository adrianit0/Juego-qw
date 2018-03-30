using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RECURSOS {
    //RECURSOS
    Madera, Piedra, Cobre, Plata, Oro,
    //COMIDA
    Manzana, ManzanaDorada, Zanahoria, Pescado,
    //OTROS OBJETOS
    Tabla, Ladrillo
}

public enum TIPORECURSO {
    Bruto, Refinado, Comida
}

public enum TIPOACCION { Talar, Construir, Investigar, Cocinar, Minar, Cosechar, Almacenar, Pescar, Socializar, Arar, SacarAlmacen, VaciarAlmacen, RecogerObjeto, Destruir, ExtraerAgua, Regar, Plantar, Craftear }
public enum HERRAMIENTA { Seleccionar = 0, Recolectar = 1, Arar = 2, Priorizar = 3, Destruir = 4, Cancelar = 5, Construir = 6, Custom = 7 }

public class GameManager : MonoBehaviour, IEquipo {
    public IntVector2 totalSize = new IntVector2 (100, 100);

    //CONTENIDO PARTIDA
    public Inventario inventario;

    //Iconos de construccion
    public IconInfo[] iconos = new IconInfo[9];
    public Sprite[] iconosPrioridad;
    public Slider barraprioridad;

    //Lista de construcciones en el juego
    private Dictionary<ESTRUCTURA, List<Estructura>> builds;
    //Lista de personajes jugables
    public List<Personaje> characters;
    //Lista de NPC
    //public List<Personaje> NPC;

    //PANEL HERRAMIENTAS
    public HERRAMIENTA herramientaSeleccionada = HERRAMIENTA.Seleccionar;
    public Sprite[] iconosHerramientas = new Sprite[4];
    public Image imagenCentral;

    // PREFABS
    public GameObject nodoPrefab;
    public GameObject actionIconPrefab;

    public GameObject sackPrefab;
    public GameObject waterPrefab;
    
    //PANEL RECURSOS
    public GameObject[] panelesRecursos = new GameObject[2];
    
    Node[,] map;
    public Dictionary<Node, SpriteRenderer> tiles { get; private set; }
    
    public Sprite[] spriteTierra = new Sprite[16];
    public Sprite spriteAgua;
    
    //CLASES SERIALIZADAS
    //Clases sin monoBehaviour.
    public PathFinding path { get; private set; }
    public ActionManager actions { get; private set; }

    //Clases con monoBehaviour.
    public Agricultura farm { get; private set; }
    public Construccion build { get; private set; }
    public Artesania craft { get; private set; }
    public ManagementManager management { get; private set; }
    public Informacion info { get; private set; }
    public CharacterInterfaceController characterController { get; private set; }

    public ResourceController resourceController { get; private set; }
    public UIManager interfaz { get; private set; }
    
    /*public GameManager (int width = 100, int height = 100) {
        totalSize = new IntVector2(width, height);
        CrearMapa();

        inventario = new Inventario(int.MaxValue);
        inventario.SetInterface((IEquipo) this);
    }*/

    void Awake() {
        builds = new Dictionary<ESTRUCTURA, List<Estructura>>();
        characters = new List<Personaje>();

        farm = GetComponent<Agricultura>();
        build = GetComponent<Construccion>();
        management = GetComponent<ManagementManager>();
        craft = GetComponent<Artesania>();
        info = GetComponent<Informacion>();
        characterController = GetComponent<CharacterInterfaceController>();

        path = new PathFinding(this);
        actions = new ActionManager(this);

        resourceController = FindObjectOfType<ResourceController>();
        interfaz = FindObjectOfType<UIManager>();

        //TODO: 
        //Poner esto en el Start cuando no haya estructuras pregeneradas.
        tiles = new Dictionary<Node, SpriteRenderer>();

        CrearMapa();

        inventario = new Inventario(int.MaxValue, this);
        inventario.SetInterface((IEquipo) this);
    }

    public void AddCharacter(Personaje character) {
        characters.Add(character);

        characterController.characters.AñadirPersonaje(character);
    }

    public Node GetNode (int x, int y) {
        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y)
            return null;

        return map[x, y];
    }

    public Node GetNode (IntVector2 pos) {
        return GetNode(pos.x, pos.y);
    }
	
    void CrearMapa () {
        map = new Node[totalSize.x, totalSize.y];
        for (int x = 0; x < totalSize.x; x++) {
            for (int y = 0; y < totalSize.y; y++) {
                GameObject _obj = Instantiate (nodoPrefab);
                _obj.transform.position = new Vector3(x, y, 0);
                _obj.transform.parent = this.transform;

                Node node = new Node(x, y, this);
                map[x, y] = node;

                SpriteRenderer render = _obj.GetComponent<SpriteRenderer>();
                render.sprite = spriteTierra[4];
                
                render.receiveShadows = true;

                tiles.Add(node, render);
            }
        }
    }

    /// <summary>
    /// Busca una estructura y dice si existe o no.
    /// </summary>
    public bool ExistBuild (ESTRUCTURA buildType) {
        if(!builds.ContainsKey(buildType) || builds[buildType]==null)
            return false;

        return builds[buildType].Count>0;

        // Antigua manera de hacerlo
        /* for (int i = 0; i < builds.Count; i++) {
            if(builds[i].GetBuildType() == buildType)
                return true;
        }

        return false;*/
    }

    /// <summary>
    /// Busca la construcción más cercana.
    /// </summary>
    public Vector2 _GetNearBuild (Vector2 initialPos, ESTRUCTURA buildType) {
        if(!ExistBuild(buildType))
            return Vector3.zero;

        List<Estructura> builds = this.builds[buildType];
        Vector2 nearest = Vector3.zero;
        int foundCount = 0;
        for(int i = 0; i < builds.Count; i++) {
            if(builds[i].GetBuildType() == buildType && (foundCount==0 || Vector2.Distance (builds[i].transform.position, initialPos) < Vector2.Distance (nearest, initialPos))) {
                nearest = builds[i].transform.position;

                foundCount++;
            }
        }

        if(foundCount == 0)
            Debug.LogWarning("No ha encontrado ninguna construcción cercana");

        return nearest;
    }

    /// <summary>
    /// En caso de que la capacidad cambie, se ejecuta este método
    /// </summary>
    public void OnCapacityChange(params ResourceInfo[] recursos) {
        for (int i = 0; i < recursos.Length; i++) {
            resourceController.ModifyResource(recursos[i]);
        }

        //Actualiza el panel de construcción
        build.ShopUpdate();
        //Actualiza el panel de crafteo.
        craft.UpdateCraft();
    }

    /// <summary>
    /// Crea un saco con objetos proximo
    /// Este saco no sirve para almacenar de manera persistente los objetos
    /// Cuando se hace de noche este saco desaparece (Por lo que necesita ser almacenado
    /// en un baul antes de que eso ocurra).
    /// </summary>
    public void  CrearSaco (IntVector2 pos, int maxSteps, ResourceInfo[] inventario) {
        int count = 0; 

        for (int i = 0; i < inventario.Length; i++) {
            count += inventario[i].quantity;
        }

        if(count == 0)
            return;

        pos = path.PathFind(pos, maxSteps, new PathSetting(PATHTYPE.huecoLibre)).GetFinalPosition();

        Estructura estructura = CreateBuild(pos, sackPrefab);
        if(estructura != null) {
            Recurso _recurso = estructura.GetComponent<Recurso>();
            _recurso.CreateResource(inventario);
            _recurso.transform.localScale = Vector3.Lerp(new Vector3(0.25f, 0.25f, 1), Vector3.one, ((float) _recurso.actualQuantity) / 250);
        }
    }

    public Estructura CreateBuild (IntVector2 position, GameObject prefab) {
        if(position.x < 0 || position.y < 0 || position.x >= totalSize.x || position.y >= totalSize.y)
            return null;

        GameObject _build = Instantiate(prefab);
        _build.transform.position = (Vector3) position;
        
        Estructura _buildScript = _build.GetComponent<Estructura>();
        _buildScript.manager = this;
        
        AddBuildInMap(position.x, position.y, _buildScript);

        return _buildScript;
    }

    /// <summary>
    /// Crea una estructura en el mapa.
    /// </summary>
    public void AddBuildInMap (int x, int y, Estructura estructura) {
        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y)
            return;

        if(!map[x, y].CreateBuild(estructura)) {
            Debug.LogWarning("No se ha podido crear la estructura.");
            return;
        }

        //Busca si existe el tipo a crear
        //Si no existe, lo crea
        ESTRUCTURA _tipo = estructura.GetBuildType();
        if(builds.ContainsKey(_tipo)) {
            if(!builds[_tipo].Contains(estructura))
                builds[_tipo].Add(estructura);
        } else {
            builds.Add(_tipo, new List<Estructura>());
        }
        
    }

    public void AddBuildInMap(IntVector2 position, Estructura estructura) {
        AddBuildInMap(position.x, position.y, estructura);
    }

    public void RemoveBuildInMap(int x, int y) {
        Estructura build = map[x, y].GetBuild();

        if(build==null)
            return;

        ESTRUCTURA _tipo = build.GetBuildType();
        if(ExistBuild(_tipo) && builds[_tipo]!=null && builds[_tipo].Contains(build))
            builds[_tipo].Remove(build);
        
        map[x, y].RemoveBuild();

        Destroy(build.gameObject);
    }

    public void RemoveBuildInMap (Vector3 position) {
        RemoveBuildInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    ///Actualiza los sprites de todo el mapa.
    /*public void UpdateMap() {
        for(int x = 0; x < map.GetLength(0); x++) {
            for(int y = 0; y < map.GetLength(1); y++) {
                map[x, y].render.sprite = SelectTileset(map[x, y].bloqueado ? 1 : 0, new Vector2(x, y));
            }
        }
    }

    Sprite SelectTileset(int valor, Vector2 position) {
        int _direccion = tipoSprite(valor, Mathf.RoundToInt(position.x), Mathf.RoundToInt (position.y));

        return (valor==0) ? spriteTierra[_direccion] : spriteAgua;
    }

    int tipoSprite(int valor, int x, int y) {
        bool[] value = new bool[4];

        if(y<(map.GetLength(1)-1) && (map[x, y + 1].bloqueado ? 1 : 0) == valor)
            value[0] = true;
        if(x>0 && (map[x - 1, y].bloqueado ? 1 : 0) == valor)
            value[1] = true;
        if(x<(map.GetLength(0)-1) && (map[x + 1, y].bloqueado ? 1 : 0) == valor)
            value[2] = true;
        if(y>0 && (map[x, y - 1].bloqueado ? 1 : 0) == valor)
            value[3] = true;
        
        if(value[0] && value[1] && value[2] && value[3])
            return 4;
        else if(value[0] && value[1] && value[2] && !value[3])
            return 7;
        else if(value[0] && value[1] && !value[2] && value[3])
            return 5;
        else if(value[0] && value[1] && !value[2] && !value[3])
            return 8;
        else if(value[0] && !value[1] && value[2] && value[3])
            return 3;
        else if(value[0] && !value[1] && value[2] && !value[3])
            return 6;
        else if(value[0] && !value[1] && !value[2] && value[3])
            return 10;
        else if(value[0] && !value[1] && !value[2] && !value[3])
            return 11;
        else if(!value[0] && value[1] && value[2] && value[3])
            return 1;
        else if(!value[0] && value[1] && value[2] && !value[3])
            return 13;
        else if(!value[0] && value[1] && !value[2] && value[3])
            return 2;
        else if(!value[0] && value[1] && !value[2] && !value[3])
            return 14;
        else if(!value[0] && !value[1] && value[2] && value[3])
            return 0;
        else if(!value[0] && !value[1] && value[2] && !value[3])
            return 12;
        else if(!value[0] && !value[1] && !value[2] && value[3])
            return 9;
        else if(!value[0] && !value[1] && !value[2] && !value[3])
            return 15;

        return 4;
    }*/
    
    public void CambiarPestañaRecursos (int index) {
        for (int i = 0; i < panelesRecursos.Length; i++) {
            panelesRecursos[i].SetActive(index == i);
        }
    }

    public void SeleccionarHerramienta(int herramienta) {
        int antiguo = (int) herramientaSeleccionada;
        if (antiguo == herramienta) {
            return;
        }

        if (herramientaSeleccionada == HERRAMIENTA.Seleccionar) {
            info.EliminarSeleccion();
        }

        herramientaSeleccionada = (HERRAMIENTA) herramienta;
        imagenCentral.sprite = iconosHerramientas[herramienta];

        //3 es el de prioridad
        //Cuando cambias a Prioridad, las acciones se mostrará la prioridad, no el valor.
        //Vuelve a cambiar esos gráficos a cambiar de herramienta.
        if (herramienta == 3) {
            actions.actionsQueue.ChangeIconToPriority();
        } else if (antiguo == 3) {
            actions.actionsQueue.ChangeIconToOriginal();
        }

        barraprioridad.value = 2;
    }

    public int SetSortingLayer (float yPos) {
        return Mathf.RoundToInt(yPos*1000 * -1);
    }
}

[System.Serializable]
public class IconInfo {
    public TIPOACCION type;
    public Sprite sprite;
}