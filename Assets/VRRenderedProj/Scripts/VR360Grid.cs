using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VR360Grid : MonoBehaviour
{
    public float OffsetFromSphere = 0f;

    [SerializeField]
    public List<Vector3> positions = new List<Vector3>();

    public int width = 4;
    public int height = 3;

    public bool ProjectOnSphere = true;

    public void CreateGrid()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int w = width + 1;
        int h = height + 1;

        Vector3[] vertices = new Vector3[(w) * (h)];
        int[] tri = new int[(w - 1) * (h - 1) * 6];
        Vector3[] normals = new Vector3[(w) * (h)];
        Vector2[] uv = new Vector2[(w) * (h)];

        int triInd = 0;
        for (int j = 0; j < h; j++)
        {
            for (int i = 0; i < w; i++)
            {
                int ind = i + j * w;

                vertices[ind] = GetPos(i, j);
                normals[ind] = -Vector3.forward;
                uv[ind] = new Vector2((i * 1.0f) / (w - 1), 1.0f - (j * 1.0f) / (h - 1));

                if (j < height && i < width)
                {
                    tri[triInd * 6 + 0] = i + j * w;
                    tri[triInd * 6 + 1] = (i + 1) + j * w;
                    tri[triInd * 6 + 2] = (i + 1) + (j + 1) * w;

                    tri[triInd * 6 + 3] = i + j * w;
                    tri[triInd * 6 + 4] = (i + 1) + (j + 1) * w;
                    tri[triInd * 6 + 5] = i + (j + 1) * w;

                    triInd++;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.normals = normals;
        mesh.uv = uv;

        /*uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);*/
    }

    public Vector3 GetPos(int i, int j)
    {
        float hor = (i * 1.0f) / width;
        float ver = (j * 1.0f) / height;

        Vector3 A = positions[0] + (positions[1] - positions[0]) * hor;
        Vector3 B = positions[3] + (positions[2] - positions[3]) * hor;

        Vector3 P = A + (B - A) * ver;

        if (ProjectOnSphere)
        {
            return FindPositiontOnProjection(P, OffsetFromSphere);
        }
        else
        {
            return P;
        }
    }

    static Vector3 FindPositiontOnProjection(Vector3 pos, float offset = 0f)
    {
        Ray ray = new Ray(Vector3.zero, pos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 31))
        {
            Vector3 dir = (hit.transform.position - hit.point).normalized;
            return hit.point + dir * offset;
        }
        else
        {
            return pos;
        }
    }
}
