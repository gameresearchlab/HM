using UnityEngine;
using System.Collections;

public class KeyInventory : MonoBehaviour {
    //This is a comma-seperated list that will be split. Each number in the list 
    //represents the door that the key possessed goes to
    public string keyInventory;
    //The transform where keys are parented
    public Transform keyPoint;

    //Split the keyInventory string in to an array of door number integers
    //Check each door number to see if it is the indicated key and if so return true
    public bool HasKey(int doorNumber) {
        if (!keyInventory.Equals("")) {
            string[] keys = keyInventory.Split(',');
            for (int i = 0; i < keys.Length; i++) {
                if (keys[i] != "" && int.Parse(keys[i]) == doorNumber) {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetKeyCount() {
        if (keyInventory != "") {
            string[] keys = keyInventory.Split(',');
            return keys.Length;
        } else {
            return 0;
        }
    }

    //If the user does not already have the key, add it to the key inventory string
    public void PutKey(int doorNumber) {
        if (!HasKey(doorNumber)) {
            //If this isn't the first item in the keyInventory string, put a comma first
            if (!keyInventory.Equals("")) {
                keyInventory += ",";
            }

            keyInventory += doorNumber.ToString();
        }
    }

    //If the user has the key, split the keyInventory string in to an array of door number integers
    //For each key, if we haven't already found the doorNumber key, add a comma if this isn't the 
    //first key in the list and then add the doorNumber. Set the keyInventory string equal to the temporary
    //string
    public void RemoveKey(int doorNumber) {
        if (HasKey(doorNumber)) {
            string[] keys = keyInventory.Split(',');

            string keyString = "";
            bool found = false;
            for (int i = 0; i < keys.Length; i++) {
                if (found || int.Parse(keys[i]) != doorNumber) {
                    keyString += ((i > 0) ? "," : "") + keys[i]; //ternary statement: if i > 0 then keyString += "," else keyString += ""
                                                                 //If this is the first time we've found the doorNumber, set found to true and do not add it to the keyString
                } else if (int.Parse(keys[i]) == doorNumber && !found) {
                    found = true;
                }
            }
            keyInventory = keyString;
        }
    }
}
