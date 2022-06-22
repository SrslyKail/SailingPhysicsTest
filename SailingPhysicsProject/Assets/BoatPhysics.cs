using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatTutorial
{
    public class BoatPhysics : MonoBehaviour
    {
        //Drags
        public GameObject underWaterObj;

        //Script for everything to do with the mesh; finding which par is above/below water, etc.
        private ModifyBoatMesh modifyBoatMesh;

        //DEBUG mesh
        private Mesh underwaterMesh;

        //Boat rigidbody
        private Rigidbody boatRB;

        //water density
        private float rhoWater = 1027f;

        void Start()
        {
            //get the boats rigidbody
            boatRB = gameObject.GetComponent<Rigidbody>();

            //initialize the boat mesh modification
            modifyBoatMesh = new ModifyBoatMesh(gameObject);

            //sort which meshes are above/below water
            underwaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        }

        void Update()
        {
            //generate the underwater mesh
            modifyBoatMesh.GenerateUnderwaterMesh();

            //DEBUG: display the underwater mesh.
            modifyBoatMesh.DisplayMesh(underwaterMesh, "UnderWater Mesh", modifyBoatMesh.underWaterTriangleData);
        }

        void FixedUpdate()
        {
            //add forces to the parts that are under the water
            if (modifyBoatMesh.underWaterTriangleData.Count > 0)
            {
                AddUnderWaterForces();
            }
        }

        //All the forces for the underwater parts
        void AddUnderWaterForces()
        {
            //get all the triangles
            List<TriangleData> underWaterTriangleData = modifyBoatMesh.underWaterTriangleData;

            for (int i=0; i < underWaterTriangleData.Count; i++)
            {
                //Current triangle
                TriangleData triangleData = underWaterTriangleData[i];

                //Calculate the buoyancy
                Vector3 buoyancyForce = BuoyancyForce(rhoWater, triangleData);

                //Add the force to the boat
                boatRB.AddForceAtPosition(buoyancyForce, triangleData.center);

                //debug stuff

                Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);

                //DEBUG: buoyancy view
                Debug.DrawRay(triangleData.center, buoyancyForce.normalized * -3f, Color.red);
            }
        }

        //The buoyancy force; this makes the boat float!
        private Vector3 BuoyancyForce(float rho, TriangleData triangleData)
        {
            //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

            // F_buoyancy = rho * g * V
            // rho - density of the mediaum you are in
            // g - gravity
            // V - volume of fluid directly above the curved surface 

            // V = z * S * n 
            // z - distance to surface
            // S - surface area
            // n - normal to the surface
            Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

            //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
            buoyancyForce.x = 0f;
            buoyancyForce.z = 0f;

            return buoyancyForce;
        }
    }
}
