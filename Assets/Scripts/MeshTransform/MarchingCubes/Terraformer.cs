using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Terraformer : MonoBehaviour
{
    public BaseDensityDescriptor densityDescriptor;
    public bool edit;
    public Vector3Int position;
    public StructureGenerator s;
    [Range(0.0001f,0.1f)]
    public float speed = 0.005f;

    public int direction = 1;
    public string SaveAs;
    public bool saveChanges;
    bool interacting;
    int count = 64;

    PersistenDensityDescriptor persistentDensity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateEditor;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
    }

    public void UpdateEditor()
    {
        if (interacting)
        {
            count--;
            if(count == 0)
            {
                interacting = false;
                count = 64;
            }
        }

        if (saveChanges)
        {
            SaveScriptableObject();
            saveChanges = false;
        }
    }
        
    public void SaveScriptableObject()
    {

        if(persistentDensity == null || persistentDensity.name != SaveAs)
        {
            persistentDensity = ScriptableObject.CreateInstance<PersistenDensityDescriptor>();
            persistentDensity.size = s.size;
            persistentDensity.Init();
            persistentDensity.size = s.size;

            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/PersistentDensity/"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "PersistentDensity");
            }

            AssetDatabase.CreateAsset(persistentDensity, "Assets/ScriptableObjects/PersistentDensity/" + SaveAs + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            EditorUtility.SetDirty(persistentDensity);
            Selection.activeObject = persistentDensity;
        }


        for (int i = 0; i < s.size.x; i++)
        {
            for (int j = 0; j < s.size.y; j++)
            {
                for (int k = 0; k < s.size.z; k++)
                {
                    persistentDensity.AddDensity(new Vector3Int(i, j, k), s.densities_matrix[i, j, k]);
                }
            }

        }


        EditorUtility.SetDirty(persistentDensity);

    }


    public async void IncreaseAsync(Vector3Int position)
    {
        if (!interacting && !s.IsGenerating())
        {
            interacting = true;
            List<Vector3Int> offsets = new List<Vector3Int>();
            for (int x = position.x - 16; x < position.x + 16; x++)
            {
                for (int y = position.y - 16; y < position.y + 16; y++)
                {
                    for (int z = position.z - 16; z < position.z + 16; z++)
                    {

                        if(x < s.size.x && x>=0 && y < s.size.y && y >= 0 && z < s.size.z && z >= 0)
                        {
                            float d = densityDescriptor.GetDensity(new Vector3Int(x, y, z), position);
                            if (d < 0.5)
                            {
                                s.densities_matrix[x, y, z] += direction * (1 - d) * speed;
                            }

                            Vector3Int offset = new Vector3Int(x / s.chunkSize, y / s.chunkSize, z / s.chunkSize);
                            if (!offsets.Contains(offset))
                            {
                                offsets.Add(offset);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < offsets.Count; i++)
            {
                ChunkTaskData ct = await s.GenerateSubChunks(offsets[i]*s.chunkSize);

                s.RegenerateChunk(ct, offsets[i] * s.chunkSize);
            }
        }
    }
}
