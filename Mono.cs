using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;

namespace AutoSave
{
    public class Mono : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(Loop());
        }
        
        IEnumerator Loop()
        {
            yield return new WaitForSeconds(AutoSave.MinsToSave * 60); //seconds traversed between each saved
            if (AutoSave.SaveInBackground)
            {
                try { AutoSave.CallServerSave(); }
                catch (Exception ex) { }

            }
            else
            {
                try { AutoSave.CallServerSave(); }
                catch (Exception ex) { }
            }

            StartCoroutine(Loop()); //start loop again
        }
    }
}
