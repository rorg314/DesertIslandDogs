using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : IUseable {
    
    //Defines how the item is used - eg toy is used by triggering play animations
    public void dogUseItem(Dog dog, Item item) { 
    
    }
    //Defines how a generic item can use an item - eg for upgrade/craft
    public void itemUseItem(Item user, Item item) {


    }
    //Update item inventories to remove item etc
    public void updateItemInventory() {

    }



}

//Useable item interface
public interface IUseable {
    //Defines how an item/food is used/consumed by a dog
    void dogUseItem(Dog dog, Item item);
    //Generic definition of how another item may use an item, eg for upgrading/crafting
    void itemUseItem(Item user, Item item);
    void updateItemInventory();

}
