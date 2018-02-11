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
            StartCoroutine(WaitForServerStart());
        }
        
        IEnumerator WaitForServerStart()
        {
            yield return new WaitForSeconds(90);//Wait a while to make sure the server is started (Not necessary)
            StartCoroutine(Loop());
        }
        IEnumerator Loop()
        {
            yield return new WaitForSeconds(60); //seconds traversed between each saved

            AutoSave.ContarObjectos();//count objects in main Thread
            yield return new WaitForSeconds(10);//Make a wait to make sure that the object count does not match the saved as they are in the same Thread

            AutoSave.saveA = new System.ComponentModel.BackgroundWorker();
            AutoSave.saveA.DoWork += new System.ComponentModel.DoWorkEventHandler(AutoSave.GuardarMapaA);
            AutoSave.saveA.WorkerSupportsCancellation = true;
            AutoSave.saveA.RunWorkerAsync();

            StartCoroutine(Loop()); //start loop again
        }
    }
}
