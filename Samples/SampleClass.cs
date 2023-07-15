using Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Samples
{
    public class SampleClass : MonoBehaviour
    {
        [SerializeField]
        private Graph _graph;
        
        private void Start()
        {
            InvokeRepeating("CountUp", 1f, 1f);
        }

        private void CountUp()
        {
            var data = _graph.data;
            
            var newData = new int[data.Length +1];

            for (var i = 0; i != data.Length; i++)
            {
                newData[i] = data[i];
            }
            
            newData[data.Length] = Random.Range(20,100);
            
            _graph.data = newData;
        }
    }
}