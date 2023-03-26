using System.Collections.Generic;
using UnityEngine;

public class SpriteDisplay : MonoBehaviour
{
    private GameObject _spriteTemplate;
    private GameObject SpriteTemplate
    {
        get
        {
            if(_spriteTemplate == null)
                _spriteTemplate = GetComponentInChildren<SpriteRenderer>().gameObject;
            return _spriteTemplate;
        }
    }

    private readonly List<Transform> _shown = new List<Transform>();

    private void Start()
    {
        SpriteTemplate.SetActive(false);
    }

    public void AddSprite(Sprite sprite)
    {
        GameObject disp = Instantiate(SpriteTemplate, transform);
        disp.SetActive(true);
        disp.GetComponent<SpriteRenderer>().sprite = sprite;
        _shown.Add(disp.transform);
        AdjustPositions();
    }

    private void AdjustPositions()
    {
        float start, delta;
        if(_shown.Count < 9)
        {
            start = 0.05f - 0.05f * _shown.Count;
            delta = 0.1f;
        }
        else
        {
            start = -0.4f;
            delta = 0.8f / (_shown.Count - 1);
        }

        for(int i = 0; i < _shown.Count; i++)
        {
            _shown[i].localPosition = new Vector3(start, .52f, 0f);
            start += delta;
        }
    }
}
