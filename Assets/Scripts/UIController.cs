using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    [SerializeField] private Text TextScore;

    public void SetScore(int newScore) {
        TextScore.text = "Score: " + newScore;
    }
}
