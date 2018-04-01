public interface IEstructura  {

    void OnStart();
    void OnUpdate(float delta);

    string OnText();
    string OnTextGroup(Estructura[] estructuras);

    void OnDestroyBuild();

}

public interface IEquipo {
    void OnCapacityChange(params ResourceInfo[] recursos);
}