using UnityEngine;
using UnityEditor;

namespace Editor
{
    [CustomPropertyDrawer(typeof(Graph))]
    public class GraphDrawer : PropertyDrawer
    {
        private const float GRAPH_HEIGHT = 130f;
        
        private Color _backgroundColor = Color.gray;
        private Color _graphColor = Color.red;

        private int _secOnScreen = 5;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GRAPH_HEIGHT + 4 * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var prefixRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixRect, GUIUtility.GetControlID(FocusType.Passive), label);
            
            var prefixSecOnScreenRect = new Rect(position.x, prefixRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixSecOnScreenRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Points at the same time"));
            var secOnScreenRect = new Rect(position.width * 0.3f + 5, prefixRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _secOnScreen = EditorGUI.IntSlider(secOnScreenRect, _secOnScreen, 1, 20);
            
            var prefixColorRect = new Rect(position.x, prefixSecOnScreenRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixColorRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Background Color"));
            var colorRect = new Rect(position.width * 0.3f + 5, prefixSecOnScreenRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _backgroundColor = EditorGUI.ColorField(colorRect, _backgroundColor);
            
            var prefixGraphColorRect = new Rect(position.x, prefixColorRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixGraphColorRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Graph Color"));
            var graphColorRect = new Rect(position.width * 0.3f + 5, prefixColorRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _graphColor = EditorGUI.ColorField(graphColorRect, _graphColor);
            
            var graphRect = new Rect(position.x, prefixGraphColorRect.yMax, position.width, GRAPH_HEIGHT);
            EditorGUI.DrawRect(graphRect, _backgroundColor);
            
            Handles.color = _graphColor;
            
            var byteArrayProperty  = property.FindPropertyRelative("data");

            if (byteArrayProperty != null)
            {
                var data = new byte[(_secOnScreen < byteArrayProperty.arraySize)? _secOnScreen : byteArrayProperty.arraySize];
            
                var max = byte.MinValue;
                var min = byte.MaxValue;
            

                if (byteArrayProperty.arraySize >= 2)
                {
                    for (var i = data.Length - 1; i >= 0; i--)
                    {
                        var index = byteArrayProperty.arraySize - data.Length + i;
                    
                        var value = byteArrayProperty.GetArrayElementAtIndex(index).intValue;
                        data[i] = (byte)value;
                    
                        if(data[i] > max)
                            max = data[i];
                        if(data[i] < min)
                            min = data[i];
                    }
                
                    var secWidth = position.width / (_secOnScreen-1);
                
                    var yScale = (max == min)? -1 : GRAPH_HEIGHT/(max-min);
                
                    for (var i = 0; i != data.Length; i++)
                    {
                        var height = (yScale <= 0)? graphRect.height/2f : (data[i] - min) * yScale;
                        var pos = new Vector3(graphRect.x + i * secWidth, graphRect.yMax - height);
                        Handles.DrawSolidDisc(pos, Vector3.forward, 3);
                    }

                    for (var i = 1; i != data.Length; i++)
                    {
                        var prevHeight = (yScale <= 0)? graphRect.height/2f : (data[i-1] - min) * yScale;
                        var prevPos = new Vector3(graphRect.x + (i-1) * secWidth, graphRect.yMax - prevHeight);
                    
                        var height = (yScale <= 0)? graphRect.height/2f : (data[i] - min) * yScale;
                        var pos = new Vector3(graphRect.x + i * secWidth, graphRect.yMax - height);
                        Handles.DrawLine(prevPos, pos);
                    }
                }
            }
            
            EditorGUI.EndProperty();
        }
    }
}