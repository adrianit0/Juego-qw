using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdatable  {

    void OnUpdate(float delta);
    void OnFixedUpdate(float fixedDelta);
    void OnVelocityChange(float newVelocity);

}
