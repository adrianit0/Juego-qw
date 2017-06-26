using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Artesania : MonoBehaviour {


    public GameObject botonPrefab;

    //Crea los botones y los almacena aquí.
    Dictionary<CrafteoBoton, Craft> listaBotones;

    //El panel que se activará/Desactivará
    public GameObject panel;

    //Panel donde irán los botones pregenerador.
    public GameObject panelInterior;

    //Los botones de la cola.
    public CrafteoQueueBoton[] botonesCola;

    Crafteable actualTable;

    //MOTOR (No creado aún )
    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
        listaBotones = new Dictionary<CrafteoBoton, Craft>();
    }

	void Start () {
        panel.SetActive(false);

        CrearBotones ( 
            //Crea 5 tablas a partir de 1 de madera.
            new Craft( CRAFTTYPE.Mesa,
                new ResourceInfo[] { new ResourceInfo(RECURSOS.Madera, 1) },
                new ResourceInfo(RECURSOS.Tabla, 5), 1, 1f),
            
            //Crea 3 bloques de piedras a partir de 1 de roca.
            new Craft (CRAFTTYPE.Mesa, 
                new ResourceInfo[] { new ResourceInfo (RECURSOS.Piedra, 1) },
                new ResourceInfo (RECURSOS.Ladrillo, 3), 10, 1f),

            //Este último es de coña y solo de pruebas.
            new Craft (CRAFTTYPE.Mesa,
                new ResourceInfo[] { new ResourceInfo (RECURSOS.Cobre, 99), new ResourceInfo (RECURSOS.Plata, 2)},
                new ResourceInfo (RECURSOS.Oro, 1), 99999, 5)

            //Seguir añadiendo.

            );
	}

    /// <summary>
    /// Crea todos los botones y los almacena en una variable Dictionary.
    /// </summary>
    /// <param name="crafteos"></param>
    void CrearBotones (params Craft[] crafteos) {
        for (int i = 0; i < botonesCola.Length; i++) {
            int x = i;
            botonesCola[i].boton.onClick.AddListener(() => {
                CancelarAccion(x);
            });
        }


        for (int i = 0; i < crafteos.Length; i++) {
            GameObject _boton = Instantiate(botonPrefab);
            CrafteoBoton _script = _boton.GetComponent<CrafteoBoton>();

            _boton.transform.SetParent(panelInterior.transform);
            _boton.transform.localScale = new Vector3(1, 1, 1);

            _script.Configurar(crafteos[i], manager);

            listaBotones.Add(_script, crafteos[i]);
            
            _script.botonAñadir.onClick.AddListener(() => {
                AñadirCrafteo(listaBotones[_script]);
            });
        }
    }

    public void OpenPanel () {
        panel.SetActive(true);
    }

    /// <summary>
    /// Muestra en pantalla la mesa actual.
    /// </summary>
    /// <param name="table"></param>
    public void SetCraftableTable(Crafteable table) {
        actualTable = table;
        //Añadir solo las recetas de crafteos disponible para esta mesa.
        //Por ejemplo, las recetas de cocina estaran ocultos para la estructura de mesa.
        foreach(CrafteoBoton boton in listaBotones.Keys) {
            bool activar = listaBotones[boton].tipoCraft == table.tipoCrafteador;
            boton.gameObject.SetActive(activar);

            for(int i = 0; i < botonesCola.Length; i++) {
                if(i < table.crafteos.Count) {
                    botonesCola[i].obtencion.imagen.enabled = true;
                    botonesCola[i].obtencion.imagen.sprite = manager.resourceController.GetSprite(table.crafteos[i].obtencion.type);
                    botonesCola[i].obtencion.cantidad.text = table.crafteos[i].obtencion.quantity.ToString();

                } else {
                    botonesCola[i].obtencion.imagen.enabled = false;
                    botonesCola[i].obtencion.cantidad.text = "";
                }
            }
        }

        UpdateCraft();
    }

    void AñadirCrafteo(Craft crafteo) {
        if(actualTable == null) {
            Debug.LogWarning("Artesania::AñadirCrafteo error: No hay ninguna mesa vinculado, por lo que no se puede cancelar.");
            return;
        }

        if (crafteo == null) {
            Debug.LogWarning("Artesania::AñadirCrafteo error: No hay crafteo para poder añadirlo.");
            return;
        }

        actualTable.AddCraft(crafteo);
    }

    void CancelarAccion (int id) {
        if (actualTable==null) {
            Debug.LogWarning("Artesania::CancelarAccion error: No hay ninguna mesa vinculado, por lo que no se puede cancelar.");
            return;
        }

        actualTable.CancelCraft(id);
    }

    //Actualiza la lista de cosas que puede construir a partir de tus recursos actuales
    //Por ejemplo, si no tienes piedra, no te dejará construir ladrillos.
    //TODO: Hacer uso de alguna lista local en lugar de hacerlo con todos los botones uses o no.
    public void UpdateCraft () {
        if(!panel.activeSelf)
            return;

        foreach (CrafteoBoton boton in listaBotones.Keys) {
            boton.SetInteractable(manager.inventario.ContainsResource(listaBotones[boton].requisitos));
        }
    }
}
