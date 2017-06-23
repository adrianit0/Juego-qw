using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceInfo {
    public RECURSOS type;
    public int quantity;

    public ResourceInfo(RECURSOS type, int initialQuantity) {
        this.type = type;

        quantity = initialQuantity;
    }
}

