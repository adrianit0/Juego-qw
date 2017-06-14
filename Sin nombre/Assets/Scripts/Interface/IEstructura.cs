public interface IEstructura  {

    void OnStart();

    string OnText();
    string OnTextGroup(Estructura[] estructuras);

    void OnDestroyBuild();

}

public interface IEquipo {
    void OnCapacityChange(params ResourceInfo[] recursos);
}