using UnityEngine;
using UnityEngine.UIElements;

public class BlockSelector : MonoBehaviour
{
    public Material mat;
    private RaycastHit hit;

    bool hasHit = true;
    Vector3[] points = new Vector3[8];

    static float epsilon = 1e-3f;

    private void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out hit))
        {
            var position = hit.point + hit.normal * -.5f;
            var blockPos = PlanetMath.PointToChunkCoords(position);

            int x = blockPos.blockX;
            int y = blockPos.blockY;
            int z = blockPos.blockZ;

            float[] offsets = new float[6];
            var chunk = hit.transform.GetComponent<Chunk>();
            if (chunk != null)
            {
                var world = chunk.world;

                offsets[0] = world.Block(x + 1, y, z, World.Space.Region) == 0 ? epsilon : -epsilon;
                offsets[1] = world.Block(x, y + 1, z, World.Space.Region) == 0 ? epsilon : -epsilon;
                offsets[2] = world.Block(x, y, z + 1, World.Space.Region) == 0 ? epsilon : -epsilon;
                offsets[3] = world.Block(x - 1, y, z, World.Space.Region) == 0 ? -epsilon : epsilon;
                offsets[4] = world.Block(x, y - 1, z, World.Space.Region) == 0 ? -epsilon : epsilon;
                offsets[5] = world.Block(x, y, z - 1, World.Space.Region) == 0 ? -epsilon : epsilon;
            }

            points[0] = PlanetMath.RegionToSphere(new Vector3(x + offsets[3], y + offsets[4], z + offsets[5]), blockPos.region);
            points[1] = PlanetMath.RegionToSphere(new Vector3(x + 1 + offsets[0], y + offsets[4], z + offsets[5]), blockPos.region);
            points[2] = PlanetMath.RegionToSphere(new Vector3(x + 1 + offsets[0], y + offsets[4], z + 1 + offsets[2]), blockPos.region);
            points[3] = PlanetMath.RegionToSphere(new Vector3(x + offsets[3], y + offsets[4], z + 1 + offsets[2]), blockPos.region);

            points[4] = PlanetMath.RegionToSphere(new Vector3(x + offsets[3], y + 1 + offsets[1], z + offsets[5]), blockPos.region);
            points[5] = PlanetMath.RegionToSphere(new Vector3(x + 1 + offsets[0], y + 1 + offsets[1], z + offsets[5]), blockPos.region);
            points[6] = PlanetMath.RegionToSphere(new Vector3(x + 1 + offsets[0], y + 1 + offsets[1], z + 1 + offsets[2]), blockPos.region);
            points[7] = PlanetMath.RegionToSphere(new Vector3(x + offsets[3], y + 1 + offsets[1], z + 1 + offsets[2]), blockPos.region);

            hasHit = true;
        } else hasHit = false;
    }

    private void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        if (hasHit)
        {
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadIdentity();
            GL.MultMatrix(Camera.main.worldToCameraMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            DrawLine(points[0], points[1]); DrawLine(points[1], points[2]); DrawLine(points[2], points[3]); DrawLine(points[3], points[0]);
            DrawLine(points[4], points[5]); DrawLine(points[5], points[6]); DrawLine(points[6], points[7]); DrawLine(points[7], points[4]);
            DrawLine(points[0], points[4]); DrawLine(points[1], points[5]); DrawLine(points[2], points[6]); DrawLine(points[3], points[7]);

            GL.End();
            GL.PopMatrix();
        }
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        GL.Vertex(start); // + transform.forward * -epsilon);
        GL.Vertex(end); // + transform.forward * -epsilon);
    }
}