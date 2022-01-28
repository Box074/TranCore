using System.Collections;
using UnityEngine;

namespace TranCore
{
    public class Inv : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (invt > 0)
            {
                invt -= Time.fixedDeltaTime;
            }
        }
        public void Hit(HitInstance hit,float invt)
        {
            if (this.invt > 0) return;
            this.invt = invt;
			FSMUtility.SendEventToGameObject(gameObject, "TOOK DAMAGE");
            HitTaker.Hit(gameObject, hit);
        }
        float invt = 0;
    }
}