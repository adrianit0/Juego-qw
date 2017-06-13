using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            //inventario.Add(new ResourceInfo(recurso, value));
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

    public int FreeSpace {
        get {
            return capacidadTotal - Count;
        }

        set { }
    }

    //VARIABLES
    public List<ResourceInfo> inventario = new List<ResourceInfo>();
    public int capacidadTotal = 0;

    public Fluido aguaTotal;
    public int litrosTotales = 6;

    public IEquipo equipo;

    //CONSTRUCTORES
    public Inventario(int capacidadTotal) {
        inventario = new List<ResourceInfo>();
        this.capacidadTotal = capacidadTotal;
    }

    //MÉTODOS
    /// <summary>
    /// Añade recursos al almacen, y devuelve la cantidad de sobra (La que no puede almacenar).
    /// </summary>
    public int AddResource(RECURSOS recurso, int cantidad, bool actualizar = true) {
        if(cantidad == 0)
            return 0;

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
            AddResource(recursos[i].type, recursos[i].quantity, false);
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
            AddResource(recursos[i].type, recursos[i].quantity, false);
        }

        if(actualizar) {
            OnValueChange(recursos);
        }
    }

    /// <summary>
    /// Coges recursos del inventario, devuelve la cantidad de recursos que no ha podido coger.
    /// </summary>
    public int GetResource(RECURSOS recurso, int cantidad, Inventario destinatario, bool actualizar = true) {
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
            OnValueChange(new ResourceInfo(recurso, cantidad));
        }

        return faltante;
    }
    
    public void GetResources (ResourceInfo[] info, Inventario destinatario) {
        Debug.Log("Sin programar");
        //Realizar
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
            if(info != null && info.quantityText != null) {
                info.quantityText.text = info.quantity.ToString();
            }
        }
        
        if (equipo!= null) {
            equipo.OnCapacityChange(recursos);
        }
    }

    //OTROS METODOS
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