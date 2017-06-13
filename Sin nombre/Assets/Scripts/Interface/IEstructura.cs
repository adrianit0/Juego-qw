public interface IEstructura  {

    string OnText();
    string OnTextGroup(Estructura[] estructuras);

    void OnDestroyBuild();

}

public interface IEquipo {
    void OnCapacityChange(params ResourceInfo[] recursos);
}