using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DensityDescriptorDisplay : MonoBehaviour
{

    public BaseDensityDescriptor densityDescriptor;
    public int z = 0;
    public Texture2D t;
    public SpriteRenderer s;
    public bool check;

    private void OnEnable()
    {
        EditorApplication.update += UpdateEdit;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEdit;
    }

    void UpdateEdit()
    {
        if (check)
        {
            if (t.height != densityDescriptor.size.y || t.width != densityDescriptor.size.x)
            {
                t = new Texture2D(densityDescriptor.size.x, densityDescriptor.size.y);
            }
            s.sprite = null;
            if (densityDescriptor)
            {
                Color c = Color.black;
                for (int i = 0; i < densityDescriptor.size.x; i++)
                {
                    for (int j = 0; j < densityDescriptor.size.y; j++)
                    {
                        float val = densityDescriptor.GetDensity(new Vector3Int(i, j, z));
                        c.r = c.g = c.b = val;
                        t.SetPixel(i, j, c);
                    }

                }
                t.Apply();
                s.sprite = Sprite.Create(t, new Rect(0, 0, densityDescriptor.size.x, densityDescriptor.size.y), new Vector2(0.5f, 0.5f));
                s.sharedMaterial.SetTexture("Diffuse", t);

            }
            check = false;
        }
    }
}
