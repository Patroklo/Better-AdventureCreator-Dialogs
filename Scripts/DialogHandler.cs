using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dialogs
{
    [ExecuteInEditMode]
    public class DialogHandler : MonoBehaviour
    {


        [HideInInspector]
        public string SelectedFile = "";

        [HideInInspector]
        public int SelectedDialogIndex = 0;

        [HideInInspector]
        public List<string> FileList;


        [HideInInspector]
        public bool LoadedDialogue = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}