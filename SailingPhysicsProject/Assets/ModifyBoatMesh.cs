using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoatTutorial
{
    //Generates the mesh thats under the water
    public class ModifyBoatMesh : MonoBehaviour
    {
        //the transformas we use to get the global position of a vertice
        private Transform boatTrans;
        //Coordinates of all the verticies in the boat
        Vector3[] boatVertices;
        //Positions in allVerticesArray - used to build triangles
        int[] boatTriangles;

        //This is so we only need to transform the vertices from local to global once
        public Vector3[] boatVerticesGlobal;
        //Find all the disnaces to water once; some triangles share vertices so we can reuse those calculations
        float[] allDistancesToWater;

        //The triangles that are under water
        public List<TriangleData> underWaterTriangleData = new List<TriangleData>();

        public ModifyBoatMesh(GameObject boatObj)
        {
            //Get the transform
            boatTrans = boatObj.transform;

            //initialize the arrays and lists
            boatVertices = boatObj.GetComponent<MeshFilter>().mesh.vertices;
            boatTriangles = boatObj.GetComponent<MeshFilter>().mesh.triangles;

            //Convert the vertices from local to global
            boatVerticesGlobal = new Vector3[boatVertices.Length];
            //Find all the disnaces to water once; some triangles share vertices so we can reuse those calculations
            allDistancesToWater = new float[boatVertices.Length];
        }

        //generate the underwater mesh
        public void GenerateUnderwaterMesh()
        {
            //Reset the data
            underWaterTriangleData.Clear();

            //Find all the disnaces to water once; some triangles share vertices so we can reuse those calculations
            for (int j = 0; j < boatVertices.Length; j++)
            {
                //Convert coords to global
                Vector3 globalPos = boatTrans.TransformPoint(boatVertices[j]);

                //save the global position so we only need to calculate it once
                boatVerticesGlobal[j] = globalPos;

                allDistancesToWater[j] = WaterController.current.DistanceToWater(globalPos, Time.time);
            }

            //add the traqingles that are below the water
            AddTriangles();
        }

        //Add all the triangles that are part of the underwater mesh
        private void AddTriangles()
        {
            List<VertexData> vertexData = new List<VertexData>();

            //add initial data that will be replaced
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());

            //Loop through the triangles (3 vertices = 1 triangle)
            int i = 0;
            while (i < boatTriangles.Length)
            {
                //loop through the vertices
                for (int x = 0; x < 3; x++)
                {
                    //save what we need
                    vertexData[x].distance = allDistancesToWater[boatTriangles[i]];

                    vertexData[x].index = x;

                    vertexData[x].globalVertexPos = boatVerticesGlobal[boatTriangles[i]];

                    i++;
                }

                //If all the vertices above water
                if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance > 0f)
                {
                    continue;
                }

                //Create the triangles below the water line

                //ALL vertices are underwater
                if (vertexData[0].distance < 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    Vector3 p1 = vertexData[0].globalVertexPos;
                    Vector3 p2 = vertexData[1].globalVertexPos;
                    Vector3 p3 = vertexData[2].globalVertexPos;

                    //Save the triangle
                    underWaterTriangleData.Add(new TriangleData(p1, p2, p3));
                }
                //1 or 2 vertices are below the water
                else
                {
                    //Sort the vertices
                    vertexData.Sort((x, y) => x.distance.CompareTo(y.distance));

                    vertexData.Reverse();

                    //One vertice is above the water, the rest is below
                    if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesOneAboveWater(vertexData);
                    }
                    //Two vertices are above the water, the other is below
                    else if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesTwoAboveWater(vertexData);
                    }
                }
            }
        }

        //Build the new triangles where one of the old vertices is above water
        private void AddTrianglesOneAboveWater(List<VertexData> vertexData)
        {
            //Letters are based off of https://www.gamedeveloper.com/programming/water-interaction-model-for-boats-in-video-games
            //H is always at position 0
            Vector3 H = vertexData[0].globalVertexPos;

            //Left of H is M
            //Right of H is L

            //Find the index of M
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }

            //We also need the heights to water
            float h_H = vertexData[0].distance;
            float h_M = 0f;
            float h_L = 0f;

            Vector3 M = Vector3.zero;
            Vector3 L = Vector3.zero;

            //This means M is at position 1 in the List
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].globalVertexPos;
                L = vertexData[2].globalVertexPos;

                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].globalVertexPos;
                L = vertexData[1].globalVertexPos;

                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }


            //Now we can calculate where we should cut the triangle to form 2 new triangles
            //because the resulting area will always form a square

            //Point I_M
            Vector3 MH = H - M;

            float t_M = -h_M / (h_H - h_M);

            Vector3 MI_M = t_M * MH;

            Vector3 I_M = MI_M + M;


            //Point I_L
            Vector3 LH = H - L;

            float t_L = -h_L / (h_H - h_L);

            Vector3 LI_L = t_L * LH;

            Vector3 I_L = LI_L + L;


            //Save the data, such as normal, area, etc      
            //2 triangles below the water  
            underWaterTriangleData.Add(new TriangleData(M, I_M, I_L));
            underWaterTriangleData.Add(new TriangleData(M, I_L, L));
        }

        //Build the new triangles where two of the old vertices are above the water
        private void AddTrianglesTwoAboveWater(List<VertexData> vertexData)
        {
            //H and M are above the water
            //H is after the vertice that's below water, which is L
            //So we know which one is L because it is last in the sorted list
            Vector3 L = vertexData[2].globalVertexPos;

            //Find the index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }


            //We also need the heights to water
            float h_L = vertexData[2].distance;
            float h_H = 0f;
            float h_M = 0f;

            Vector3 H = Vector3.zero;
            Vector3 M = Vector3.zero;

            //This means that H is at position 1 in the list
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].globalVertexPos;
                M = vertexData[0].globalVertexPos;

                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].globalVertexPos;
                M = vertexData[1].globalVertexPos;

                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }


            //Now we can find where to cut the triangle

            //Point J_M
            Vector3 LM = M - L;

            float t_M = -h_L / (h_M - h_L);

            Vector3 LJ_M = t_M * LM;

            Vector3 J_M = LJ_M + L;


            //Point J_H
            Vector3 LH = H - L;

            float t_H = -h_L / (h_H - h_L);

            Vector3 LJ_H = t_H * LH;

            Vector3 J_H = LJ_H + L;


            //Save the data, such as normal, area, etc
            //1 triangle below the water
            underWaterTriangleData.Add(new TriangleData(L, J_H, J_M));
        }

        //Help class to store triangle data so we can sort the distances
        private class VertexData
        {
            //The distance to water from this vertex
            public float distance;
            //An index so we can form clockwise triangles
            public int index;
            //The global Vector3 position of the vertex
            public Vector3 globalVertexPos;
        }

        //Display the underwater mesh
        public void DisplayMesh(Mesh mesh, string name, List<TriangleData> triangesData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            //Build the mesh
            for (int i = 0; i < triangesData.Count; i++)
            {
                //From global coordinates to local coordinates
                Vector3 p1 = boatTrans.InverseTransformPoint(triangesData[i].p1);
                Vector3 p2 = boatTrans.InverseTransformPoint(triangesData[i].p2);
                Vector3 p3 = boatTrans.InverseTransformPoint(triangesData[i].p3);

                vertices.Add(p1);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p2);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p3);
                triangles.Add(vertices.Count - 1);
            }

            //Remove the old mesh
            mesh.Clear();

            //Give it a name
            mesh.name = name;

            //Add the new vertices and triangles
            mesh.vertices = vertices.ToArray();

            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
        }
    }
}
