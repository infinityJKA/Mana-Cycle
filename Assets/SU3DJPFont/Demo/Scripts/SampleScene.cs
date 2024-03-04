//
// Selected U3D Japanese fonts  sample scene's script
//
// the script encoding is UTF-8 with BOM
//

using UnityEngine;

namespace FutureCartographer.U3DJapaneseFont.Demo
{
	public class SampleScene : MonoBehaviour
	{
        public float RotateY = 10.0f;

		// camera rotation
		void Update()
		{
			this.transform.Rotate(0.0f, RotateY * Time.deltaTime, 0.0f);
		}
	}
}
