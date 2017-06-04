using UnityEngine;
using System.Collections;

public class SkyboxColor : MonoBehaviour
{
    public Material skybox;
    public float maxBrightness = 1f;

    public void RandomColorSwap()
    {
        Color rngColor = new Color(Random.Range(0, maxBrightness), Random.Range(0, maxBrightness), Random.Range(0, maxBrightness), 1f);
        GameObject.FindGameObjectWithTag("interface").GetComponent<Interface>().SendColorToArduino(rngColor);
        StartCoroutine(ColorFade(rngColor));
    }

    public void SetColor(Color32 c)
    {
        StartCoroutine(ColorFade(c));
    }

    IEnumerator ColorFade(Color color)
    {
        while(skybox.GetColor("_Tint") != color)
        {
            skybox.SetColor("_Tint", Vector4.MoveTowards(skybox.GetColor("_Tint"), color, Time.deltaTime * 1));
            yield return null;
        }
    }
}
