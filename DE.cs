using System.Collections;
using UnityEngine;

namespace TranCore
{
    public class DE : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
		void OnTriggerEnter2D(Collider2D collider) => G(collider.gameObject);
		void OnTriggerStay2D(Collider2D collider) => G(collider.gameObject);
        void OnCollisionEnter2D(Collision2D collision)
        {
            OnCollisionStay2D(collision);
        }
		void G(GameObject go)
		{
			hit.Source = gameObject;
            (go.GetComponent<Inv>() ?? go.AddComponent<Inv>()).Hit(
                hit, 1f / (sc == 0 ? 1f : sc));
		}
        void OnCollisionStay2D(Collision2D collision)
        {
            G(collision.gameObject);
        }
        public int sc = 1;
        public HitInstance hit;
    }
}