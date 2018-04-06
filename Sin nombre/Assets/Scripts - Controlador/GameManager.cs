﻿using System.Collections;
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
    private Dictionary<ESTRUCTURA, List<Estructura>> builds;    //Todas las estructuras
    
    //Lista de personajes jugables
    public List<Personaje> characters { get; private set; }
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
    
    public LightManager lightManager { get; private set; }
    public TimeManager time { get; private set; }
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
        
        time = GetComponent<TimeManager>();
        craft = GetComponent<Artesania>();
        info = GetComponent<Informacion>();
        characterController = GetComponent<CharacterInterfaceController>();

        path = new PathFinding(this);
        actions = new ActionManager(this);

        lightManager = FindObjectOfType<LightManager>();
        resourceController = FindObjectOfType<ResourceController>();
        interfaz = FindObjectOfType<UIManager>();

        //TODO: 
        //Poner esto en el Start cuando no haya estructuras pregeneradas.
        tiles = new Dictionary<Node, SpriteRenderer>();

        CrearMapa();

        inventario = new Inventario(int.MaxValue, this);
        inventario.SetInterface((IEquipo) this);
    }

    void Start() {
        //Añadimos los UPDATES de cada manager.
        time.AddUpdatable(lightManager.gameObject);
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

    public void CrearSaco (IntVector2 pos, ResourceInfo[] inventario) {
        //MAX STEP POR DEFECTO
        CrearSaco(pos, 15, inventario);
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

        IUpdatable[] _update = estructura.GetComponents<IUpdatable>();
        if(_update != null)
            time.AddUpdatable(_update);
        
    }

    public void AddBuildInMap(IntVector2 position, Estructura estructura) {
        AddBuildInMap(position.x, position.y, estructura);
    }

    /// <summary>
    /// Elimina una estructura, devolviendo un porcentaje de lo que costaba su creación
    /// El valor va del 0 al 1.
    /// </summary>
    public void RemoveBuildInMap(int x, int y, float devolver = 0) {
        Estructura _build = map[x, y].GetBuild();

        if(_build==null)
            return;

        ResourceInfo[] items = build.getObjetoConstrucción(_build.nombre);

        ESTRUCTURA _tipo = _build.GetBuildType();
        if(ExistBuild(_tipo) && builds[_tipo]!=null && builds[_tipo].Contains(_build))
            builds[_tipo].Remove(_build);

        IUpdatable[] update = _build.GetComponents<IUpdatable>();

        if (update!=null)
            time.RemoveUpdatable(update);
        
        map[x, y].RemoveBuild();

        if(devolver > 0 && items != null && items.Length > 0) {
            int total = 0;
            for(int i = 0; i < items.Length; i++) {
                items[i].quantity = Mathf.RoundToInt(items[i].quantity * devolver);
                total += items[i].quantity;
            }

            if(total > 0) {
                CrearSaco(new IntVector2(x, y), items);
            }
        }

        Destroy(_build.gameObject);
    }

    public void RemoveBuildInMap (Vector3 position, float devolver = 0) {
        RemoveBuildInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), devolver);
    }
    
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


    public Sprite GetIconSprite(TIPOACCION tipoAccion) {
        for(int i = 0; i < iconos.Length; i++) {
            if(iconos[i].type == tipoAccion)
                return iconos[i].sprite;
        }

        //Si no encuentra el icono devuelve el primero.
        return iconos[0].sprite;
    }
}

[System.Serializable]
public class IconInfo {
    public TIPOACCION type;
    public Sprite sprite;
}