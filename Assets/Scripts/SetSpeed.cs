using TMPro;
using UnityEngine;

public class SetSpeed : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Movement movement;
    
    private void Start()
    {
        text.text = "0 km/h";
    }

    private void Update()
    {
        text.text = $"{movement.GetSpeed()} km/h";
    }
}
