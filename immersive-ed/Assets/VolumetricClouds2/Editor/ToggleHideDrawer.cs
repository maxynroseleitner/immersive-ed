using UnityEditor;
using UnityEngine;

public class ToggleHideDrawer : MaterialPropertyDrawer
{
    string propertyDependance;
    string propertyToggler = string.Empty;
    bool isPropertyToggler
    {
        get { return propertyToggler != string.Empty; }
    }

    public ToggleHideDrawer(string togglePropertyName)
    {
        propertyDependance = togglePropertyName;
    }

    public ToggleHideDrawer(string togglePropertyName, string togglingProperty)
    {
        propertyDependance = togglePropertyName;
        propertyToggler = togglingProperty;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        Material mat = prop.targets[0] as Material;
        if (mat.GetFloat(propertyDependance) == 1)
        {
            position.y += 2;
            position.x += 10;
            
            Rect bgColor = position;
            bgColor.height += 2;
            bgColor.y += -2;
            bgColor.x += -5;
            EditorGUI.DrawRect(bgColor, new Color(0.0f, 0.0f, 0.0f, 0.15f));

            position.width += -10;
            position.height -= 5;
            if(isPropertyToggler)
                DrawToggle(position, prop, editor, label);
            else
                DrawDefaultField(position, prop, editor, label);
            //editor.DefaultShaderProperty(/*position,*/ prop, "  "+label);
            
        }
    }
    
    // Not in use atm
    void DrawDefaultField(Rect position, MaterialProperty prop, MaterialEditor editor, string label)
    {
        switch (prop.type)
        {
            case MaterialProperty.PropType.Float:
                prop.floatValue = editor.FloatProperty(position, prop, label);
                break;
            case MaterialProperty.PropType.Color:
                prop.colorValue = editor.ColorProperty(position, prop, label);
                break;
            case MaterialProperty.PropType.Range:
                prop.floatValue = EditorGUILayout.Slider(label, prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y);
                break;
            case MaterialProperty.PropType.Texture:
                prop.textureValue = editor.TextureProperty(position, prop, label);
                break;
            case MaterialProperty.PropType.Vector:
                prop.vectorValue = editor.VectorProperty(position, prop, label);
                break;
            default:
                throw new System.NotImplementedException(prop.type.ToString());
        }
    }

    void DrawToggle(Rect position, MaterialProperty prop, MaterialEditor editor, string label)
    {
        bool value = EditorGUI.Toggle(position, label, prop.floatValue > 0.5);
        prop.floatValue = value ? 1f : 0f;//editor.FloatProperty(position, prop, label);
        if(value)
            Shader.EnableKeyword(propertyToggler);
        else
            Shader.DisableKeyword(propertyToggler);
    }

    
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        Material mat = prop.targets[0] as Material;
        if (mat.GetFloat(propertyDependance) == 1)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropType.Float:
                    return 23;
                case MaterialProperty.PropType.Color:
                    return 23;
                case MaterialProperty.PropType.Range:
                    return 23;
                case MaterialProperty.PropType.Texture:
                    return 73;
                case MaterialProperty.PropType.Vector:
                    return 23;
            }
        }
        return 0;
    }

}