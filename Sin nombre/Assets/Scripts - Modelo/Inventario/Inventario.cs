using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Que vuelva a mostrarse la cantidad de inventario en el menu superior derecho

[System.Serializable]
public class Inventario {

	public ResourceInfo this [RECURSOS recurso] {
        get {
            for (int i = 0; i < inventario.Count; i++) {
                if(inventario[i].type == recurso) {
                    return inventario[i];
                }
            }

            return null;
        }

        set {
            for(int i = 0; i < inventario.Count; i++) {
                if(inventario[i].type == recurso) {
                    inventario[i] = value;
                    return;
                }
            }

            inventario.Add(new ResourceInfo(recurso, value.quantity));
        }
    }

    public int this [int index] {
        get {
            return inventario[index].quantity;
        }

        set {
            inventario[index].quantity = value;
        }
    }

    public int Count {
        get {
            int cantidad = 0;
            for(int i = 0; i < inventario.Count; i++) {
                cantidad += inventario[i].quantity;
            }
            return cantidad;
        }

        set { }
    }

    public int Lenght {
        get {
            return inventario.Count;
        }

        set { }
    }

    public int MaxCapacity {
        get {
            return capacidadTotal;
        }

        set {
            Debug.Log("No se puede modificar la capacidad total de un inventario. Cree en su lugar un nuevo inventario.");
        }
    }

    public int FreeSpace {
        get {
            return capacidadTotal - Count;
        }

        set { }
    }

    //VARIABLES INVENTARIO
    List<ResourceInfo> inventario = new List<ResourceInfo>();
    int capacidadTotal = 0;

    //LIMITE DEL INVENTARIO, ACTUALMENTE UNICAMENTE SIRVE PARA LOS ALMACENES.
    public ResourceManagement limiteInventario { get; private set; }

    IEquipo equipo;
    GameManager manager;

    //CONSTRUCTORES
    public Inventario(int capacidadTotal, GameManager manager) {
        inventario = new List<ResourceInfo>();
        this.capacidadTotal = capacidadTotal;
        this.manager = manager;

        limiteInventario = new ResourceManagement(manager);
    }

    //MÉTODOS
    /// <summary>
    /// Añade recursos al almacen, y devuelve la cantidad de sobra (La que no puede almacenar).
    /// </summary>
    public int AddResource(RECURSOS recurso, int cantidad, bool actualizar = true) {
        if(cantidad <= 0)
            return cantidad;

        //Si está bloqueado el objeto no lo deja añadir.
        if(!limiteInventario.GetBool(recurso)) {
            return cantidad;
        }

        int sobrante = 0;
        int freeSpace = FreeSpace;
        if(cantidad > (freeSpace)) {
            sobrante = cantidad - freeSpace;
            cantidad = freeSpace;
        }

        if(cantidad == 0)
            return sobrante;
        
        bool encontrado = false;
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity += cantidad;
                encontrado = true;
                break;
            }
        }

        if(!encontrado) //Si no existe ese recurso en este inventario, lo crea.
            inventario.Add(new ResourceInfo(recurso, cantidad));

        if (actualizar) {
            OnValueChange(new ResourceInfo(recurso, cantidad));
        }
        
        return sobrante;
    }

    public void AddResource (ResourceInfo[] recursos, bool actualizar = true) {
        if(recursos == null)
            return;

        for(int i = 0; i < recursos.Length; i++) {
            int sobrante = AddResource(recursos[i].type, recursos[i].quantity, false);
            if (sobrante > 0) {
                Debug.LogWarning("Se han perdido " + sobrante + " del recurso " + recursos[i].type +". Arregarlo cuanto antes.");
            }
        }

        if(actualizar) {
            OnValueChange(recursos);
        }
    }

    /// <summary>
    /// Añades los recursos directamente desde una fuente de extracción de recursos.
    /// </summary>
    public void AddResource(Recurso recurso, bool actualizar = true) {
        if(recurso == null)
            return;

        ResourceInfo[] recursos = recurso.GetResource(FreeSpace);

        if(recursos == null)
            return;

        for(int i = 0; i < recursos.Length; i++) {
            int sobrante = AddResource(recursos[i].type, recursos[i].quantity, false);
            if(sobrante > 0) {
                Debug.LogWarning("Se han perdido " + sobrante + " del recurso " + recursos[i].type + ". Arregarlo cuanto antes.");
            }
        }

        if(actualizar) {
            OnValueChange(recursos);
        }
    }

    /// <summary>
    /// Devuelve la cantidad que se tenga de ese recurso en el almacen.
    /// </summary>
    public int GetResourceCount (RECURSOS recurso) {
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                return inventario[i].quantity;
            }
        }

        return 0;
    }

    /// <summary>
    /// Devuelve la cantidad de ese tipo que se tenga en el almacen.
    /// </summary>
    public int GetResourceTypeCount (TIPORECURSO tipo) {
        int cantidad = 0;
        for(int i = 0; i < manager.resourceController.panelRecurso.Length; i++) {
            if(manager.resourceController.panelRecurso[i].tipo == tipo) {
                cantidad += GetResourceCount(manager.resourceController.panelRecurso[i].resource);
            }
        }

        return cantidad;
    }

    /// <summary>
    /// Coges recursos del inventario, devuelve la cantidad de recursos que no ha podido coger.
    /// </summary>
    public int MoveResource(RECURSOS recurso, int cantidad, Inventario destinatario, bool actualizar = true) {
        if(cantidad == 0)
            return 0;

        int faltante = 0;

        int disponible = this[recurso].quantity;

        if(cantidad > disponible) {
            faltante = cantidad - disponible;
            cantidad = disponible;
        }

        int devuelto = destinatario.AddResource(recurso, cantidad);
        RemoveResource(recurso, cantidad-devuelto);

        if (actualizar) {
            OnValueChange(new ResourceInfo(recurso, -cantidad));
        }

        return faltante;
    }


    public bool ContainsResource (PathSetting settings) {
        foreach(ResourceInfo recurso in inventario) {
            if(settings.Value(recurso.type))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Te dice si tienes esos materiales.
    /// Con que te falte alguno de los materiales expuestos devolverá false.
    /// </summary>
    public bool ContainsResource (params ResourceInfo [] resourceList) {
        //Si la lista está vacía eso significa que si tienes todos los ingredientes a falta de ella.
        if (resourceList == null ||resourceList.Length==0) {
            return true;
        }
        for (int i = 0; i < resourceList.Length; i++) {
            if (GetResourceCount (resourceList[i].type) < resourceList[i].quantity) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Toma una cantidad del inventario.
    /// Devuelve la cantidad tomada, que puede ser inferior a la que se pide si quantity > cantidad de objetos en ese almacen.
    /// </summary>
    public int GetResource (RECURSOS type, int quantity, bool actualizar = true) {
        if(quantity == 0)
            return 0;

        ResourceInfo info = this[type];
        if(info == null)
            return 0;

        quantity = Mathf.Clamp(quantity, 0, info.quantity);
        info.quantity -= quantity;

        if(actualizar) {
            OnValueChange(new ResourceInfo(type, quantity));
        }

        return quantity;
    }
    
    /// <summary>
    /// Coge esa cantidad del inventario y lo devuelve.
    /// </summary>
    public ResourceInfo[] GetResources (ResourceInfo[] objetos, bool actualizar = true) {
        if(objetos == null || objetos.Length == 0) {
            Debug.LogWarning("Inventario::GetResources error: No hay objetos para tomar");
            return null;
        }
            

        ResourceInfo[] cantidad = new ResourceInfo[objetos.Length];

        for (int i = 0; i < objetos.Length; i++) {
            cantidad[i] = new ResourceInfo(objetos[i].type, GetResource(objetos[i].type, objetos[i].quantity, false));
        }

        if(actualizar) {
            OnValueChange(cantidad);
        }

        return cantidad;
    }

    /// <summary>
    /// Vacia el contenido especificado en el método y lo añade en el destinatario.
    /// La cantidad sobrante se devuelve a este inventario.
    /// </summary>
    public void GetResources(ResourceInfo[] objetos, Inventario destinatario, bool actualizar = true) {
        if(objetos == null || objetos.Length == 0) {
            Debug.LogWarning("Inventario::GetResources error: No hay objetos para tomar.");
            return;
        }

        if (destinatario == null) {
            Debug.LogWarning("Inventario::GetResources error: No hay destinatario.");
            return;
        }

        ResourceInfo[] info = GetResources(objetos, false);
        if(info != null || info.Length != 0) {
            for(int i = 0; i < info.Length; i++) {
                int sobrante = destinatario.AddResource(info[i].type, info[i].quantity, false);

                if(sobrante > 0) {
                    AddResource(info[i].type, sobrante, false);
                }
            }
        }

        if(actualizar) {
            destinatario.OnValueChange(info);
            for (int i = 0; i < info.Length; i++) {
                info[i].quantity *= -1;
            }

            OnValueChange(info);
        }
    }

    /// <summary>
    /// Elimina recursos del inventario
    /// </summary>
    public void RemoveResource(RECURSOS recurso, int cantidad, bool actualizar = true) {
        if(cantidad == 0)
            return;

        bool encontrado = false;
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity -= cantidad;
                encontrado = true;
            }
        }

        if(!encontrado)
            return;

        if(actualizar) {
            OnValueChange(new ResourceInfo(recurso, -cantidad));
        }
    }

    /// <summary>
    /// Mete una copia de otro inventario en este.
    /// </summary>
    /// <param name="otroInventario"></param>
    public void CopyContent (Inventario otroInventario) {
        for (int i = 0; i < otroInventario.Lenght; i++) {
            AddResource(otroInventario.inventario[i].type, otroInventario.inventario[i].quantity);
        }
    }

    /// <summary>
    /// Vacia todo el contenido de un inventario y lo almacena en otro.
    /// </summary>
    public void CleanResource(Inventario otroInventario, bool actualizar = true) {
        List<ResourceInfo> lista = new List<ResourceInfo>();
        for(int i = 0; i < inventario.Count; i++) {
            int sobrante = otroInventario.AddResource(inventario[i].type, inventario[i].quantity, false);
            lista.Add(new ResourceInfo(inventario[i].type, - inventario[i].quantity + sobrante));
            inventario[i].quantity = sobrante;
        }

        if(actualizar) {
            OnValueChange(lista.ToArray());

            for (int i = 0; i < lista.Count; i++) 
                lista[i].quantity *= -1;

            otroInventario.OnValueChange(lista.ToArray());
        }
    }

    /// <summary>
    /// Elimina todo el inventario.
    /// </summary>
    public void CleanResource(bool actualizar = true) {
        List<ResourceInfo> lista = new List<ResourceInfo>();
        for(int i = 0; i < inventario.Count; i++) {
            lista.Add(new ResourceInfo(inventario[i].type, -inventario[i].quantity));
            inventario[i].quantity = 0;
        }

        if(equipo != null) {
            equipo.OnCapacityChange(lista.ToArray());
        }
    }

    public void OnValueChange (params ResourceInfo[] recursos) {
        for (int i = 0; i < recursos.Length; i++) {
            ResourceInfo info = this[recursos[i].type];
            /*if(info != null && info.quantityText != null) {
                info.quantityText.text = info.quantity.ToString();
            }*/
        }
        
        if (equipo!= null) {
            equipo.OnCapacityChange(recursos);
        }
    }

    public void SetInterface (IEquipo value) {
        equipo = value;
    }

    //OTROS METODOS
    public RECURSOS GetResourceType (int index) {
        return inventario[index].type;
    }

    public bool IsFull () {
        return Count == capacidadTotal;
    }

    public float GetPerc () {
        return ((float) Count) / ((float) capacidadTotal);
    }

    public ResourceInfo[] ToArray () {
        return inventario.ToArray();
    }
}