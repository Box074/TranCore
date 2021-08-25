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
        void OnCollisionEnter2D(Collision2D collision)
        {
            OnCollisionStay2D(collision);
        }
        void OnCollisionStay2D(Collision2D collision)
        {
            hit.Source = gameObject;
            (collision.gameObject.GetComponent<Inv>() ?? collision.gameObject.AddComponent<Inv>()).Hit(
                hit, 1f / (sc == 0 ? 1f : sc));
        }
        public int sc = 1;
        public HitInstance hit;
    }
}