using UnityEngine;
using UnityEditor;

namespace Editor
{
    [CustomPropertyDrawer(typeof(Graph))]
    public class GraphDrawer : PropertyDrawer
    {
        private const float GRAPH_HEIGHT = 140;
        private const float POINT_THICKNESS = 1f;
        private const float GRAPH_LINE_STEP_COUNT = 4;
        private const float GRAPH_LEFT_INDENT = 30;
        
        private Color _backgroundColor = Color.gray;
        private Color _graphColor = Color.red;

        private int _secOnScreen = 100;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GRAPH_HEIGHT + 4 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight/2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var prefixRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixRect, GUIUtility.GetControlID(FocusType.Passive), label);
            
            var prefixSecOnScreenRect = new Rect(position.x, prefixRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixSecOnScreenRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Points at the same time"));
            var secOnScreenRect = new Rect(position.width * 0.3f + 5, prefixRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _secOnScreen = EditorGUI.IntSlider(secOnScreenRect, _secOnScreen, 1, 100);
            
            var prefixColorRect = new Rect(position.x, prefixSecOnScreenRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixColorRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Background Color"));
            var colorRect = new Rect(position.width * 0.3f + 5, prefixSecOnScreenRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _backgroundColor = EditorGUI.ColorField(colorRect, _backgroundColor);
            
            var prefixGraphColorRect = new Rect(position.x, prefixColorRect.yMax, position.width * 0.3f - 2.5f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(prefixGraphColorRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Graph Color"));
            var graphColorRect = new Rect(position.width * 0.3f + 5, prefixColorRect.yMax, position.width * 0.7f - 2.5f, EditorGUIUtility.singleLineHeight);
            _graphColor = EditorGUI.ColorField(graphColorRect, _graphColor);
            
            var graphRect = new Rect(position.x + GRAPH_LEFT_INDENT, prefixGraphColorRect.yMax + EditorGUIUtility.singleLineHeight/2f, position.width, GRAPH_HEIGHT);
            EditorGUI.DrawRect(graphRect, _backgroundColor);
            
            var maxRect = new Rect(position.x, prefixGraphColorRect.yMax, GRAPH_LEFT_INDENT, EditorGUIUtility.singleLineHeight);
            var minRect = new Rect(position.x, graphRect.yMax - EditorGUIUtility.singleLineHeight/2f, GRAPH_LEFT_INDENT, EditorGUIUtility.singleLineHeight);
            
            var byteArrayProperty  = property.FindPropertyRelative("data");

            if (byteArrayProperty != null)
            {
                var data = new byte[(_secOnScreen < byteArrayProperty.arraySize)? _secOnScreen : byteArrayProperty.arraySize];
            
                var max = byte.MinValue;
                var min = byte.MaxValue;
                
                if (byteArrayProperty.arraySize >= 2)
                {
                    //GET MIN MAX
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
                    
                    max = RoundToNearest(max, 20);
                    min = RoundToNearest(min, 20, false);
                    
                    EditorGUI.PrefixLabel(maxRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent($"{max:F1}"));
                    EditorGUI.PrefixLabel(minRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent($"{min:F1}"));
                
                    var secWidth = position.width / (_secOnScreen-1);
                
                    var yScale = (max == min)? -1 : GRAPH_HEIGHT/(max-min);
                
                    Handles.color = DarkenColor(_backgroundColor, 0.5f);
                
                    //DRAW GRAPH LINES
                    var graphLineYPosDelta = (graphRect.yMax-graphRect.yMin)/GRAPH_LINE_STEP_COUNT;
                    var graphLineValueDelta = (max-min)/GRAPH_LINE_STEP_COUNT;
                    for (var d = 1; d < GRAPH_LINE_STEP_COUNT; d++)
                    {
                        var yPos = graphRect.yMin + graphLineYPosDelta * d;
                        var value = max - graphLineValueDelta * d;
                        var lineRect = new Rect(position.x, yPos - EditorGUIUtility.singleLineHeight/2, GRAPH_LEFT_INDENT, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PrefixLabel(lineRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent($"{value:F1}"));
                        var leftPos = new Vector3(graphRect.xMin, yPos);
                        var rightPos = new Vector3(graphRect.xMax, yPos);
                        Handles.DrawLine(leftPos, rightPos);
                    }
                    
                    Handles.color = _graphColor;
                    
                    //DRAW POINTS
                    for (var i = 0; i != data.Length; i++)
                    {
                        var height = (yScale <= 0)? graphRect.height/2f : (data[i] - min) * yScale;
                        var pos = new Vector3(graphRect.x + i * secWidth, graphRect.yMax - height);
                        Handles.DrawSolidDisc(pos, Vector3.forward, POINT_THICKNESS);
                    }

                    
                    //DRAW LINES
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
        
        private static byte RoundToNearest(byte input, byte roundTo, bool roundUp = true)
        {
            if (roundUp)
                return (byte)((input + roundTo - 1) / roundTo * roundTo);
            return (byte)(input / roundTo * roundTo);
        }
        
        private static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        private static Color DarkenColor(Color color, float percentage)
        {
            var multiplier = 1f - percentage;
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
        }
    }
}