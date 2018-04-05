using UnityEngine;
using UnityEditor;

public class VCloudShaderGUI : MaterialEditor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Help"))
            Application.OpenURL("https://docs.google.com/document/d/1vUXzQ5Ww9NR7m7F5n7adUWXJnWUH0u47JHmcifKK48w/edit?usp=sharing");

        base.OnInspectorGUI();
        
        UpdateRenderQueue();

    }

    public void OnSceneGUI()
    {
        if(Event.current.alt)
            return;
        Material m = (Material)target;
        {
            Handles.color = Color.black;
            // Transform
            Vector4 transform = m.GetVector("_CloudTransform");
            // pos
            Vector3 newCloudPos = Handles.DoPositionHandle(new Vector3(-transform.z, transform.x, -transform.w), Quaternion.identity);
            // size
            float handleSize = HandleUtility.GetHandleSize(newCloudPos);
            float newSize = Handles.ScaleSlider(transform.y, newCloudPos, -Vector3.up, Quaternion.identity, handleSize * 1.5f, 0f);

            Vector4 remap = new Vector4(newCloudPos.y, newSize, -newCloudPos.x, -newCloudPos.z);
            if(remap != transform)
            {
                Undo.RecordObject (m, "Modified VCloud material");
                m.SetVector("_CloudTransform", remap);
            }
            
            // Wind
            Vector4 windDirection = m.GetVector("_WindDirection");
            Vector3 windReference = windDirection;
            Vector3 newWindDirection = windReference.normalized;
            newWindDirection = Handles.RotationHandle(Quaternion.LookRotation(windDirection.normalized), newCloudPos) * Vector3.forward;
            newWindDirection.y = 0f;
            if(windReference != newWindDirection)
            {
                Undo.RecordObject (m, "Modified VCloud material");
                m.SetVector("_WindDirection", new Vector4(newWindDirection.x, newWindDirection.y, newWindDirection.z, windDirection.w));
            }


            // non-interactable
            Handles.Label(newCloudPos, "Cloud Position");
            Vector3 windArrowPos = newCloudPos + Vector3.up*handleSize;
            Vector3 windArrowEnd = windArrowPos + newWindDirection*handleSize;
            #if UNITY_5_4_OR_NEWER
            Handles.ArrowHandleCap(125445, windArrowPos, Quaternion.LookRotation(newWindDirection), handleSize, EventType.Repaint);
            #else
            Handles.ArrowCap(125445, windArrowPos, Quaternion.LookRotation(newWindDirection), handleSize);
            #endif
            Handles.Label(windArrowEnd, "Wind Direction");
        }


        
        int sphere = m.GetInt("_SphereMapped");
        if(sphere > 0)
        {
            Vector4 spherePos = m.GetVector("_CloudSpherePosition");
            Vector3 spherePosV3 = spherePos;
            Vector3 newPos = Handles.DoPositionHandle(spherePos, Quaternion.identity);
            Handles.Label(newPos, "Cloud Position");
            if(spherePosV3 != newPos)
            {
                Undo.RecordObject (m, "Modified VCloud material");
                m.SetVector("_CloudSpherePosition", new Vector4(newPos.x, newPos.y, newPos.z, spherePos.w));
            }
        }
    }

    void UpdateRenderQueue()
    {
        serializedObject.Update();

        var shader = serializedObject.FindProperty("m_Shader");
        if (!isVisible || shader.objectReferenceValue == null)
            return;
        if (shader.hasMultipleDifferentValues)
            return;
        if (shader.objectReferenceValue == null)
            return;
        if (!(target is Material))
            return;

        Material m = (Material)target;
        m.renderQueue = m.GetInt("_RenderQueue");
    }
}

