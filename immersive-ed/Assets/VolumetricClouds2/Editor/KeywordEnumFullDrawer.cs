using UnityEditor;
using UnityEngine;

public class KeywordEnumFullDrawer : MaterialPropertyDrawer
{
    string[] options;

    public KeywordEnumFullDrawer(string options1, string options2)
    {
        this.options = new string[2];
        options[0] = options1;
        options[1] = options2;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3)
    {
        this.options = new string[3];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4)
    {
        this.options = new string[4];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4, string options5)
    {
        this.options = new string[5];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
        options[4] = options5;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4, string options5, string options6)
    {
        this.options = new string[6];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
        options[4] = options5;
        options[5] = options6;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4, string options5, string options6, string options7)
    {
        this.options = new string[7];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
        options[4] = options5;
        options[5] = options6;
        options[6] = options7;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4, string options5, string options6, string options7, string options8)
    {
        this.options = new string[8];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
        options[4] = options5;
        options[5] = options6;
        options[6] = options7;
        options[7] = options8;
    }

    public KeywordEnumFullDrawer(string options1, string options2, string options3, string options4, string options5, string options6, string options7, string options8, string options9)
    {
        this.options = new string[9];
        options[0] = options1;
        options[1] = options2;
        options[2] = options3;
        options[3] = options4;
        options[4] = options5;
        options[5] = options6;
        options[6] = options7;
        options[7] = options8;
        options[8] = options9;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (prop.type == MaterialProperty.PropType.Float)
        {
            if (options.Length < 2)
            {
                Debug.LogError("Missing some option strings for " + prop.displayName + " we need at least 2");
                return;
            }
            if (prop.floatValue > options.Length - 1)
                prop.floatValue = options.Length - 1;
            else if (prop.floatValue < 0)
                prop.floatValue = 0;
            prop.floatValue = EditorGUILayout.Popup(label, (int)prop.floatValue, options);

            foreach (string option in options)
            {
                foreach (var objectMaterial in prop.targets)
                {
                    Material mat = objectMaterial as Material;
                    mat.DisableKeyword(option);
                }
            }

            foreach (var objectMaterial in prop.targets)
            {
                Material mat = objectMaterial as Material;
                mat.EnableKeyword(options[(int)prop.floatValue]);
            }

        }
        else
        {
            Debug.LogError("Enums must be float/int, change property : " + label + " to int");
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0;
    }
}