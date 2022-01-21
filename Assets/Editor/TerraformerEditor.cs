using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Terraformer))]
public class TerraformerEditor : Editor
{
    SerializedProperty edit;
    SerializedProperty interacting;
    SerializedProperty direction;
    bool isTerraforming = false;
    private void OnEnable()
    {
        edit = serializedObject.FindProperty("edit");
        direction = serializedObject.FindProperty("direction");
    }

    private void OnSceneGUI()
    {
        if (edit.boolValue)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (Event.current.type == EventType.MouseDown)
            {
                isTerraforming = true;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                isTerraforming = false;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl)
            {
                direction.intValue = -1 * direction.intValue;
            }

            if(isTerraforming && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 v = hit.transform.parent.parent.InverseTransformPoint(hit.point);
                    Vector3Int vInt = new Vector3Int((int)v.x, (int)v.y, (int)v.z);

                    (target as Terraformer).IncreaseAsync(vInt);
                    serializedObject.Update();
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
        
        
    }
}
