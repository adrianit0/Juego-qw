using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RECURSOS {
    //RECURSOS
    Madera, Piedra, Cobre, Plata, Oro,
    //COMIDA
    Manzana, ManzanaDorada, Zanahoria, Sardina, Lubina
}
public enum TIPOACCION { Talar, Construir, Investigar, Cocinar, Minar, Cosechar, Almacenar, Pescar, Socializar, Arar, SacarAlmacen, VaciarAlmacen, RecogerObjeto, Destruir, ExtraerAgua, Regar, Plantar }
public enum HERRAMIENTA { Seleccionar = 0, Recolectar = 1, Arar = 2, Priorizar = 3, Destruir = 4, Cancelar = 5, Construir = 6, Custom = 7 }

public class GameManager : MonoBehaviour, IEquipo {

    public Vector2 totalSize = new Vector2(20, 20);

    //CONTENIDO PARTIDA
    public Inventario _inventario;
    public IconInfo[] iconos = new IconInfo[9];

    public List<Estructura> builds = new List<Estructura>();

    //PANEL HERRAMIENTAS
    public HERRAMIENTA herramientaSeleccionada = HERRAMIENTA.Seleccionar;
    public Sprite[] iconosHerramientas = new Sprite[4];
    public Image imagenCentral;
    
    //PANEL RECURSOS
    public GameObject[] panelesRecursos = new GameObject[2];

    //OTRAS COSAS
    public GameObject sacoObjetos;
    public GameObject agua;

    public GameObject nodoPrefab;
    public GameObject objetivoPrefab;

    GameObject objParent;
    Node[,] map;
    
    public List<Personaje> characters = new List<Personaje>();
    public List<Action> actions = new List<Action>();       //Lista por de acciones por adjudicar.
    public List<Action> actualActions = new List<Action>(); //Lista de acciones actualmente realizandose.
    
    public Sprite[] spriteTierra = new Sprite[16];
    public Sprite spriteAgua;

    PathFinding path;
    Agricultura farm;
    public Construccion build;  //Necesario en el personaje.
    public Informacion info;    //Necesario en las estructuras.
    
	void Awake () {
        path = new PathFinding(this);

        farm = GetComponent<Agricultura>();
        build = GetComponent<Construccion>();
        info = GetComponent<Informacion>();

        CrearMapa();
	}

    void Start () {
        InvokeRepeating("SearchAction", 0.25f, 0.25f);
        _inventario.equipo = (IEquipo) this;
    }

    /// <summary>
    /// Devuelve el camino más corto desde la posición del personaje hasta una posición
    /// </summary>
    public IntVector2[] PathFinding(Personaje character, Vector2 position) {
        return path.PathFind(character, new PathSetting(position)).path;
    }

    public PathResult PathFinding(Personaje character, PathSetting settings) {
        return path.PathFind(character, settings);
    }

    public PathResult PathFinding(IntVector2 position, int maxSteps, PathSetting settings) {
        return path.PathFind(position, maxSteps, settings);
    }

    public Node GetNode (int x, int y) {
        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y)
            return null;

        return map[x, y];
    }

    public Action CreateAction (int _x, int _y, HERRAMIENTA herramienta, CustomAction customAction = null) {
        if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
                return null;

        //Mira si esta acción ya está seleccionada.
        if (herramienta!=HERRAMIENTA.Cancelar && !(herramienta==HERRAMIENTA.Custom && customAction!=null && customAction.repeatable)) {
            for(int i = 0; i < actions.Count; i++) {
                if(actions[i] != null && actions[i].position == new Vector3(_x, _y, 0)) {
                    return null;
                }
            }

            for(int i = 0; i < actualActions.Count; i++) {
                if(actualActions[i].position == new Vector3(_x, _y, 0)) {
                    return null;
                }
            }
        }

        Vector2 _pos = new Vector2(_x, _y);
        
        //Buscamos que tipo de acción queremos hacer.
        TIPOACCION accion = TIPOACCION.Talar;

        //Valores personalizados, si el valor es diferente se tomará en lugar del valor genérico.
        GameObject customPrefab = null;
        float customTime = -1;
        Sprite customIcon = null;
        List<ResourceInfo> recNecesario = null;

        switch (herramienta) {
            case HERRAMIENTA.Seleccionar:
                if(map[_x, _y].GetBuild() == null)
                    return null;

                map[_x, _y].GetBuild().MostrarInformacion();

                return null;
            case HERRAMIENTA.Recolectar:
                if(map[_x, _y].GetBuild() == null)
                    return null;

                Recurso _resource = map[_x, _y].GetBuild().GetComponent<Recurso>();

                if(_resource != null) {
                    if(_resource.actualQuantity == 0)   //Si el recurso está vacio no te permite usarlo.
                        return null;
                    accion = _resource.actionType;
                } else if(map[_x, _y].GetBuildType() == ESTRUCTURA.Agua) {
                    accion = TIPOACCION.Pescar;
                } else {
                    return null;
                }
                break;

            case HERRAMIENTA.Arar:
                if(map[_x, _y].GetBuild() != null)
                    return null;

                accion = TIPOACCION.Arar;

                customPrefab = farm.huertoPrefab;
                customTime = farm.tiempoArar; 

                break;

            case HERRAMIENTA.Construir:
                if(map[_x, _y].GetBuild() != null)
                    return null;

                accion = TIPOACCION.Construir;

                customPrefab = build.construcciones[build.selectID].prefab;
                customTime = build.construcciones[build.selectID].tiempo;
                customIcon = build.construcciones[build.selectID].spriteModelo;

                recNecesario = new List<ResourceInfo>();
                for (int i = 0; i < build.construcciones[build.selectID].recursosNecesarios.Length; i++) {
                    recNecesario.Add(new ResourceInfo(build.construcciones[build.selectID].recursosNecesarios[i].recurso, build.construcciones[build.selectID].recursosNecesarios[i].cantidadNecesaria));
                }

                break;

            case HERRAMIENTA.Destruir:
                if(map[_x, _y].GetBuild() == null || !map[_x, _y].GetBuild().esDestruible)
                    return null;

                accion = TIPOACCION.Destruir;
                customTime = map[_x, _y].GetBuild().tiempoDestruccion;

                break;


            case HERRAMIENTA.Cancelar:
                for(int i = 0; i < actions.Count; i++) {
                    if(actions[i] != null && (Vector2) actions[i].position == _pos) {
                        RemoveAction(actions[i]);
                    }
                }

                for(int i = 0; i < actualActions.Count; i++) {
                    if(actualActions[i] != null && (Vector2) actualActions[i].position == _pos) {
                        RemoveAction(actualActions[i]);
                    }
                }
                return null;

            case HERRAMIENTA.Custom:
                //Herramienta especial. Para realizar cosas que con las anteriores no se pueden (Como extraer cosas del almacen).
                if(customAction == null)
                    return null;

                accion = customAction.tipo;

                switch (accion) {
                    case TIPOACCION.Almacenar:
                    case TIPOACCION.SacarAlmacen:
                        recNecesario = new List<ResourceInfo>(customAction.recNecesarios);
                        break;

                    case TIPOACCION.VaciarAlmacen:
                        if (map[_x, _y].GetBuild() != null && map[_x, _y].GetBuild().GetComponent<Almacen>().inventario.Count>0) {
                            customTime = 0.5f;
                            customIcon = SearchIcon(TIPOACCION.SacarAlmacen);
                        } else {
                            return null;
                        }
                        break;

                    case TIPOACCION.Plantar:
                        recNecesario = new List<ResourceInfo>(customAction.recNecesarios);
                        customTime = 0.50f;
                        customIcon = SearchIcon(TIPOACCION.Almacenar);
                        break;

                    case TIPOACCION.ExtraerAgua:
                        customTime = 0.5f;
                        customIcon = SearchIcon(TIPOACCION.SacarAlmacen);
                        break;

                    case TIPOACCION.Regar:
                        customTime = 0.25f;
                        customIcon = SearchIcon(TIPOACCION.Almacenar);
                        break;
                }

                break;

            default:
                Debug.LogWarning("Herramienta no programada aún");
                return null;
        }
        
        GameObject _obj = Instantiate(objetivoPrefab);
        SpriteRenderer actionRender = _obj.GetComponent<SpriteRenderer>();
        _obj.transform.position = _pos;
        
        actionRender.sprite = customIcon != null ? customIcon : SearchIcon(accion);

        Action actionScript = null;

        //Según el tipo acción que sea se crearé bajo un tipo de sobrecarga u otra.
        if (accion == TIPOACCION.Construir) {
            actionScript = new Action(customPrefab, build.selectID, new Vector3(_x, _y), customTime, actionRender, recNecesario);
        } else if (customPrefab == null) {
            actionScript = new Action(map[_x, _y].GetBuild(), accion, new Vector3(_x, _y), customTime > 0 ? customTime : map[_x, _y].GetBuild().tiempoTotal, actionRender, recNecesario);
        } else {
            actionScript = new Action(customPrefab, accion, new Vector3(_x, _y), customTime, actionRender, recNecesario);
        }

        return actionScript;
    }

    public Action CreateAction(Vector2 position, HERRAMIENTA herramienta, CustomAction customAction = null) {
        return CreateAction(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), herramienta, customAction);
    }

    public void AddAction(int _x, int _y, HERRAMIENTA herramienta, CustomAction customAction = null) {
        Action _action = CreateAction(_x, _y, herramienta, customAction);
        if(_action != null) {
            actions.Add(_action);
        }
    }

    public void AddAction (Vector2 position, HERRAMIENTA herramienta, CustomAction customAction = null) {
        AddAction(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), herramienta, customAction);
    }

    public void RemoveAction (Action action) {
        if (actions.Contains (action)) {
            if(action.renderIcon != null)
                Destroy(action.renderIcon.gameObject);

            actions.Remove(action);
        }
        
        if (actualActions.Contains (action) && action.worker != null) {
            action.worker.RemoveAction(action);
        }
    }

    Sprite SearchIcon (TIPOACCION tipoAccion) {
        for (int i=0; i < iconos.Length; i++) {
            if(iconos[i].type == tipoAccion)
                return iconos[i].sprite;
        }

        return null;
    }

    //Busca la acción idonea para realizar para cada personaje.
    void SearchAction () {
        if(actions.Count == 0)
            return;

        //Los personajes que no estuvieran realizando ninguna acción.
        List<Personaje> freeWorkers = new List<Personaje>();

        foreach (Personaje worker in characters) {
            if(worker.GetActionsCount() == 0)
                freeWorkers.Add(worker);
        }

        if(freeWorkers.Count == 0)
            return;
        
        for (int i = 0; i < actions.Count; i++) {
            //Calcularemos que personaje es el mejor para realizar cada acción libre, como actualmente no hay nada de eso programado, el personaje más cercano será quien se ocupe de la acción.
            Action action = actions[i];
            if (action==null) {
                actions.RemoveAt(i);
                i--;
                continue;
            }

            //Si no se puede realizar la acción tendrá que esperar 5s antes de volver a intentarlo.
            if (action.desactivado) {
                action.actualTime += 0.25f;
                if (action.actualTime > 5f) {
                    action.actualTime = 0;
                    action.desactivado = false;
                } else {
                    continue;
                }
            }

            int nearWorkers = 0;
            float nearPosition = -1;
            for(int x = 0; x < freeWorkers.Count; x++) {
                float ActualPositions = Vector3.Distance (freeWorkers[x].transform.position, action.position);

                if (nearPosition == -1 || ActualPositions<nearPosition) {
                    nearPosition = ActualPositions;
                    nearWorkers = x;
                }
            }

            PathResult resultado = PathFinding(freeWorkers[nearWorkers], new PathSetting(action.position));
            if (resultado.GetFinalPosition()==Vector3.zero) {
                action.renderIcon.color = Color.red;
                action.desactivado = true;
                return;
            }
            freeWorkers[nearWorkers].SetPositions(resultado.path);
            freeWorkers[nearWorkers].AddAction (action);
            
            actions.RemoveAt(i);
            freeWorkers.RemoveAt(nearWorkers);
            i--;

            if(freeWorkers.Count == 0)
                return;
        }
    }
	
    void CrearMapa () {
        map = new Node[(int) totalSize.x, (int) totalSize.y];
        objParent = new GameObject();
        objParent.transform.position = Vector3.zero;
        objParent.name = "Nodes";

        for (int x = 0; x < totalSize.x; x++) {
            for (int y = 0; y < totalSize.y; y++) {
                GameObject _obj = Instantiate (nodoPrefab);
                _obj.transform.position = new Vector3(x, y, 0);
                _obj.transform.parent = objParent.transform;
                map[x, y] = new Node(x, y, 100, this);
                
                _obj.GetComponent<SpriteRenderer>().sprite = spriteTierra[4];
            }
        }
    }

    /// <summary>
    /// Busca una estructura y dice si existe o no.
    /// </summary>
    public bool ExistBuild (ESTRUCTURA buildType) {
        for (int i = 0; i < builds.Count; i++) {
            if(builds[i].GetBuildType() == buildType)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Busca la construcción más cercana.
    /// </summary>
    public Vector2 _GetNearBuild (Vector2 initialPos, ESTRUCTURA buildType) {
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

    public void OnCapacityChange(params ResourceInfo[] recursos) {
        build.ShopUpdate();
    }

    public void  CrearSaco (IntVector2 pos, int maxSteps, ResourceInfo[] inventario) {
        pos = PathFinding(pos, maxSteps, new PathSetting(PATHTYPE.huecoLibre)).GetFinalPosition();

        Estructura estructura = CreateBuild(pos, sacoObjetos);
        if(estructura != null) {
            Recurso _recurso = estructura.GetComponent<Recurso>();
            _recurso.CreateResource(inventario);
            _recurso.transform.localScale = Vector3.Lerp(new Vector3(0.25f, 0.25f, 1), Vector3.one, ((float) _recurso.actualQuantity) / 100);
        }
    }

    public Estructura CreateBuild (IntVector2 position, GameObject prefab) {
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


        if (!builds.Contains (estructura))
            builds.Add(estructura);
    }

    public void AddBuildInMap(Vector3 position, Estructura estructura) {
        AddBuildInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), estructura);
    }

    public void RemoveBuildInMap(int x, int y) {
        Estructura build = map[x, y].GetBuild();

        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y|| build==null)
            return;

        if(!builds.Contains(build))
            builds.Remove(build);

        Destroy(build.gameObject);

        map[x, y].RemoveBuild();
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
        if (herramientaSeleccionada == HERRAMIENTA.Seleccionar) {
            info.EliminarSeleccion();
        }

        herramientaSeleccionada = (HERRAMIENTA) herramienta;
        imagenCentral.sprite = iconosHerramientas[herramienta];
    }

    public int SetSortingLayer (float yPos) {
        return Mathf.RoundToInt(yPos*1000 * -1);
    }
}

[System.Serializable]
public class ResourceInfo {
    public string name;

    public RECURSOS type;

    public int quantity;
    public Sprite sprite;

    public Text quantityText;

    public ResourceInfo (RECURSOS type, int initialQuantity) {
        name = type.ToString();
        this.type = type;

        quantity = initialQuantity;
    }
}

[System.Serializable]
public class IconInfo {
    public TIPOACCION type;
    public Sprite sprite;
    
}