using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialKey : MonoBehaviour
{
    public float speed;
    public Sprite up;
    public Sprite down;

    private Image _image;
    
    // Start is called before the first frame update
    private void Start()
    {
        _image = GetComponent<Image>();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (true)
        {
            _image.sprite = up;
            yield return new WaitForSeconds(speed);
            _image.sprite = down;
            yield return new WaitForSeconds(speed);
        }
    }
}
