public interface IEstructura  {

    string OnText();
    void OnDestroyBuild();

}

public interface IEquipo {
    void OnCapacityChange(params ResourceInfo[] recursos);
}