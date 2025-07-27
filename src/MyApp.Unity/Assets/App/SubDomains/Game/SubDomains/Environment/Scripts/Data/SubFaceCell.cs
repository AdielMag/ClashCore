using UnityEngine;

namespace App.SubDomains.Game.SubDomains.Environment.Scripts.Data
{
    public class SubFaceCell
    {
        public Vector3[] vertices;
        public int faceIndex;
        public int subFaceIndex;

        public SubFaceCell(Vector3[] vertices, int faceIndex, int subFaceIndex)
        {
            this.vertices = vertices;
            this.faceIndex = faceIndex;
            this.subFaceIndex = subFaceIndex;
        }
    }
}