using UnityEngine;

public class Hoguera : Estructura, IEstructura, IUpdatable {

    public Light luz;

    public float minLight = 1.5f, maxLight = 2f;

    float total = 0;
    public void OnStart() {
        manager.time.AddUpdatable(GetComponent<SpriteAnimation>().getUpdatable());
    }

    public void OnUpdate(float delta) {
        if(luz == null)
            return;

        total += delta;
        luz.intensity = Mathf.PingPong(total, maxLight - minLight) + minLight;
    }

    public void OnFixedUpdate(float delta) { }
    public void OnVelocityChange(float nueva) { }

    public string OnText() {
        //ALIMENTAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.VaciarAlmacen), "Alimentar fuego", false, () => { });
        //COCINAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Cocinar), "Cocinar alimento", false, () => { });
        //APAGAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Destruir), "Apagar fuego", false, () => { });


        return "Es una calurosa hoguera... Parece tardar hasta que se apague sola...";
    }

    public string OnTextGroup(Estructura[] estructuras) {
        //APAGAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Destruir), "Apagar fuego", false, () => { });

        return "Son hogueras...";
    }

    public void OnDestroyBuild() {
        //No pasa nada al destruirse...
    }
}
