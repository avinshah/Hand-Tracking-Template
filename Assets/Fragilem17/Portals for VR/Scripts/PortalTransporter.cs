using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fragilem17.MirrorsAndPortals
{
    /**
     * Used to transport PortalableObjects to the linked portal defined in the portalSurface
     */
    public class PortalTransporter : MonoBehaviour
    {
        private Portal _portal;
        public List<PortalableObject> portalableObjects = new List<PortalableObject>();
        //private List<CloneRenderer> cloneObjects = new List<CloneRenderer>();
        public HashSet<CloneRenderer> cloneObjects = new HashSet<CloneRenderer>();
        public HashSet<CloneRenderer> cloneObjectsActuallyTouching = new HashSet<CloneRenderer>();
        public HashSet<CloneRenderer> cloneObjectsToRemove = new HashSet<CloneRenderer>();
        private List<PortalAffectorSphere> portalAffectorSpheres = new List<PortalAffectorSphere>();
        private Vector3 _originalScale;

        [Space(10)]
        [Header("Events")]
        public UnityEvent<PortalableObject> OnObjectEnteredPortal;
        public UnityEvent<PortalableObject> OnObjectTransportedAwayFromHere;
        public UnityEvent<PortalableObject> OnObjectTransportedToHere;
        public UnityEvent<PortalableObject> OnObjectExitedPortal;

        [HideInInspector]
        public Collider MyCollider;

        public enum PortalReason {
            Undefined,
            ExitedCurrentPortalCollider,
            EnteredOtherPortalCollider,
            FailedInsideColliderTest,
        }

        public Portal Portal { get => _portal; }

        public void Initialise()
        {
            MyCollider = GetComponent<Collider>();
            if (!MyCollider)
            {
                Debug.LogWarning(PortalUtils.Colorize("[PORTALS] ", PortalUtils.DebugColors.Warn, true) + name + " has a PortalTransporter Component but no trigger Collider, add a big enough Collider for things to hit it before reaching the actual portal.");
            }
            else
            {
                MyCollider.isTrigger = true;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (!rb)
            {
                Debug.LogWarning(PortalUtils.Colorize("[PORTALS] ", PortalUtils.DebugColors.Warn, true) + name + " has a PortalTransporter Component but no RigidBody Component. Add a (Kinematic) Rigidbody Component to this GameObject.");
            }

            _portal = GetComponentInParent<Portal>();
            if (!_portal)
            {
                Debug.LogWarning(PortalUtils.Colorize("[PORTALS] ", PortalUtils.DebugColors.Warn, true) + name + " has a PortalTransporter Component but no Portal Component, add a Portal Component to this or a parent GameObject.");
            }
        }

        protected void OnEnable()
        {
            Initialise();       
        }


        private void FixedUpdate()
        {
            //CheckNeedPortal();
        }


        private void LateUpdate()
        {
            //CheckNeedPortal();
        }

        protected void Update()
        {
            CheckNeedPortal();
        }

        private List<PortalableObject> currentPortalableObjects = new List<PortalableObject>();

        protected void CheckNeedPortal()
        {
            PortalableObject po = null;

            // don't modify list while looping:
            currentPortalableObjects.Clear();
            currentPortalableObjects.AddRange(portalableObjects);

            for (int i = 0; i < currentPortalableObjects.Count; ++i)
            {
                po = currentPortalableObjects[i];
                if (po)
                {
					if (po.IsMasterPortalableObject && !IsInsideCollider(po.transform.position))
					{
                        ExternalTriggerExit(po.gameObject, false, PortalReason.FailedInsideColliderTest);
                        break;
					}


                    Vector3 objPos = _portal.PortalSurface.transform.InverseTransformPoint(po.transform.position);

                    if (objPos.z > 0)
                    {
                        if (po.CanPortal())
                        {
                            //Debug.Log("Portal! "_portal.name + " : " + p.name + " : " + objPos.z);
                            //Debug.Log("PortalTransporter: PortalFrom: " + Portal.name + " : obj " + po.name + " : " + objPos.z);
                                                       

                            if (_portal.OtherPortal && _portal.OtherPortal.PortalTransporter)
                            {
                                // if the other portal is running the check later then this one.. adding it directly might cause it to portal back! delay this is a frame!
                                // CanPortal now has a frameAge that needs to be higher then 0 to avoid the above comment.
                                _portal.OtherPortal.PortalTransporter.ExternalTriggerEnter(po.gameObject, false);
                                _portal.OtherPortal.PortalTransporter.OnObjectTransportedToHere.Invoke(po);
                            }
                            po.PortalFrom(_portal);
                            OnObjectTransportedAwayFromHere.Invoke(po);
                        }

                    }
                }
            }
        }

        private bool IsInsideCollider(Vector3 position)
        {
            Vector3 closestPoint = MyCollider.ClosestPoint(position);
            return Vector3.Distance(closestPoint, position) < 0.1f;
        }

        public void ExternalTriggerEnter(GameObject other, bool fromPhysics = false)
        {
            // first trigger the exit in the other portal
            if (!fromPhysics && _portal.OtherPortal && _portal.OtherPortal.PortalTransporter)
            {
                _portal.OtherPortal.PortalTransporter.ExternalTriggerExit(other, fromPhysics, PortalReason.EnteredOtherPortalCollider);
            }

            Vector3 objPos = _portal.PortalSurface.transform.InverseTransformPoint(other.transform.position);
            // did we enter in front of the portal?
            //Debug.Log("ExternalTriggerEnter objPos: " + other.name + " " + objPos.z + " " + fromPhysics + " :" + Portal.name);
            if (!fromPhysics || (fromPhysics && objPos.z < 0))
            {
                PortalableObject obj = other.GetComponent<PortalableObject>();
                if (obj && obj.isActiveAndEnabled && !portalableObjects.Contains(obj))
                {
                    //Debug.Log("PortalTransporter:   1 Adding to portal list of " + _portal.name + " object:" + other.name + " physics: " + fromPhysics);
                    portalableObjects.Add(obj);
                    //obj.OnExitPortalCollider.AddListener(onPortalableObjectExitPortalCollider);

                    obj.SetIsInPortal(_portal);
                    OnObjectEnteredPortal.Invoke(obj);
                }
                else
                {
                    PortalableObjectCollider helper = other.GetComponent<PortalableObjectCollider>();
                    if (helper && helper.isActiveAndEnabled && !portalableObjects.Contains(helper.PortalableObject))
                    {
                        //Debug.Log("PortalTransporter:   2 Adding to portal list of " + _portal.name + " object:" + other.name + " physics: " + fromPhysics);
                        portalableObjects.Add(helper.PortalableObject);
                        //obj.OnExitPortalCollider.AddListener(onPortalableObjectExitPortalCollider);

                        helper.PortalableObject.SetIsInPortal(_portal);
                        OnObjectEnteredPortal.Invoke(helper.PortalableObject);
                    }
                }


                CloneRenderer cloneObject = other.GetComponent<CloneRenderer>();
                if (cloneObject && cloneObject.isActiveAndEnabled && !cloneObjects.Contains(cloneObject))
                {
                    cloneObjects.Add(cloneObject);
                    cloneObject.SetIsInPortal(_portal, true);
                }

                PortalAffectorSphere affectorSphere = other.GetComponent<PortalAffectorSphere>();
                if (affectorSphere && affectorSphere.isActiveAndEnabled && !portalAffectorSpheres.Contains(affectorSphere))
                {
                    //if (fromPhysics)
                    //{
                    portalAffectorSpheres.Add(affectorSphere);
                    //}
                    affectorSphere.SetIsInPortal(_portal);
                }
            }
        }

        /*private void onPortalableObjectExitPortalCollider(PortalableObject obj, Portal p)
		{
            if (p == Portal)
            {
                ExternalTriggerExit(obj.gameObject, false);
            }
        }*/

        public void ExternalTriggerExit(GameObject other, bool fromPhysics = false, PortalReason reason = PortalReason.Undefined)
        {
            PortalableObject portalableObj = other.GetComponent<PortalableObject>();

            /*
            bool isPortallingAlongWithMasterPortalable = false;
            if (portalableObj) {
                //portalableObj.OnExitPortalCollider.RemoveListener(onPortalableObjectExitPortalCollider);
                isPortallingAlongWithMasterPortalable = portalableObj.EnablePortalAlongWithMasterPortalable;
            }
            
            // todo: as long as we're grabbing something, and it's gonna portal along with the player, then don't exit the portal when it's collider exits this transporter.
            // but, what to do when the player does not portal?
			if (reason == PortalReason.ExitedCurrentPortalCollider && isPortallingAlongWithMasterPortalable)
			{
                // are we on the other side of the surface when exiting the collider? then keep it inside the portal
                Vector3 objPos = _portal.PortalSurface.transform.InverseTransformPoint(other.transform.position);
                Debug.Log("objPos: " + objPos.z);
				if (objPos.z > 0)
				{
                    return;
				}
			}*/
            

            CloneRenderer cloneObject = other.GetComponent<CloneRenderer>();
            /*if (cloneObject)
            {
                Debug.Log(cloneObject.name + " exits portal " + Portal.name + " IF " + cloneObjects.Contains(cloneObject) + " AND NOT isPortallingAlongWithMasterPortalable: " + isPortallingAlongWithMasterPortalable);
            }*/

            if (cloneObject && cloneObjects.Contains(cloneObject))
            {
                cloneObjects.Remove(cloneObject);
                cloneObject.ExitPortal(_portal, fromPhysics);
            }

            if (portalableObj && portalableObjects.Contains(portalableObj))
            {
                //Debug.Log("Exit Portal " + gameObject.name);
                //Debug.Log("Removing from portal list of " + _portal.name + " object:" + other.name);

                //Debug.Log("PortalTransporter: removing from portal list of " + _portal.name + " object:" + portalableObj.gameObject.name + " physics: " + fromPhysics + " reason: "+ reason);
                portalableObjects.Remove(portalableObj);
                portalableObjects.Remove(portalableObj);
                portalableObj.ExitPortal(_portal);
                OnObjectExitedPortal.Invoke(portalableObj);
            }

            PortalAffectorSphere affectorSphere = other.GetComponent<PortalAffectorSphere>();
            if (affectorSphere && portalAffectorSpheres.Contains(affectorSphere))
            {
                portalAffectorSpheres.Remove(affectorSphere);
                affectorSphere.ExitPortal(_portal);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            //Debug.Log("OnTriggerEnter: " + other.name);
            ExternalTriggerEnter(other.gameObject, true);
        }

		private void OnTriggerStay(Collider other)
        {
            ExternalTriggerEnter(other.gameObject, true);
        }

		protected void OnTriggerExit(Collider other)
        {
            ExternalTriggerExit(other.gameObject, true, PortalReason.ExitedCurrentPortalCollider);
        }
    }
}