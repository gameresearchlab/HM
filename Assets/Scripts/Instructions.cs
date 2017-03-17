using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/* Name: Mathew Tomberlin
 * CST306
 */
public class Instructions : MonoBehaviour {
    public string text = "Aim the flashlight at the edges of the screen to rotate.";
    public Text UIText;
    public bool alternating = true;
    public float checkpoint = 0;
	// Use this for initialization
	void Start () {
        if (GetComponent<Text>() != null) {
            UIText = GetComponent<Text>();
            ChangeText(this.text);
            StartCoroutine(Alternate());
        }
	}

    public IEnumerator Alternate() {
        alternating = true;
        int index = 0;
        while (alternating) {
            float duration = 5.0f;
            while (duration > 0.0f)
            {
                duration -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (index < 5) {
                index++;
            } else {
                index = 0;
            }

            switch (index) {
                case 0:
                    ChangeText("Aim the flashlight at the edges of the screen to rotate.");
                    break;
                case 1:
                    ChangeText("Make a fist with your left hand. Move your fist like a joystick to move the character.");
                    break;
                case 2:
                    ChangeText("Move your hand near keys, batteries, doorknobs and curtain ropes to use them.");
                    break;
                case 3:
                    ChangeText("If your flashlight malfunctions, try hitting your hands together, or find a battery.");
                    break;
                case 4:
                    ChangeText("Press the space bar to pause the game.");
                    break;
                case 5:
                    ChangeText("If you see a ghost, try pointing your flashlight at it.");
                    break;
            }
        }
    }

    public void ChangeText(string newText) {
        this.text = newText;
        UIText.text = this.text;
    }
}
