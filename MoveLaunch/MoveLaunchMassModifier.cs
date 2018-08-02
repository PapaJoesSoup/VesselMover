using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VesselMover
{
    public class MoveLaunchMassModifier : PartModule
    {
        public bool modify = true;
        private float defaultMass = 0;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
                defaultMass = this.part.mass;
                this.part.mass = 0;
            }
            base.OnStart(state);
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (modify)
                {
                    this.part.mass = 0;
                }
                else
                {
                    StartCoroutine(Drop());
                }
            }
        }

        IEnumerator Drop()
        {
            yield return new WaitForEndOfFrame();
            this.part.mass = defaultMass / 6;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            this.part.mass = defaultMass / 4;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            this.part.mass = defaultMass / 2;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            this.part.mass = defaultMass;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Destroy(this);
        }
    }
}
